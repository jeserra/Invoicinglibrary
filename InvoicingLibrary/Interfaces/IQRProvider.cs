using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoicingLibrary.Interfaces
{
    public interface IQRProvider
    {
        Task<byte[]> GenerateQR(string rfc, string UUID);
    }
}
