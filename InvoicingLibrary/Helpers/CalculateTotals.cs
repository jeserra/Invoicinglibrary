using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoicingLibrary.BindingModels;

namespace InvoicingLibrary.Helpers
{
    public class CalculateTotals
    {
        public static void Calculate(BindingModels.Comprobante comprobante)
        {
            decimal subTotal = 0;
            decimal Total = 0;
            decimal totalImpuestos = 0;
            foreach (var concepto in comprobante.Conceptos)
            {
                subTotal += concepto.Cantidad * concepto.ValorUnitario;
                foreach(var impuesto in concepto.ConceptosImpuestos)
                {
                    totalImpuestos += impuesto.Importe;
                }
            }
            Total = totalImpuestos + subTotal;

            comprobante.SubTotal = subTotal;
            comprobante.Total = Total;
        } 
    }
}
