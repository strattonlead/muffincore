namespace Muffin.SevDesk.Api.Models.Contacts
{
    /// <summary>
    /// https://api.sevdesk.de/#section/Attributes-of-a-contact
    /// </summary>
    public class Contact
    {
        /// <summary>
        /// The name of an organisation
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The first name of an individual (yes, we know...)
        /// </summary>
        [JsonProperty(PropertyName = "surename")]
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of an individual
        /// </summary>
        [JsonProperty(PropertyName = "familyname")]
        public string LastName { get; set; }

        /// <summary>
        /// The middle name (or name-suffix) of an individual
        /// </summary>
        [JsonProperty(PropertyName = "name2")]
        public string MiddleName { get; set; }

        /// <summary>
        /// The category of the contact
        /// </summary>
        [JsonProperty(PropertyName = "category")]
        public object Category { get; set; }

        /// <summary>
        /// Time frame in which a cashback is granted to the customer if he pays an invoice
        /// </summary>
        [JsonProperty(PropertyName = "defaultCashbackTime")]
        public int DefaultCashbackTime { get; set; }

        /// <summary>
        /// Percentage of which the price of an invoice is reduced if payed in above time frame
        /// </summary>
        [JsonProperty(PropertyName = "defaultCashbackPercent")]
        public decimal DefaultCashbackPercent { get; set; }

        /// <summary>
        /// Tax number of the contact.
        /// </summary>
        [JsonProperty(PropertyName = "taxNumber")]
        public string TaxNumber { get; set; }

        /// <summary>
        /// Defines if the contact is free of vat
        /// </summary>
        [JsonProperty(PropertyName = "excemptVat")]
        public bool ExcemptVat { get; set; }

        /// <summary>
        /// Defines the vat-regulation for the invoice.
        /// Can be: default, eu, noteu, custom, ss
        /// </summary>
        [JsonProperty(PropertyName = "taxType")]
        public string TaxType { get; set; }

        /// <summary>
        /// You can enter the ID of your own vat-regulation here if you provided custom for taxType
        /// </summary>
        [JsonProperty(PropertyName = "taxSet")]
        public int TaxSet { get; set; }

        /// <summary>
        /// The default time this end customer has to pay invoices
        /// </summary>
        [JsonProperty(PropertyName = "defaultTimeToPay")]
        public int DefaultTimeToPay { get; set; }

        /// <summary>
        /// The bank number of the contact
        /// </summary>
        [JsonProperty(PropertyName = "bankNumber")]
        public int BankNumber { get; set; }

        /// <summary>
        /// The birthday
        /// </summary>
        [JsonProperty(PropertyName = "birthday")]
        public long Birthday { get; set; }

        /// <summary>
        /// The vat number
        /// </summary>
        [JsonProperty(PropertyName = "vatNumber")]
        public long VatNumber { get; set; }

        /// <summary>
        /// A default discount amount this contact gets
        /// </summary>
        [JsonProperty(PropertyName = "defaultDiscountAmount")]
        public decimal DefaultDiscountAmount { get; set; }

        /// <summary>
        /// Define if the value in defaultDiscountAmount is regarded as a percentage.
        /// </summary>
        [JsonProperty(PropertyName = "defaultDiscountPercentage")]
        public bool DefaultDiscountPercentage { get; set; }

        /// <summary>
        /// Gender (m,w, or your own definition)
        /// </summary>
        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; set; }

        /// <summary>
        /// Academic title of the contact
        /// </summary>
        [JsonProperty(PropertyName = "academicTitle")]
        public string AcademicTitle { get; set; }

        /// <summary>
        /// A description for the contact
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Position an individual holds in an organisation
        /// </summary>
        [JsonProperty(PropertyName = "titel")]
        public string Title { get; set; }

        /// <summary>
        /// The organisation to which this individual belongs
        /// </summary>
        [JsonProperty(PropertyName = "parent")]
        public Contact Parent { get; set; }

        /// <summary>
        /// The customer number of the contact.
        /// </summary>
        [JsonProperty(PropertyName = "customerNumber")]
        public string CustomerNumber { get; set; }

        /// <summary>
        /// The bank account number
        /// </summary>
        [JsonProperty(PropertyName = "bankAccount")]
        public string BankAccount { get; set; }
    }
}
