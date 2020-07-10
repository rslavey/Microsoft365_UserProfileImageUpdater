using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace GenerateEncryptedPassword
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.Write("Salt: ");
            var salt = Encoding.Unicode.GetBytes(Console.ReadLine());
            Console.Write("Password: ");
            var password = Console.ReadLine();
            Clipboard.SetText(EncryptString(password, salt));
            Console.WriteLine();
            Console.WriteLine("Encrypted password has been saved to the clipboard. Press Enter to quit.");
            Console.ReadLine();
        }

        public static string EncryptString(string input, byte[] salt)
        {
            byte[] encryptedData = ProtectedData.Protect(
                Encoding.Unicode.GetBytes(input),
                salt,
                DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(encryptedData);
        }
    }
}
