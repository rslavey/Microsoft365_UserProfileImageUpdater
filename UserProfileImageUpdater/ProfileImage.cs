using Microsoft.SharePoint.Client;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using static UserProfileImageUpdater.Logger;

namespace UserProfileImageUpdater
{
    class ProfileImage
    {
        private Stream _imageStream;
        private string _imageName;

        public ProfileImage (string imageName, Stream imageStream)
        {
            _imageStream = imageStream;
            _imageName = imageName;
        }

        public object UploadToSPO(ClientContext clientContext, string spPhotoPathTemplate, string sizeSuffix, int size)
        {
            try
            {
                LogMessage($"Uploading {sizeSuffix}({size}px) image to SPO", EventLogEntryType.Information, ServiceEventID.SharePointOnlineError);

                using (Stream img = ResizeImage(_imageStream, size))
                {
                    if (img != null)
                    {
                        var spImageUrl = string.Format(spPhotoPathTemplate, _imageName, sizeSuffix);
                        if (clientContext.HasPendingRequest)
                        {
                            clientContext.ExecuteQuery();
                        }
                        Microsoft.SharePoint.Client.File.SaveBinaryDirect(clientContext, spImageUrl, img, true);
                        return spImageUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }

        private static Stream ResizeImage(Stream OriginalImage, int NewWidth)
        {
            OriginalImage.Seek(0, SeekOrigin.Begin);
            MemoryStream memStream = new MemoryStream();
            using (Image originalImage = Image.FromStream(OriginalImage, true, true))
            {
                if (originalImage.Width <= NewWidth)
                {
                    OriginalImage.Seek(0, SeekOrigin.Begin);
                    originalImage.Dispose();
                    return OriginalImage;
                }

                int newHeight = (NewWidth * originalImage.Height) / originalImage.Width;

                Bitmap newImage = new Bitmap(NewWidth, newHeight);

                using (Graphics gr = Graphics.FromImage(newImage))
                {
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gr.DrawImage(originalImage, new Rectangle(0, 0, NewWidth, newHeight));
                }

                newImage.Save(memStream, ImageFormat.Jpeg);
            }
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }
    }
}
