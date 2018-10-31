using System;
using System.Text;
using System.Security.Cryptography;

namespace ProcessCFDI.Utils
{
    public class Security
    {
        public static string Hash(string ToHash)
        {
            // Use UTF8 encoder
            Encoder enc = System.Text.Encoding.UTF8.GetEncoder();

            // Create a buffer and convert
            byte[] data = new byte[ToHash.Length];
            enc.GetBytes(ToHash.ToCharArray(), 0, ToHash.Length, data, 0, true);

            // Implementation the SHA1 compute
            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                byte[] result = sha1.ComputeHash(data);
                return BitConverter.ToString(result).Replace("-", "").ToLower();
            }
        }
    }
}
