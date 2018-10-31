using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Invoicing.srvSeguridad;
using Invoicing.srvTimbrado;
using Invoicing.srvClientes;
using ProcessCFDI.Utils;
using System.ServiceModel;
using Invoicing.Interfaces;
using System.Configuration;

namespace Invoicing.CFDIProviders
{
    public class EcodexProvider:ISATProvider
    {

        public string INTEGRATOR_ID
        {
            get
            {
                return  ConfigurationManager.AppSettings["INTEGRATOR_ID"].ToString();
            }
        }
      //  private const String INTEGRATOR_ID = "2b3a8764-d586-4543-9b7e-82834443f219";

        public byte[] ObtenerQR(string RFC, String UUID, long transactionId)
        {
            try
            {
                TimbradoClient client = new TimbradoClient();

                var response =  client.ObtenerQRTimbrado(RFC, ObtenerToken(RFC, transactionId), ref transactionId, UUID);
                return response.Imagen;
            }
            catch (FaultException<srvTimbrado.FallaServicio> serviceFault)
            {
                throw new Exception(String.Format("Error al timbrar  {0} {1}", serviceFault.Message, serviceFault.Detail.Numero));
            }
            catch (FaultException<srvTimbrado.FallaSesion> serviceSesion)
            {
                throw new Exception(String.Format("Error al timbrar {0}", serviceSesion.Message));
            }
            catch (FaultException<srvTimbrado.FallaValidacion> faultvalidation)
            {
                throw new Exception(String.Format("Error al timbrar {0}", faultvalidation.Message));
            }
        }

        public String Timbrar (String RFC,  String Comprobante, long transactionId)
        {
            try
            {
                TimbradoClient client = new TimbradoClient();

                ComprobanteXML commprobante = new ComprobanteXML();
                commprobante.DatosXML = Comprobante;
                var timbrado = client.TimbraXML(ref commprobante, RFC, ObtenerToken(RFC, transactionId) , ref transactionId);
                return commprobante.DatosXML;
            }
            catch(FaultException<srvTimbrado.FallaServicio> serviceFault)
            {
                throw new Exception(String.Format( "Error al timbrar  {0} {1}", serviceFault.Message, serviceFault.Detail.Numero));
                
            }
            catch (FaultException<srvTimbrado.FallaSesion> serviceSesion)
            {
                throw new Exception(String.Format( "Error al timbrar {0}", serviceSesion.Message));
            }
            catch (FaultException<srvTimbrado.FallaValidacion> faultvalidation)
            {
                throw new Exception(String.Format( "Error al timbrar {0}", faultvalidation.Message));
            }
        }

        public string ObtenerToken (string RFC, long transactionID)
        {
            SeguridadClient srv = new SeguridadClient();
            var serviceToken = srv.ObtenerToken(RFC, ref transactionID);

            return ObtenerHash(serviceToken);
            /*var toHash = String.Format("{0}|{1}", INTEGRATOR_ID, serviceToken);
            var token = Security.Hash(toHash);            
            return token;*/
        }

        public string ObtenerHash (string serviceToken)
        {
            var toHash = String.Format("{0}|{1}", INTEGRATOR_ID, serviceToken);
            var token = Security.Hash(toHash);
            return token;
        }
    }
}
