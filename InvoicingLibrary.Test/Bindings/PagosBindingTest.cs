using System;
using System.Net.NetworkInformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InvoicingLibrary.CFDI;
using NSubstitute;
using InvoicingLibrary.Interfaces;
using InvoicingLibrary.Test.Certifcate;
using Newtonsoft.Json;
using ProcessCFDI.Controllers;
using InvoicingLibrary.Helpers;
using InvoicingLibrary.BindingModels;
using System.Collections.Generic;

namespace InvoicingLibrary.Test.Bindings
{
    [TestClass]
    public class PagosBindingTest
    {
        public BindingModels.Comprobante _testComprobante;
        public Interfaces.ICertificatesRepository _MockRepository;
        public Interfaces.IInvoicingRepository _MoInvoicingRepository;
        public Interfaces.ISATProvider _moqSatProvider;


        public void InitializeRepository()
        {
            _MockRepository = Substitute.For<ICertificatesRepository>();
            _MockRepository.GetCertificate("20001000000300022815").ReturnsForAnyArgs(new CertificateMoq());

            _MoInvoicingRepository = Substitute.For<IInvoicingRepository>();
            _moqSatProvider = new SatProviderMoq();
        }

        [TestInitialize]
        public void InitializeComprobante()
        {
            InitializeRepository();
        }

        [TestMethod]
        public void GenerateEmptyComprobantePagos()
        {
            var comprobante = GenerateComprobantePago.GenerateNew();
            Assert.IsNotNull(comprobante);
             
        }

        [TestMethod]
        public void GenerateComprobantePagosTest()
        {
            var _testPago = GenerateComprobantePago.GenerateNew();
            _testPago.LugarExpedicion = "99100";
            _testPago.noCertificado = "20001000000300022815";
            _testPago.UsoCFDI = "G01";
            _testPago.Emisor = new BindingModels.Emisor()
            {
                Nombre = "Juan perez",
                RegimenFiscal = "612",
                RFC = "FUNK671228PH6"
            };
            _testPago.Receptor = new BindingModels.Receptor()
            {
                Nombre = "Pepe perez",
                RFC = "AAA010101AAA"
             
            };
                _testPago.Pagos = new Pagos()
                {
                    ListaPagos = new List<Pago>()
                    {
                        new Pago()
                        {
                             TipoCadPago = null,
                             FechaPago = DateTimeOffset.Parse( "2017-01-03T12:11:09"),
                             FormaDePagoP = "01",
                             MonedaP = "MXN",
                             Monto = 4500,
                             NumOperation = "01",                             
                             ListaDocumentos = new List<DoctosRelacionados>()
                                {
                                    new DoctosRelacionados ()
                                    {
                                        NumParcialidad = 1,
                                        MetodoDePagoDR = "01",
                                        idDocumento = Guid.NewGuid(),
                                        Folio = 10,
                                        Serie = "AAA",
                                        ImpPagado = 4500,
                                        MonedaDR = "MXN"
                                    }
                                }
                        }
                    }
                };

            var cfdiController = new CFDIv33(_MockRepository, _moqSatProvider);
            var xmlComprobante = cfdiController.CreateCFDI(_testPago);
            Assert.IsNotNull(_testPago);
        }
        
    }
}
