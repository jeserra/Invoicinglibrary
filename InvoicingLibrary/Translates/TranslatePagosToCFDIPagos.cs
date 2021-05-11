using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Invoicing.BindingModels;

namespace Invoicing.Translates
{
    public class TranslatePagosToCFDIPagos
    { 
        public static BindingModels.Comprobante TranslateToCFDI ( BindingModels.Pagos  from)
        {
            BindingModels.Comprobante to = new BindingModels.Comprobante()
            {

                 SubTotal = 0,

            };
            return to;
        }

    }
}
