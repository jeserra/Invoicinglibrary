using InvoicingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;
using Invo.src.Timbrado._2011.CFDI;
using In.src.Cliente._2011.CFDI;
using Inv.src.Seguridad._2011.CFDI;
using System.ServiceModel;
using ProcessCFDI.Utils;

namespace InvoicingLibrary.CFDIProviders
{
    class EcodexProvider : ISATProvider
    {
        public string INTEGRATOR_ID
        {
            get => ConfigurationManager.AppSettings["INTEGRATOR_ID"].ToString();
            
        }
        public async Task<byte[]> ObtenerQR(string RFC, String UUID, long transactionId)
        {
            try
            {
                TimbradoClient client = new TimbradoClient();
                SolicitudObtenerQRTimbrado solicitud = new SolicitudObtenerQRTimbrado(RFC, ObtenerToken(RFC, transactionId),  transactionId, UUID);
                var response = await client.ObtenerQRTimbradoAsync(solicitud);
               
                return response.QR.Imagen;
            }
            catch (FaultException<Invo.src.Timbrado._2011.CFDI.FallaServicio> serviceFault)
            {
                throw new Exception(String.Format($"Error al timbrar  {serviceFault.Message} { serviceFault.Detail.Numero}"));
            }
            catch (FaultException<Invo.src.Timbrado._2011.CFDI.FallaSesion> serviceSesion)
            {
                throw new Exception(String.Format($"Error al timbrar { serviceSesion.Message}"));
            }
            catch (FaultException<Invo.src.Timbrado._2011.CFDI.FallaValidacion> faultvalidation)
            {
                throw new Exception(String.Format($"Error al timbrar {faultvalidation.Message}"));
            }
        }

        public String Timbrar(String RFC, String Comprobante, long transactionId)
        {
            try
            {
                TimbradoClient client = new TimbradoClient();
               ComprobanteXML commprobante = new ComprobanteXML();
                SolicitudTimbraXML solicitud = new SolicitudTimbraXML(commprobante, RFC, ObtenerToken(RFC, transactionId), transactionId);
                commprobante.DatosXML = Comprobante;
                var timbrado = client.TimbraXMLAsync(solicitud);
                return commprobante.DatosXML;
            }
            catch (FaultException<Invo.src.Timbrado._2011.CFDI.FallaServicio> serviceFault)
            {
                throw new Exception(String.Format("Error al timbrar  {0} {1}", serviceFault.Message, serviceFault.Detail.Numero));

            }
            catch (FaultException<Invo.src.Timbrado._2011.CFDI.FallaSesion> serviceSesion)
            {
                throw new Exception(String.Format("Error al timbrar {0}", serviceSesion.Message));
            }
            catch (FaultException<Invo.src.Timbrado._2011.CFDI.FallaValidacion> faultvalidation)
            {
                throw new Exception(String.Format("Error al timbrar {0}", faultvalidation.Message));
            }
        }

        public string ObtenerToken(string RFC, long transactionID)
        {
            SeguridadClient srv = new SeguridadClient();
            SolicitudObtenerToken solicitud = new SolicitudObtenerToken(RFC, transactionID);
            var serviceToken = srv.ObtenerTokenAsync(solicitud);

            return ObtenerHash(serviceToken.ToString());
            /*var toHash = String.Format("{0}|{1}", INTEGRATOR_ID, serviceToken);
            var token = Security.Hash(toHash);            
            return token;*/
        }

        public string ObtenerHash(string serviceToken)
        {
            var toHash = String.Format($"{INTEGRATOR_ID}|{serviceToken}");
            var token = Security.Hash(toHash);
            return token;
        }
    }
}
