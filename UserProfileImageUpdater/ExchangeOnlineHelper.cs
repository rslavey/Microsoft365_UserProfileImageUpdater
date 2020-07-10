using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using static UserProfileImageUpdater.Helper;

namespace UserProfileImageUpdater
{
    class ExchangeOnlineHelper
    {
        public class ExchangeOnlineSetPhoto
        {
            private static PSCredential _creds;
            public ExchangeOnlineSetPhoto(string username, string password)
            {
                _creds = new PSCredential(username, GetSecurePassword(password));
            }

            public object SetPhoto(string email, string photo, int attemptCount = 0)
            {
                Runspace runspace = RunspaceFactory.CreateRunspace();
                runspace.Open();
                var ps = PowerShell.Create();
                try
                {
                    string connectionUri = Properties.Settings.Default.ExchangeOnlinePSConnectionUri;
                    PSCommand command = new PSCommand();
                    command.AddCommand("New-PSSession");
                    command.AddParameter("ConfigurationName", "Microsoft.Exchange");
                    command.AddParameter("ConnectionUri", new Uri(connectionUri));
                    command.AddParameter("Credential", _creds);
                    command.AddParameter("Authentication", "Basic");
                    ps.Commands = command;
                    ps.Runspace = runspace;
                    Collection<PSObject> result = ps.Invoke();
                    if (ps.Streams.Error.Count > 0 || result.Count != 1)
                    {
                        throw ps.Streams.Error[0].Exception;
                    }
                    ps = PowerShell.Create();
                    command = new PSCommand();
                    command.AddCommand("Invoke-Command");
                    command.AddParameter("ScriptBlock", ScriptBlock.Create($"Set-UserPhoto -Identity \"{email}\" -PictureData ([System.IO.File]::ReadAllBytes(\"{photo}\")) -Confirm:$False"));
                    command.AddParameter("Session", result[0]);
                    ps.Commands = command;
                    ps.Runspace = runspace;
                    var results = ps.Invoke();
                    if (ps.Streams.Error.Count > 0)
                    {
                        //Try a few times. Exchange Online is finicky.
                        if (attemptCount > 2)
                        {
                            throw ps.Streams.Error[0].Exception;
                        }
                        else
                        {
                            SetPhoto(email, photo, attemptCount++);
                        }
                    }
                    return results;
                }
                catch (Exception ex)
                {
                    return ex;
                }
                finally
                {
                    runspace.Dispose();
                    ps.Dispose();
                }
            }
        }

    }
}
