using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chilkat;
using Invoicing.Interfaces;

namespace Invoicing.Test.SealChilkat
{
    public class SealerChilkat
    {
        ICertificatesRepository _certRepository;
        public SealerChilkat(ICertificatesRepository certificatesRepository)
        {
            _certRepository = certificatesRepository;
        }

        public string Sellado(string CadenaOriginal)
        {

            bool cargaexitosa = false;
            Chilkat.PrivateKey Pkey = new PrivateKey();
            Chilkat.Rsa loRsa = new Rsa();
            Chilkat.Cert Cert = new Chilkat.Cert();
            var base64cert = _certRepository.GetCertificate("00000000000000");

            cargaexitosa = Cert.LoadFromBase64(base64cert.CerFile);

            byte[] llave = System.Convert.FromBase64String(base64cert.KeyFile);
            cargaexitosa = Pkey.LoadPkcs8Encrypted(llave, base64cert.Pwd);

            cargaexitosa = loRsa.UnlockComponent("ECODEXRSA_D5WpF9zunPvz");
            if (!cargaexitosa)
            {
                return string.Format("0", loRsa.LastErrorText);
                // throw new Exception(33, string.Format("0", loRsa.LastErrorText));
            }

            if (loRsa.ImportPublicKey(Pkey.GetXml()) == false)
            {
                return string.Format("Error al obtener la llave publica.");
                //throw new Exception(34, string.Format("Error al obtener la llave publica."));
            }
            loRsa.LittleEndian = false;
            loRsa.Charset = "utf-8";
            loRsa.EncodingMode = "base64";

            string sello = loRsa.SignStringENC(CadenaOriginal, "sha-1");
            if (!string.IsNullOrEmpty(sello))
            {
                return sello;
                //SelloInf.Certificado = certi.CertificadoB64;
            }
            else
                return "Error en el sellado";
        }

    }
}
