using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoicingLibrary.Interfaces
{
    public interface ICertificate
    {
        int idCertificate { get; set; } 
        string CerFile { get; set; }
        string KeyFile { get; set; }
        string Pwd { get; set; }
        string NoCertificate { get; set; }

        DateTime ValidFrom { get; set; }
        DateTime ValidUntil { get; set; }
    }
}
