using Muffin.SevDesk.Api.Models.Billing;
using System.Threading.Tasks;

namespace Muffin.SevDesk.Api
{
    public interface ISevDeskService
    {
        Task<string> GetNextInvoiceNumber(string objectType, string type);
        Task<MemoryFile> GetInvoicePdf(int invoiceId);
        Task SendInvoiceViaEmail(int invoiceId, SendMailData sendMailData);

        Task<bool> CheckCustomerNumberAvailability(string customerNumber);
        Task<string> GetNextCustomerNumber();
    }
}