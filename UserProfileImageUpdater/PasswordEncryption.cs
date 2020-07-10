using System;
using System.Diagnostics;
using System.Text;
using static UserProfileImageUpdater.Logger;

namespace UserProfileImageUpdater
{
    public class PasswordEncryption
    {
        static byte[] entropy = Encoding.Unicode.GetBytes(Properties.Settings.Default.EncryptionSalt);

        public static string DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.LocalMachine);
                return Encoding.Unicode.GetString(decryptedData);
            }
            catch (Exception ex)
            {
                LogMessage($"Error decrypting password: {ex.Message}", EventLogEntryType.Warning, ServiceEventID.EncryptionError);
                return string.Empty;
            }
        }
    }
}
