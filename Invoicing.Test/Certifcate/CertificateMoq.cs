using System;
using System.Text;
using Invoicing.Interfaces;
using System.IO;
using System.Security.Cryptography.X509Certificates;
namespace Invoicing.Test.Certifcate
{
    public class CertificateMoq : ICertificate
    {

        
        private void Initialize ()
        {
           var pathCer =   @"..\\..\\Resources\\CSD_Pruebas_CFDI_LAN7008173R5.cer";
           var pathKey =   @"..\\..\\Resources\\CSD_Pruebas_CFDI_LAN7008173R5.key";
            X509Certificate x509Certificate = new X509Certificate(pathCer);
              
            CerFile = Convert.ToBase64String(x509Certificate.GetPublicKey());
            byte[] serialNumber = x509Certificate.GetSerialNumber();
            Array.Reverse(serialNumber);
            NoCertificate = Encoding.UTF8.GetString(serialNumber);
            //_controllerComprobante.CFDIComprobante.certificado = x509Certificate.GetPublicKeyString();
            Pwd = "12345678a";
            //noCertificado = "20001000000200000258";
            var bytesFile = File.ReadAllBytes(pathKey);
            KeyFile = Convert.ToBase64String( File.ReadAllBytes(pathKey));
        }
     
        public CertificateMoq()
        {
            Initialize();
        }
        public string CerFile
        {
            get;
            set;            
        }

        public DateTime ValidFrom
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int idCertificate
        {
            get;
            set;
        }

        public string KeyFile
        {
            get;
            set;
        }

        public string NoCertificate
        {
            get;
            set;
        }

        public string Pwd
        {
            get;
            set;
        }

        public DateTime ValidUntil
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
