# Microsoft 365 User Profile Image Updater
> Centrally manage employee profile images for Microsoft 365 products such as SharePoint Online, Exchange and Outlook, Skype, Delve, etc.

Microsoft 365 allows individuals to update their profile photos, but there is no way to centrally manage these photos. This Windows Service will collect employee profile photos from a folder and update the employee's profile on Microsoft 365.

## Configuration

### Modify the app.config file in the UserProfileImageUpdater project

The app.config file contains a number of settings that will need to be configured for your environment.

- *SPOUsername*: User with access to update profile images (needs to be assigned to the roles "SharePoint admin" and "User admin" in Microsoft 365)
- *ProfileImageInterval*: How often the service checks for updated images (in seconds)
- *ImageProfilePath*: The path to the user images (the Windows Service account will need access to this location)
- *ProfileSiteUrl*: The SharePoint Admin Site url (e.g. https://mydomain-admin.sharepoint.com/)
- *MySiteUrl*: The Sharepoint My Site url (e.g. https://mydomain-my.sharepoint.com)
- *SmallImageSize*: Image size for small images (should not be changed unless Microsoft changes this in the future)
- *MediumImageSize*: Image size for medium images (should not be changed unless Microsoft changes this in the future)
- *LargeImageSize*: Image size for large images (should not be changed unless Microsoft changes this in the future)
- *SPOEncryptedPassword*: See the next section
- *LoggingLevel*: How much detail is saved to the Event Viewer (options are Verbose, Warning, Error)
- *ProfileImageModDates*: A text file where changes will be tracked, used for persistence in case service is stopped.
- *SPOProfilePhotoPath*: The path to user profile photos on SharePoint My Site (should not be changed unless Microsoft changes this in the future)
- *EventLogName*: The Event Log where logs are stored
- *EventLogSource*: The name of the events
- *UserProfileUpdater_UPSvc_UserProfileService*: The path to the SharePoint web service for user profiles (should not be changed unless Microsoft changes this in the future)
- *ExchangeOnlinePSConnectionUri*: The path to the Exchange Online Powershell  (should not be changed unless Microsoft changes this in the future)
- *EncryptionSalt*: A random string of text of your choosing
- *O365AccountEmailSuffix*: The email suffix for your tenant (e.g. @mydomain.com)
- *O365AccountEmailSuffixFilenameSafe*: The format Microsoft uses to save files with the email domain (e.g. _mydomain_com)

### Generate an encrypted password for your Microsoft365 service account

To update user profiles, you will need an account that has access to do so. This account needs to be assigned to the roles "SharePoint admin" and "User admin" in Microsoft 365.

- Compile the GenerateEncryptedPassword project and run the executable on the machine where the Windows Service will be installed
- Choose a salt and enter it (keep track of this)
- Enter the user password

The encrypted password will be saved to the clipboard. Paste this into the app.config file setting "SPOEncryptedPassword". Also update the EncryptionSalt setting with the salt you used.

## Installation

See https://docs.microsoft.com/en-us/dotnet/framework/windows-services/how-to-install-and-uninstall-services for details on installing Windows Services.

## Image Guidelines

Save user profile images in the folder specified by the app.config setting "ImageProfilePath". Images should be at least 800px along the shortest side and saved as .jpg file. Save the file with the format `user_one_mydomain_com.jpg` where the user's Microsoft 365 account is user.one@mydomain.com.

## Release History

* 1.0.0
    * Release

<!-- Markdown link & img dfn's -->
[wiki]: https://github.com/rslavey/Microsoft365_UserProfileImageUpdater/wiki
