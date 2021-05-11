using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoicingLibrary.Utils
{
    public class ValidateCertificate
    {
        public static bool Validate(string PassKey, string KeyFile)
        {

            try

            {
                System.Security.SecureString secPassPhrase = new System.Security.SecureString();
                foreach (char passChar in PassKey.ToCharArray())
                    secPassPhrase.AppendChar(passChar);

                byte[] privateKey = Convert.FromBase64String(KeyFile);
                var rsaCertificate = Utils.SSLKey.DecodeEncryptedPrivateKeyInfo(privateKey, secPassPhrase);
                if (rsaCertificate == null)
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;

        }
    }
}
