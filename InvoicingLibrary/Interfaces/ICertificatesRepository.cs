using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoicingLibrary.Interfaces
{
    public interface ICertificatesRepository
    {
        ICertificate GetCertificate(string noCertificado);
        ICertificate GetCertificate(string AccountId, String RFC);
        bool SaveCertificate(int accountId, ICertificate certificate);
    }
}
