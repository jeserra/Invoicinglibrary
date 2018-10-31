using System;
using System.Text;
using Invoicing.Utils;
using System.Xml.Serialization;
using Invoicing.Interfaces;
using System.IO;

namespace Invoicing.CFDI
{
    public class CFDIv33:CFDIBase
    {
        public CFDIv33(ICertificatesRepository certificatesRepository, ISATProvider satProvider) : base(certificatesRepository, satProvider)
        {
            
        }

        public string GetXML(cfdi33.Comprobante comprobante)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            // ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            
            ns.Add("Pagos", "http://www.sat.gob.mx/Pagos");
            ns.Add("ValesDeDespensa", "http://www.sat.gob.mx/valesdedespensa");
            //      ns.Add("ecb", "http://www.sat.gob.mx/ecb");


            //     ns.Add("valesdedespensa", "http://www.sat.gob.mx/valesdedespensa");

            return XmlSerializerHelper.ToXmlString<cfdi33.Comprobante>(comprobante, ns, Encoding.UTF8);
        }

        public  string CreateCFDI(Invoicing.BindingModels.Comprobante apiComprobante, bool Timbrado = true)
        {
            try
            {
                long transaccion = 100;
                var comprobante = Translates.TranslateModelToCFDI.TranslateToCFDI(apiComprobante);
                comprobante.Certificado = CertificatesRepository.GetCertificate(comprobante.NoCertificado).CerFile;
                var xmlComprobante = GetXML(comprobante);
                comprobante.Sello = SetSeal(comprobante, xmlComprobante, comprobante.NoCertificado);
                var xmlComprobanteSellado = GetXML(comprobante);

                if (!Timbrado)
                    return xmlComprobante;
                else
                {
                    return Timbrar(apiComprobante, xmlComprobanteSellado, transaccion);
                }
              
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string Timbrar(Invoicing.BindingModels.Comprobante apiComprobante, string xmlComprobanteSellado, long transaccion)
        {
            try
            {  
                var xmlTimbrado = SatProvider.Timbrar(apiComprobante.Emisor.RFC, xmlComprobanteSellado, transaccion);
                var timbrado = UtilTimbrado.ObtenerDatosTimbrado(xmlTimbrado);
                apiComprobante.UUID = Guid.Parse(timbrado.UUID);
                apiComprobante.FechaTimbrado = timbrado.FechaTimbrado;
                return xmlTimbrado;
            }
            catch (Exception ex)
            {
                throw;
                //Console.WriteLine(ex.Message);
                // Escribir en el log el error del timbrado y avisar al usuario
                //return xmlComprobanteSellado;
            }
        }

        public string Timbrar(string RFCEmisor, string xmlComprobanteSellado, long transaccion)
        {
            try
            {
                var xmlTimbrado = SatProvider.Timbrar(RFCEmisor, xmlComprobanteSellado, transaccion);
                var timbrado = UtilTimbrado.ObtenerDatosTimbrado(xmlTimbrado);
                
                return xmlTimbrado;
            }
            catch (Exception ex)
            {
                throw;
                //Console.WriteLine(ex.Message);
                // Escribir en el log el error del timbrado y avisar al usuario
                //return xmlComprobanteSellado;
            }
        }
        public string AgregarConfirmacion (string xml,  string confirmacion)
        {
            // TODO: Agregar lectura del string a stream para deserializar
            //XmlSerializerHelper.Deserialize<Comprobante>();
            throw new System.NotImplementedException();
        }

        public cfdi33.Comprobante DeserializeXML( string xmlCFDIFile)
        {

            //load default template
            //return XmlSerializerHelper.Deserialize<cfdi33.Comprobante>(xmlCFDIFile);
            StringReader reader = new StringReader(xmlCFDIFile);
            return XmlSerializerHelper.Deserialize<cfdi33.Comprobante>(reader);
        }
    }
}
