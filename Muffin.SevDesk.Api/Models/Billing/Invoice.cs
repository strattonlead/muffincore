using Newtonsoft.Json;
using System;

namespace Muffin.SevDesk.Api.Models.Billing
{
    /// <summary>
    /// https://api.sevdesk.de/#section/Attributes-of-an-invoice
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Defines the vat-regulation for the invoice.
        /// Can be: default, eu, noteu, custom, ss
        /// </summary>
        [JsonProperty(PropertyName = "taxType")]
        public string TaxType { get; set; }

        /// <summary>
        /// Currency of the invoice.
        /// Needs to be currency code according to ISO-4217.
        /// </summary>
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Text describing the vat regulation you chose.
        /// A common text of our German customers would be:
        /// Umsatzsteuer ausweisen or zzgl.Umsatzsteuer 19%
        /// </summary>
        [JsonProperty(PropertyName = "taxText")]
        public string TaxText { get; set; }

        /// <summary>
        /// The tax rate of the invoice.
        /// Please be aware, that this value will be overwritten by tax rates of the invoice positions.
        /// </summary>
        [JsonProperty(PropertyName = "taxRate")]
        public int TaxRate { get; set; }

#warning TODO
        /// <summary>
        /// The sevDesk user which acts as a contact person for this invoice.
        /// </summary>
        [JsonProperty(PropertyName = "contactPerson")]
        public object ContactPerson { get; set; }

        /// <summary>
        /// If the sevDesk account is falling under the small entrepreneur scheme the invoices must't contain any vat.
        /// If this is the case, this attribute should be true, otherwise false.
        /// </summary>
        [JsonProperty(PropertyName = "smallSettlement")]
        public bool SmallSettlement { get; set; }

        /// <summary>
        /// The invoice date.
        /// </summary>
        [JsonProperty(PropertyName = "invoiceDate")]
        public long InvoiceDate { get; set; }

        /// <summary>
        /// The invoice status.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        /// <summary>
        /// This attribute determines, if the price you give the invoice positions will be regarded as gross or net.
        ///If true, the price attribute will hold the net value, otherwise the gross value, as described in the section above.
        /// </summary>
        [JsonProperty(PropertyName = "showNet")]
        public int ShowNet { get; set; }

        /// <summary>
        /// If the end-customer gets a discount if he pays the invoice in a given time, you can specify the percentage of the discount here.
        ///Should this be the case, you will need to provide a value for the attribute discountTime too, otherwise there is no time given and the end-customer won't get a discount.
        ///If you don't want this, just leave this attribute at zero.
        /// </summary>
        [JsonProperty(PropertyName = "discount")]
        public int Discount { get; set; }

        /// <summary>
        /// If a value other than zero is used for the discount attribute you need to specify the amount of days for which the discount is granted.
        /// </summary>
        [JsonProperty(PropertyName = "discountTime")]
        public int DiscountTime { get; set; }

        /// <summary>
        /// If the invoice is enshrined, it can not longer be changed.
        //If you want this, you can provide the value "1".
        ///Please be aware, that this action can not be undone.
        /// </summary>
        [JsonProperty(PropertyName = "enshrined")]
        public bool Enshrined { get; set; }

        /// <summary>
        /// You can use this attribute to provide a note for the invoice.
        ///It can be used for reference numbers, order numbers or other important information.
        /// </summary>
        [JsonProperty(PropertyName = "customerInternalNote")]
        public string CustomerInternalNote { get; set; }

        /// <summary>
        /// Holds the complete address to which the invoice is directed.
        /// You can use line brakes to separate the different address parts.
        /// </summary>
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        /// <summary>
        ///  The delivery date of the invoice.
        ///This can also be a date range if you provide another timestamp for deliveryDateUntil.
        /// </summary>
        [JsonProperty(PropertyName = "deliveryDate")]
        public long DeliveryDate { get; set; }

        /// <summary>
        /// You can provide a value here if you want the delivery date to be a date range and you have already given a timestamp to deliveryDate.
        /// </summary>
        [JsonProperty(PropertyName = "deliveryDateUntil")]
        public long DeliveryDateUntil { get; set; }

        /// <summary>
        /// If you don't plan to send the invoice over another endpoint like /Invoice/sendViaEmail or Invoice/sendBy but instead give it the status "200" directly, you need to specify a send type here.
        /// Valid types are: VPR(printed), VPDF(downloaded), VM(mailed), VP(postal).
        /// </summary>
        [JsonProperty(PropertyName = "sendType")]
        public long SendType { get; set; }

        /// <summary>
        /// You can specify the object from which an invoice originated, like an order.
        /// Just provide the ID of this object.
        /// </summary>
        [JsonProperty(PropertyName = "origin")]
        public int Origin { get; set; }

        /// <summary>
        /// The invoice number.
        /// </summary>
        [JsonProperty(PropertyName = "invoiceNumber")]
        public string InvoiceNumber { get; set; }

#warning TODO
        /// <summary>
        /// Your own tax set that should be used if you defined custom as taxType.
        /// </summary>
        [JsonProperty(PropertyName = "taxSet")]
        public object TaxSet { get; set; }

#warning TODO
        /// <summary>
        /// The end-customer to which the invoice is directed.
        /// Please note, you need to provide a contact if the invoice has any other status than 100.
        /// </summary>
        [JsonProperty(PropertyName = "contact")]
        public object Contact { get; set; }

        /// <summary>
        /// The invoice header.
        /// Usually consists of the invoice number and a prefix.
        /// </summary>
        [JsonProperty(PropertyName = "header")]
        public string Header { get; set; }

        /// <summary>
        /// A head text for the invoice.
        /// Can contain certain html tags.
        /// </summary>
        [JsonProperty(PropertyName = "headText")]
        public string HeadText { get; set; }

        /// <summary>
        /// A foot text for the invoice.
        /// Can contain certain html tags.
        /// </summary>
        [JsonProperty(PropertyName = "footText")]
        public string FootText { get; set; }

        /// <summary>
        /// The time the end-customer has to pay the invoice in days.
        /// </summary>
        [JsonProperty(PropertyName = "timeToPay")]
        public int TimeToPay { get; set; }

        /// <summary>
        /// The date the end-customer has payed the invoice.
        /// </summary>
        [JsonProperty(PropertyName = "payDate")]
        public long PayDate { get; set; }

        /// <summary>
        /// The payment method for the invoice.
        /// Needs the ID of a specified payment method.
        /// </summary>
        [JsonProperty(PropertyName = "paymentMethod")]
        public int PaymentMethod { get; set; }

        /// <summary>
        /// A cost centre for the invoice.
        /// </summary>
        [JsonProperty(PropertyName = "costCentre")]
        public object CostCentre { get; set; }

        /// <summary>
        /// The date the invoice was sent to the end-customer.
        /// </summary>
        [JsonProperty(PropertyName = "sendDate")]
        public long SendDate { get; set; }

        /// <summary>
        /// RE - invoice
        /// WKR - recurring invoice
        /// SR - cancellation invoice
        /// MA - invoice remider
        /// TR - partial invoice
        /// ER - final invoice
        /// </summary>
        [JsonProperty(PropertyName = "invoiceType")]
        public string InvoiceType { get; set; }

        /// <summary>
        /// The dunning level.
        /// Starts with 1 (Payment reminder) and should be incremented by one every time another reminder is sent.
        /// </summary>
        [JsonProperty(PropertyName = "dunningLevel")]
        public int DunningLevel { get; set; }

        /// <summary>
        /// The deadline for the next reminder.
        /// </summary>
        [JsonProperty(PropertyName = "reminderDeadline")]
        public long ReminderDeadline { get; set; }

        /// <summary>
        /// The reminder debit.
        /// </summary>
        [JsonProperty(PropertyName = "reminderDebit")]
        public decimal ReminderDebit { get; set; }

        /// <summary>
        /// The total reminder amount.
        /// </summary>
        [JsonProperty(PropertyName = "reminderTotal")]
        public decimal ReminderTotal { get; set; }

        /// <summary>
        /// The additional reminder charge.
        /// </summary>
        [JsonProperty(PropertyName = "reminderCharge")]
        public decimal ReminderCharge { get; set; }

        /// <summary>
        /// The interval in which recurring invoices are due. ISO-8601 Duration
        /// </summary>
        [JsonProperty(PropertyName = "accountIntervall")]
        public TimeSpan AccountIntervall { get; set; }

        /// <summary>
        /// The date when the next invoice is due.
        /// </summary>
        [JsonProperty(PropertyName = "accountNextInvoice")]
        public long AccountNextInvoice { get; set; }
    }
}