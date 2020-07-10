using Microsoft.SharePoint.Client;
using System;
using System.Diagnostics;
using System.IO;
using static UserProfileImageUpdater.Logger;
using static UserProfileImageUpdater.SPOHelper;

namespace UserProfileImageUpdater
{
    public class PictureUploader
    {
        const string _sPOProfilePrefix = "i:0#.f|membership|";

        private UPSvc.UserProfileService _userProfileService;
        private ClientContext _clientContext;
        private string _profilePhotoPath;
        public PictureUploader(string profileSiteUrl, string mySiteUrl, string sPoAuthUsername, string sPoAuthPasword, string profilePhotoPath)
        {
            _clientContext = GetClientContext(mySiteUrl, sPoAuthUsername, sPoAuthPasword);
            _userProfileService = GetUserProfileService(profileSiteUrl, sPoAuthUsername, sPoAuthPasword);
            _profilePhotoPath = string.Concat(profilePhotoPath.TrimEnd('/'), "/{0}_{1}Thumb.jpg");
        }

        public object UploadPicture(string sPoUserProfileName, Stream image, string imageSizeSuffix, int imageSize)
        {
            LogMessage($"Begin processing for user {sPoUserProfileName}", EventLogEntryType.Information, ServiceEventID.SharePointOnlineError);

            var profileImage = new ProfileImage(sPoUserProfileName, image);
            try
            {
                return profileImage.UploadToSPO(_clientContext, _profilePhotoPath, imageSizeSuffix, imageSize);
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public object SetUserProfileProperties(string sPoUserProfileName, string profileImagePath)
        {
            string[] profilePropertyNamesToSet = new string[] { "PictureURL", "SPS-PicturePlaceholderState" };
            string[] profilePropertyValuesToSet = new string[] { profileImagePath, "0" };
            return SetMultipleProfileProperties(_userProfileService, _sPOProfilePrefix + sPoUserProfileName, profilePropertyNamesToSet, profilePropertyValuesToSet);
        }
    }
}
