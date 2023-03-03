namespace Muffin.SevDesk.Api.Models
{
    /// <summary>
    /// https://api.sevdesk.de/#section/Types-and-status-of-invoices
    /// </summary>
    public static class InvoiceTypes
    {
        /// <summary>
        /// A normal invoice which documents a simple selling process.
        /// </summary>
        public const string NormalInvoice = "RE";

        /// <summary>
        /// An invoice which generates normal invoices with the same values regularly in fixed time frames (every month, year, ...).
        /// </summary>
        public const string RecurringInvoice = "WKR";

        /// <summary>
        /// An invoice which cancels another already created normal invoice.
        /// </summary>
        public const string CancellationInvoice = "SR";

        /// <summary>
        /// An invoice which gets created if the end-customer failed to pay a normal invoice in a given time frame.
        /// Often includes some kind of reminder fee.
        /// </summary>
        public const string ReminderInvoice = "MA";

        /// <summary>
        /// Part of a complete invoice. All part invoices together result in the complete invoice.
        /// Often used if the end-customer can partly pay for items or services.
        /// </summary>
        public const string PartInvoice = "TR";

        /// <summary>
        /// The final invoice of all part invoices which completes the invoice.
        /// After the final invoice is payed by the end-customer, the selling process is complete.
        /// </summary>
        public const string FinalInvoice = "ER";
    }
}
