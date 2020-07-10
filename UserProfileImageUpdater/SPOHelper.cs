using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.UserProfiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security;
using UserProfileImageUpdater.UPSvc;
using static UserProfileImageUpdater.Logger;
using static UserProfileImageUpdater.Helper;

namespace UserProfileImageUpdater
{
    class SPOHelper
    {
        internal static void SetSingleProfileProperty(UserProfileService userProfileService, string UserName, string PropertyName, string PropertyValue)
        {
            try
            {
                PropertyData[] data = new PropertyData[1];
                data[0] = new PropertyData
                {
                    Name = PropertyName,
                    IsValueChanged = true,
                    Values = new ValueData[1]
                };
                data[0].Values[0] = new ValueData
                {
                    Value = PropertyValue
                };
                userProfileService.ModifyUserPropertyByAccountName(UserName, data);
            }
            catch (Exception ex)
            {
                LogMessage("Exception trying to update profile property " + PropertyName + " for user " + UserName + "\n" + ex.Message, EventLogEntryType.Error, ServiceEventID.ProfileImageUploaderFailure);
            }
        }

        internal static object SetMultipleProfileProperties(UserProfileService userProfileService, string UserName, string[] PropertyName, string[] PropertyValue)
        {
            LogMessage("Setting multiple SPO user profile properties for " + UserName, EventLogEntryType.Information, ServiceEventID.ProfileImageUploaderFailure);
            try
            {
                int arrayCount = PropertyName.Count();
                PropertyData[] data = new PropertyData[arrayCount];
                for (int x = 0; x < arrayCount; x++)
                {
                    data[x] = new PropertyData
                    {
                        Name = PropertyName[x],
                        IsValueChanged = true,
                        Values = new ValueData[1]
                    };
                    data[x].Values[0] = new ValueData
                    {
                        Value = PropertyValue[x]
                    };
                }
                userProfileService.ModifyUserPropertyByAccountName(UserName, data);
                return true;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        internal string GetSingleProfileProperty(ClientContext clientContext, string UserName, string PropertyName)
        {
            try
            {
                var peopleManager = new PeopleManager(clientContext);
                ClientResult<string> profileProperty = peopleManager.GetUserProfilePropertyFor(UserName, PropertyName);
                clientContext.ExecuteQuery();
                if (profileProperty.Value.Length > 0)
                {
                    return profileProperty.Value;
                }
                else
                {
                    LogMessage("Cannot find a value for property " + PropertyName + " for user " + UserName, EventLogEntryType.Information, ServiceEventID.SharePointOnlineError);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                LogMessage("User Error: Exception trying to get profile property " + PropertyName + " for user " + UserName + "\n" + ex.Message, EventLogEntryType.Error, ServiceEventID.SharePointOnlineError);
                return string.Empty;
            }
        }

        internal string[] GetMultipleProfileProperties(ClientContext clientContext, string UserName, string[] PropertyNames)
        {
            try
            {
                var peopleManager = new PeopleManager(clientContext);
                UserProfilePropertiesForUser profilePropertiesForUser = new UserProfilePropertiesForUser(clientContext, UserName, PropertyNames);
                IEnumerable<string> profilePropertyValues = peopleManager.GetUserProfilePropertiesFor(profilePropertiesForUser);
                clientContext.Load(profilePropertiesForUser);
                clientContext.ExecuteQuery();
                return profilePropertyValues.ToArray();
            }
            catch (Exception ex)
            {
                LogMessage("Exception trying to get profile properties for user " + UserName + "\n" + ex.Message, EventLogEntryType.Error, ServiceEventID.SharePointOnlineError);
                return null;
            }
        }
        
        internal static ClientContext GetClientContext(string profileSiteUrl, string sPoAuthUserName, string sPoAuthPassword)
        {
            try
            {

                LogMessage("Initializing service object for SPO Client API " + profileSiteUrl, EventLogEntryType.Information, ServiceEventID.SharePointOnlineError);
                var clientContext = new ClientContext(profileSiteUrl);
                SecureString securePassword = GetSecurePassword(sPoAuthPassword);
                clientContext.Credentials = new SharePointOnlineCredentials(sPoAuthUserName, securePassword);
                LogMessage("Finished creating service object for SPO Client API " + profileSiteUrl, EventLogEntryType.Information, ServiceEventID.SharePointOnlineError);
                return clientContext;
            }
            catch (Exception ex)
            {
                LogMessage("Error creating client context for SPO " + profileSiteUrl + " " + ex.Message, EventLogEntryType.Error, ServiceEventID.SharePointOnlineError);
                return null;
            }
        }
        
        internal static UserProfileService GetUserProfileService(string profileSiteUrl, string sPoAuthUserName, string sPoAuthPassword)
        {
            try
            {
                string webServiceExt = "_vti_bin/userprofileservice.asmx";
                string adminWebServiceUrl = string.Empty;

                adminWebServiceUrl = $"{profileSiteUrl.TrimEnd('/')}/{webServiceExt}";

                LogMessage("Initializing SPO web service " + adminWebServiceUrl, EventLogEntryType.Information, ServiceEventID.SharePointOnlineError);

                SecureString securePassword = GetSecurePassword(sPoAuthPassword);
                SharePointOnlineCredentials onlineCred = new SharePointOnlineCredentials(sPoAuthUserName, securePassword);
                string authCookie = onlineCred.GetAuthenticationCookie(new Uri(profileSiteUrl));
                CookieContainer authContainer = new CookieContainer();
                authContainer.SetCookies(new Uri(profileSiteUrl), authCookie);
                var userProfileService = new UserProfileService();
                userProfileService.Url = adminWebServiceUrl;
                userProfileService.CookieContainer = authContainer;
                return userProfileService;
            }
            catch (Exception ex)
            {
                LogMessage("Error initiating connection to profile web service in SPO " + ex.Message, EventLogEntryType.Error, ServiceEventID.SharePointOnlineError);
                return null;
            }
        }
    }
}
