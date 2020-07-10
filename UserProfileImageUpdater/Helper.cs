using System.Security;

namespace UserProfileImageUpdater
{
    class Helper
    {
        internal static SecureString GetSecurePassword(string Password)
        {
            SecureString sPassword = new SecureString();
            foreach (char c in Password.ToCharArray()) sPassword.AppendChar(c);
            return sPassword;
        }
    }
}
