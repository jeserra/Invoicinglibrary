using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.IO.Compression;
using InvoicingLibrary.Utils;
using InvoicingLibrary.cfdi33;

namespace ProcessCFDI.Controllers
{
    public class CFDIController 
    {
       
        private Comprobante _comprobante= null;
        private EstadoDeCuentaBancario _ecb = null;
        

       
        public CFDIController()
        {
            
            InitializeWebService();
            InitializeCFDI();
        }

 
        public Comprobante CFDIComprobante
        { 
            get {return _comprobante;}
            set { _comprobante = value; }
        }

        public EstadoDeCuentaBancario CFDIECB
        {
            get { return _ecb; }
            set { _ecb = value; }
        }
         
       
        protected void InitializeWebService()
        {
            
        }

        protected void InitializeCFDI()
        {
            _comprobante = new Comprobante();
            //load default template
            _comprobante = XmlSerializerHelper.Deserialize<Comprobante>(xmlCFDIFile);

            _ecb = new EstadoDeCuentaBancario();
            //load default template
            _ecb = XmlSerializerHelper.Deserialize<EstadoDeCuentaBancario>(xmlECBFile);

            
        }



        
   


        public int SetCFDI(string TheXML)
        {
            byte[] compressed = Zip(TheXML);
            string TheBase64 = Convert.ToBase64String(compressed);
            return TheBase64.Length;
            //return proxy_cfdi.SetCFDI(new SrvProcessCFDI.SetCFDIRequest() { UserName = _view.User, SerieAndFolio = _view.SerieAndFolio, TheXML = TheBase64, OriginalChain = "", DepositAmount = _view.DepositAmount, ComplementChain = "", IsBorder = _view.OnlyBorder }).iResult;
        }

        public int SetCFDIOriginalChain(string OriginalChain)
        {
            byte[] compressed = Zip(OriginalChain);
            string TheBase64 = Convert.ToBase64String(compressed);
            return TheBase64.Length;
            //return proxy_cfdi.SetOriginalChain(new SrvProcessCFDI.SetOriginalChainRequest() { UserName = _view.User, SerieAndFolio = _view.SerieAndFolio, OriginalChain = TheBase64 }).iResult;
        }

        

        public ComprobanteEmisor CFDIGetEmisor()
        {
            return _comprobante.Emisor;
        }
        public ComprobanteImpuestos CFDIGetImpuestos()
        {
            return _comprobante.Impuestos;
        }
        public List<ComprobanteConcepto> CFDIGetConceptos()
        {
            return _comprobante.Conceptos.ToList();
        }


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
       
        public void AddNameSpaces(System.Xml.XmlNode node,string prefix)
        {
            foreach (System.Xml.XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == System.Xml.XmlNodeType.Element)
                    child.Prefix = prefix;
                AddNameSpaces(child, prefix);
            }
        }

        public static string RemoveAllXmlNamespace(string xmlData)
        {
            string xmlnsPattern = "\\s+xmlns\\s*(:\\w)?\\s*=\\s*\\\"(?<url>[^\\\"]*)\\\"";
            

            MatchCollection matchCol = Regex.Matches(xmlData, xmlnsPattern);

            foreach (Match m in matchCol)
            {
                xmlData = xmlData.Replace(m.ToString(), "");
            }
            return xmlData;
        }


       

        public void AgregaComplemento_ECB()
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            ns.Add("ecb", "http://www.sat.gob.mx/ecb");

            string strXMLECB = XmlSerializerHelper.ToXmlString<EstadoDeCuentaBancario>(_ecb, ns, System.Text.Encoding.UTF8);

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            doc.LoadXml(strXMLECB);
            AddNameSpaces(doc,"ecb");

            System.Xml.XmlElement root = doc.DocumentElement;
            //any for Items
            _comprobante.Complemento.Items = new System.Xml.XmlElement[] { root };
         }

        

        public bool SaveAddenda()
        {
            XmlSerializerNamespaces nsECB = new XmlSerializerNamespaces();
            nsECB.Add("", "");
            nsECB.Add("ecb", "http://www.sat.gob.mx/ecb");

            string strXMLECB = XmlSerializerHelper.ToXmlString<EstadoDeCuentaBancario>(_ecb, nsECB, System.Text.Encoding.UTF8);

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            doc.LoadXml(strXMLECB);
            AddNameSpaces(doc, "ecb");

            //System.Xml.XmlElement root = doc.DocumentElement;
            //_comprobante.Addenda.Any = new System.Xml.XmlElement[] { root };
            
            // XmlSerializerNamespaces nsADDENDA = new XmlSerializerNamespaces();
            //nsADDENDA.Add("cfdi", @"http://www.sat.gob.mx/cfd/3");

            
            //string strXMLADDENDA = XmlSerializerHelper.ToXmlString<ComprobanteAddenda>(_comprobante.Addenda, nsADDENDA);
            //doc.LoadXml(strXMLADDENDA);

            using (TextWriter sw = new StreamWriter(System.IO.Path.Combine(filesGenerated, "CFDI" + _comprobante.Serie + _comprobante.Folio + "_Addenda.xml"), false, Encoding.UTF8)) //Set encoding
            {
                doc.Save(sw);
            }
            return true;
        }

        public bool SaveComprobanteAndSeal( )
        {

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            ns.Add("ecb", "http://www.sat.gob.mx/ecb");
            ns.Add("valesdedespensa", "http://www.sat.gob.mx/valesdedespensa");
            string strXML = XmlSerializerHelper.ToXmlString<Comprobante>(_comprobante, ns, System.Text.Encoding.UTF8);

             
            SetSeal(strXML);

            strXML = XmlSerializerHelper.ToXmlString<Comprobante>(_comprobante, ns, System.Text.Encoding.UTF8);
            strXML = strXML.Replace("xmlns:ecb=\"http://www.sat.gob.mx/ecb\">", ">");

            //strXML = strXML.Replace("UTF-16", "UTF-8");
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(strXML);
            //doc.Save(System.IO.Path.Combine(filesGenerated, "CFDI" + _comprobante.serie + _comprobante.folio + ".xml"));

            using (TextWriter sw = new StreamWriter(System.IO.Path.Combine(filesGenerated, "CFDI" + _comprobante.Serie + _comprobante.Folio + ".xml"), false, Encoding.UTF8)) //Set encoding
            {
                doc.Save(sw);

            }

            return true;
        }

        public bool SaveComprobante()
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            ns.Add("ecb", "http://www.sat.gob.mx/ecb");
            ns.Add("valesdedespensa", "http://www.sat.gob.mx/valesdedespensa");
            string strXML = XmlSerializerHelper.ToXmlString<Comprobante>(_comprobante, ns, System.Text.Encoding.UTF8);
           strXML=strXML.Replace("xmlns:ecb=\"http://www.sat.gob.mx/ecb\">", ">");
           
            //strXML = strXML.Replace("UTF-16", "UTF-8");
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(strXML);
            //doc.Save(System.IO.Path.Combine(filesGenerated, "CFDI" + _comprobante.serie + _comprobante.folio + ".xml"));

            using (TextWriter sw = new StreamWriter(System.IO.Path.Combine(filesGenerated, "CFDI" + _comprobante.Serie + _comprobante.Folio + ".xml"), false, Encoding.UTF8)) //Set encoding
            {
                doc.Save(sw);
                
            }

           return true;
        }

     

        public void SetSeal(string TheXML)
        {

            string OriginalChain = GetOriginalChain(TheXML, xsltOriginalChain);
         
        
            byte[] SHA1hash = GetSHA1(OriginalChain);

            string PassKey = "ocsisol12";
            System.Security.SecureString secPassPhrase = new System.Security.SecureString();
            foreach (char passChar in PassKey.ToCharArray())
                secPassPhrase.AppendChar(passChar);

            System.Security.Cryptography.RSACryptoServiceProvider privateKey = LoadPrivateKeyFromFile(keyFile, secPassPhrase);

            string seal = GetSeal(SHA1hash, privateKey);


            CFDIComprobante.Sello = seal;
         
        }


        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);


            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);

                    //CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
      
    #region OnlySeal

       
        public byte[] GetSHA1(string OriginalChain)
        {
            System.Security.Cryptography.SHA1CryptoServiceProvider cryptoTransformSHA1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            return cryptoTransformSHA1.ComputeHash(Encoding.UTF8.GetBytes(OriginalChain));
        }

        public string GetSeal(byte[] rgbHash, System.Security.Cryptography.RSACryptoServiceProvider privateKey)
        {
            System.Security.Cryptography.RSAPKCS1SignatureFormatter rsaPKCS1 = new System.Security.Cryptography.RSAPKCS1SignatureFormatter(privateKey);
            rsaPKCS1.SetHashAlgorithm("SHA1");

            return Convert.ToBase64String(rsaPKCS1.CreateSignature(rgbHash));
        }


        public string GetOriginalChain(string stringXML, string XSLTFile)
        {
            StringWriter sw = new StringWriter();

            try
            {
                System.Xml.Xsl.XslCompiledTransform xslt = new System.Xml.Xsl.XslCompiledTransform();

                xslt.Load(XSLTFile);

                System.Xml.XmlDocument FromXmlFile = new System.Xml.XmlDocument();
                FromXmlFile.LoadXml(stringXML);

                xslt.Transform(FromXmlFile, null, sw);
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo generar la cadena", ex.InnerException);
            }
            return sw.ToString();
        }

        public System.Security.Cryptography.X509Certificates.X509Certificate LoadCertificateFromFile(string fullFileName)
        {
            return  new System.Security.Cryptography.X509Certificates.X509Certificate(fullFileName);
        }

        public System.Security.Cryptography.RSACryptoServiceProvider LoadPrivateKeyFromFile(string fullFileName, System.Security.SecureString secPassPhrase)
        {
            byte[] privateKey = File.ReadAllBytes(fullFileName);
            return InvoicingLibrary.Utils.SSLKey.DecodeEncryptedPrivateKeyInfo(privateKey, secPassPhrase);
        }
    #endregion

        #region XMLProcessing
        public string xsdTimbreFiscalFile
        {
            get { return System.IO.Path.Combine(filesForInput, @"xsd/v3/TimbreFiscalDigital.xsd"); }
            set { throw new NotImplementedException(); }
        }

        public string xsltOriginalChain
        {
            get { return System.IO.Path.Combine(filesForInput, "xslt/cadenaoriginal_3_2.xslt"); }
            set { throw new NotImplementedException(); }
        }

        public string xsltOriginalChainTFD
        {
            get { return System.IO.Path.Combine(filesForInput, "xslt/cadenaoriginal_TFD_1_0.xslt"); }
            set { throw new NotImplementedException(); }
        }
        public string xmlCFDIFile
        {
            get { return System.IO.Path.Combine(filesForInput, @"xml/cfdv32.xml"); }
            set { throw new NotImplementedException(); }
        }
        public string xsdCFDIFile
        {
            get { return System.IO.Path.Combine(filesForInput, @"xsd/v32/cfdv32.xsd"); }
            set { throw new NotImplementedException(); }
        }

        public string xmlECBFile
        {
            get { return System.IO.Path.Combine(filesForInput, @"xml/ecb.xml"); }
            set { throw new NotImplementedException(); }
        }

        public string xmlValesDeDespensaFile
        {
            get { return System.IO.Path.Combine(filesForInput, @"xml/valesdedespensa.xml"); }
            set { throw new NotImplementedException(); }
        }
        public string xsltValesDeDespensaFile
        {
            get { return System.IO.Path.Combine(filesForInput, @"xslt/cfd/valesdedespensa/valesdedespensa.xslt"); }
            set { throw new NotImplementedException(); }
        }
        public string xsdValesDeDespensaFile
        {
            get { return System.IO.Path.Combine(filesForInput, @"xsd/common/valesdedespensa/valesdedespensa.xsd"); }
            set { throw new NotImplementedException(); }
        }
        
        public string certFile
        {
            get { return Path.Combine(filesForInput, @"certificates\production\00001000000200244085.cer"); }
            set { throw new NotImplementedException(); }
        }

        public string keyFile
        {
            get { return Path.Combine(filesForInput, @"certificates\production\oso101216gs5_1202031708s.key"); }
            set { throw new NotImplementedException(); }
        }

        public string Test_certFile
        {
            get { return Path.Combine(filesForInput, @"certificates\test\SUL010720JN8_1210231422S.cer"); }
            set { throw new NotImplementedException(); }
        }

        public string Test_keyFile
        {
            get { return Path.Combine(filesForInput, @"certificates\test\SUL010720JN8_1210231422S.key"); }
            set { throw new NotImplementedException(); }
        }

        public string filesGenerated
        {
            get { return System.IO.Path.Combine(filesForOutPut,@"generated"); }
            set { throw new NotImplementedException(); }
        }

        public string AppPath
        {
            get { return Path.GetFullPath(@"..\..\..\ProcessCFDI"); }
        }
        private string filesForInput
        {

            get { return System.IO.Path.Combine(AppPath, @"Files\ForInput"); }
            set { throw new NotImplementedException(); }
        }

        private string filesForOutPut
        {
            get { return System.IO.Path.Combine(AppPath, @"Files\ForOutPut"); }
            set { throw new NotImplementedException(); }
        }

        private string filesForOutPut_Consumptions_Ecodex
        {
            get { return System.IO.Path.Combine(AppPath, @"Files\ForOutPut\Consumptions\Ecodex"); }
            set { throw new NotImplementedException(); }
        }

        private string filesForOutPut_Refills_Consumptions
        {
            get { return System.IO.Path.Combine(AppPath, @"Files\ForOutPut\Refills\Consumptions"); }
            set { throw new NotImplementedException(); }
        }

        private string filesForOutPut_Refills_Ecodex
        {
            get { return System.IO.Path.Combine(AppPath, @"Files\ForOutPut\Refills\Ecodex"); }
            set { throw new NotImplementedException(); }
        }

        public string XMLToString(System.Xml.XmlDocument xmlDoc)
        {
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter sw = new System.IO.StringWriter(sb);
            xmlDoc.Save(sw);
            return sw.ToString();
        }
      
        #endregion


    }
}
