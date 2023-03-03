namespace Muffin.SevDesk.Api.Models.Billing
{
    public class InvoiceItem
    {
        /// <summary>
        /// The tax rate of the position.
        /// </summary>
        [JsonProperty(PropertyName = "taxRate")]
        public decimal TaxRate { get; set; }

        /// <summary>
        /// The unity in which one item is measured.
        /// </summary>
        [JsonProperty(PropertyName = "unity")]
        public object Unity { get; set; }

        /// <summary>
        /// The quantity of items.
        /// </summary>
        [JsonProperty(PropertyName = "quantity")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// The invoice to which the position belongs.
        /// </summary>
        [JsonProperty(PropertyName = "invoice")]
        public Invoice Invoice { get; set; }

        /// <summary>
        /// A discount amount for the specific position.
        /// </summary>
        [JsonProperty(PropertyName = "discount")]
        public decimal Discount { get; set; }

        /// <summary>
        /// A text describing the position.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// The number of the position if there are multiple positions in an invoice.
        /// Needs to start with zero and is incremented for every new position.
        /// If you want to order them differently, you can change their position numbers to your needs.
        /// </summary>
        [JsonProperty(PropertyName = "positionNumber")]
        public int PositionNumber { get; set; }

        /// <summary>
        /// The name of the item in the position.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The price for one unit of the item in the position.
        /// </summary>
        [JsonProperty(PropertyName = "price")]
        public decimal Price { get; set; }

        /// <summary>
        /// You can define a part from your sevDesk inventory here.
        /// Please be aware, you will still need to provide the name, price and everything else as this will not automatically be generated.
        /// </summary>
        [JsonProperty(PropertyName = "part")]
        public object Part { get; set; }
    }
}
