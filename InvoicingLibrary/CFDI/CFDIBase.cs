using System;
using System.Text;
using System.IO;
using InvoicingLibrary.Interfaces;
using System.Xml.Xsl;
using System.Xml;

namespace InvoicingLibrary.CFDI
{
    public class CFDIBase
    {
        internal ICertificatesRepository CertificatesRepository;
        internal ISATProvider SatProvider;

        public CFDIBase(ICertificatesRepository certificatesRepository,  ISATProvider satProvider)
        {
            CertificatesRepository = certificatesRepository;
            SatProvider = satProvider;
        }
         
        private const string xsltPath = "http://www.sat.gob.mx/sitio_internet/cfd/3/cadenaoriginal_3_3/cadenaoriginal_3_3.xslt";
         public string XMLToString(System.Xml.XmlDocument xmlDoc)
        {
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter sw = new System.IO.StringWriter(sb);
            xmlDoc.Save(sw);
            return sw.ToString();
        }

        public string GetOriginalChain(string stringXML)
        {
            StringWriter sw = new StringWriter();

            try
            {
                XslCompiledTransform xslt = new System.Xml.Xsl.XslCompiledTransform();
                XsltSettings sets = new XsltSettings(true, true);
                var resolver = new XmlUrlResolver();
                xslt.Load(xsltPath, sets, resolver);

                XmlDocument FromXmlFile = new System.Xml.XmlDocument();
                FromXmlFile.LoadXml(stringXML);

                xslt.Transform(FromXmlFile, null, sw);
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo generar la cadena", ex.InnerException);
            }
            return sw.ToString();
        }

        public byte[] GetSHA1(string OriginalChain)
        {
            System.Security.Cryptography.SHA1CryptoServiceProvider cryptoTransformSHA1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            return cryptoTransformSHA1.ComputeHash(Encoding.UTF8.GetBytes(OriginalChain));
        }

        public byte[] GetSHA256(string OriginalChain)
        {
            System.Security.Cryptography.SHA256CryptoServiceProvider cryptoTransformSHA1 = new System.Security.Cryptography.SHA256CryptoServiceProvider();
            return cryptoTransformSHA1.ComputeHash(Encoding.UTF8.GetBytes(OriginalChain));
        }
        public string SetSeal(cfdi33.Comprobante CFDIComprobante, string TheXML, string noCertificado)
        {

            ICertificate certificate = CertificatesRepository.GetCertificate(noCertificado);
            string OriginalChain = GetOriginalChain(TheXML);

            byte[] SHA256hash = GetSHA256(OriginalChain);

            string PassKey = certificate.Pwd;
            System.Security.SecureString secPassPhrase = new System.Security.SecureString();
            foreach (char passChar in PassKey.ToCharArray())
                secPassPhrase.AppendChar(passChar);

            System.Security.Cryptography.RSACryptoServiceProvider privateKey = LoadPrivateKeyFromString( secPassPhrase, certificate.KeyFile);

            return   GetSeal(SHA256hash, privateKey);
        }

        public string GetSeal(byte[] rgbHash, System.Security.Cryptography.RSACryptoServiceProvider privateKey)
        {
            System.Security.Cryptography.RSAPKCS1SignatureFormatter rsaPKCS1 = new System.Security.Cryptography.RSAPKCS1SignatureFormatter(privateKey);
            rsaPKCS1.SetHashAlgorithm("SHA256");

            return Convert.ToBase64String(rsaPKCS1.CreateSignature(rgbHash));
        }

        public System.Security.Cryptography.RSACryptoServiceProvider LoadPrivateKeyFromString( System.Security.SecureString secPassPhrase, string keyFile)
        { 
            byte[] privateKey = Convert.FromBase64String(keyFile);
            return Utils.SSLKey.DecodeEncryptedPrivateKeyInfo(privateKey, secPassPhrase);
        }

        #region image
        public byte[] imageToByte(System.Drawing.Bitmap img)
        {
            MemoryStream stream = new MemoryStream();
            img.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            Byte[] bytes = stream.ToArray();

            return bytes;
        }

        private byte[] BmpToBytes_MemStream(System.Drawing.Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            // Save to memory using the Jpeg format
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            // read to end
            byte[] bmpBytes = ms.GetBuffer();
            bmp.Dispose();
            ms.Close();

            return bmpBytes;
        }
        #endregion

        

        
    }
}
