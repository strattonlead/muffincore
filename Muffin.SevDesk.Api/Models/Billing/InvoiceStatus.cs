namespace Muffin.SevDesk.Api.Models.Billing
{
    /// <summary>
    /// https://api.sevdesk.de/#section/Types-and-status-of-invoices
    /// </summary>
    public static class InvoiceStatus
    {
        /// <summary>
        /// The invoice is a deactivated recurring invoice.
        /// This status code is only relevant for recurring invoices.
        /// </summary>
        public const int DeactivatedRecurringInvoice = 50;

        /// <summary>
        /// The invoice is still a draft.
        /// It has not been sent to the end-customer and can still be changed.
        /// </summary>
        public const int Draft = 100;

        /// <summary>
        /// The invoice has been sent to the end-customer.
        /// It is either shown as open if the pay date is not exceeded or due if it is.
        /// </summary>
        public const int OpenDue = 200;

        /// <summary>
        /// The invoice has been payed by the end-customer.
        /// This means, that it is linked to a transaction on some payment account in sevDesk.
        /// </summary>
        public const int Payed = 1000;
    }
}
