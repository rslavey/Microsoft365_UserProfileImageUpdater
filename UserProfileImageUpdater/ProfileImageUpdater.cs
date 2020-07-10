using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static UserProfileImageUpdater.ExchangeOnlineHelper;

namespace UserProfileImageUpdater
{
    class ProfileImageUpdater : IUpdaterTask
    {
        public DateTime LastChecked { get; set; }
        public TimeSpan CheckInterval { get; set; }
        private static string _imageFolderPath;
        private PictureUploader _pictureUploader;
        private ExchangeOnlineSetPhoto _exchangeOnlineSetPhoto;
        private Dictionary<string, DateTime> _lastModifiedDates = new Dictionary<string, DateTime>();
        bool Running { get; set; }

        public ProfileImageUpdater(TimeSpan checkInterval, string imageFolderPath)
        {
            LastChecked = DateTime.MinValue;
            CheckInterval = checkInterval;
            _imageFolderPath = imageFolderPath;
            _pictureUploader = new PictureUploader(Properties.Settings.Default.ProfileSiteUrl
                        , Properties.Settings.Default.MySiteUrl
                        , Properties.Settings.Default.SPOUsername
                        , PasswordEncryption.DecryptString(Properties.Settings.Default.SPOEncryptedPassword)
                        , Properties.Settings.Default.SPOProfilePhotoPath
                        );
            _exchangeOnlineSetPhoto = new ExchangeOnlineSetPhoto(Properties.Settings.Default.SPOUsername, PasswordEncryption.DecryptString(Properties.Settings.Default.SPOEncryptedPassword));
            Running = true;
            if (!File.Exists(Properties.Settings.Default.ProfileImageModDates))
            {
                try
                {
                    foreach (var file in Directory.GetFiles(_imageFolderPath).Where(x => Path.GetExtension(x).ToLower() == ".jpg"))
                    {
                        _lastModifiedDates.Add(Path.GetFileName(file), File.GetLastWriteTime(file));
                    }
                    File.WriteAllText(Properties.Settings.Default.ProfileImageModDates, JsonConvert.SerializeObject(_lastModifiedDates));
                }
                catch (Exception ex)
                {
                    Logger.LogMessage($"Error writing mod dates file: {ex.Message}", EventLogEntryType.Error, Logger.ServiceEventID.LocalFileSystemError);
                }
            }
            _lastModifiedDates = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(File.ReadAllText(Properties.Settings.Default.ProfileImageModDates));
        }

        public async Task Check()
        {
            while (Running)
            {
                if (DateTime.Now - LastChecked > CheckInterval)
                {
                    Logger.LogMessage($"Checking ProfileImages", EventLogEntryType.Information, Logger.ServiceEventID.StatusMessage);
                    Task<bool> updateResult = this.Update();
                    await updateResult;
                    if (updateResult.Result)
                    {
                        LastChecked = DateTime.Now;
                    }
                    else
                    {
                        Logger.LogMessage("ProfileImageUpdater Failed", EventLogEntryType.Error, Logger.ServiceEventID.ProfileImageUploaderFailure);
                    }
                }
                else
                {
                    await Task.Delay(CheckInterval);
                }
            }

        }

        public async Task<bool> Update()
        {
            try
            {
                foreach (var file in Directory.GetFiles(_imageFolderPath).Where(x => Path.GetExtension(x).ToLower() == ".jpg"))
                {
                    var fileLastModified = File.GetLastWriteTime(file);
                    var fileName = Path.GetFileName(file);
                    var lastModifiedDateEntry = _lastModifiedDates.FirstOrDefault(x => x.Key == fileName);

                    if (lastModifiedDateEntry.Equals(default(KeyValuePair<string, DateTime>)))
                    {
                        _lastModifiedDates.Add(fileName, fileLastModified);
                        File.WriteAllText(Properties.Settings.Default.ProfileImageModDates, JsonConvert.SerializeObject(_lastModifiedDates));
                    }
                    else if (lastModifiedDateEntry.Value < fileLastModified)
                    {
                        _lastModifiedDates.Remove(fileName);
                        _lastModifiedDates.Add(fileName, fileLastModified);
                        File.WriteAllText(Properties.Settings.Default.ProfileImageModDates, JsonConvert.SerializeObject(_lastModifiedDates));
                    }
                    else
                    {
                        continue;
                    }

                    Logger.LogMessage($"Image {file} has been updated.", EventLogEntryType.Information, Logger.ServiceEventID.StatusMessage);
                    var fileStream = new MemoryStream(File.ReadAllBytes(file));

                    var fileNameNoExt = Path.GetFileNameWithoutExtension(file);
                    var email = fileNameNoExt.Replace(Properties.Settings.Default.O365AccountEmailSuffixFilenameSafe, Properties.Settings.Default.O365AccountEmailSuffix).Replace('_', '.');

                    var smallImageUpload = _pictureUploader.UploadPicture(fileNameNoExt, fileStream, "S", Properties.Settings.Default.SmallImageSize);
                    var mediumImageUpload = _pictureUploader.UploadPicture(fileNameNoExt, fileStream, "M", Properties.Settings.Default.MediumImageSize);
                    var largeImageUpload = _pictureUploader.UploadPicture(fileNameNoExt, fileStream, "L", Properties.Settings.Default.LargeImageSize);

                    if (mediumImageUpload is string && !string.IsNullOrEmpty((string)mediumImageUpload))
                    {
                        _pictureUploader.SetUserProfileProperties(email, $"{Properties.Settings.Default.MySiteUrl}{mediumImageUpload}");
                        Logger.LogMessage($"ImageUpload successful for {email}", EventLogEntryType.Information, Logger.ServiceEventID.StatusMessage);
                    }
                    else
                    {
                        Logger.LogMessage($"ImageUpload Failed for {email}: {((Exception)mediumImageUpload).Message}", EventLogEntryType.Error, Logger.ServiceEventID.ProfileImageUploaderFailure);
                    }
                    var setExchangePhoto = _exchangeOnlineSetPhoto.SetPhoto(email, file);
                    if (setExchangePhoto is Exception)
                    {
                        Logger.LogMessage($"ExchangeOnline Set Photo failed for {email}: {((Exception)setExchangePhoto).Message}", EventLogEntryType.Error, Logger.ServiceEventID.ExchangeOnlineUpdateError);
                    }
                    else
                    {
                        var sb = new StringBuilder();
                        foreach (PSObject obj in (Collection<PSObject>)setExchangePhoto)
                        {
                            sb.AppendLine(obj.ToString());
                        }
                        Logger.LogMessage($"ExchangeOnline Set Photo results for {email}: {sb}", EventLogEntryType.Information, Logger.ServiceEventID.StatusMessage);
                    }

                    //Pause 10 seconds in order to avoid Exchange Online throttling
                    Thread.Sleep(10000);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"ProfileImageUpdater Failed: {ex.Message}", EventLogEntryType.Error, Logger.ServiceEventID.ProfileImageUploaderFailure);
                return false;
            }
        }
    }
}
