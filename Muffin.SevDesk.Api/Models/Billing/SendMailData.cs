using Newtonsoft.Json;

namespace Muffin.SevDesk.Api.Models.Billing
{
    /// <summary>
    /// https://api.sevdesk.de/#tag/Invoice/operation/sendInvoiceViaEMail
    /// </summary>
    public class SendMailData
    {
        /// <summary>
        /// The recipient of the email.
        /// </summary>
        [JsonProperty(PropertyName = "toEmail")]
        public string ToEmail { get; set; }

        /// <summary>
        /// The subject of the email.
        /// </summary>
        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }

        /// <summary>
        /// The text of the email. Can contain html.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Should a copy of this email be sent to you?
        /// </summary>
        [JsonProperty(PropertyName = "copy")]
        public bool Copy { get; set; }

        /// <summary>
        /// Additional attachments to the mail. String of IDs of existing documents in your * sevdesk account separated by ','
        /// </summary>
        [JsonProperty(PropertyName = "additionalAttachments")]
        public string AdditionalAttachments { get; set; }

        /// <summary>
        /// String of mail addresses to be put as cc separated by ','
        /// </summary>
        [JsonProperty(PropertyName = "ccEmail")]
        public string CcEmail { get; set; }

        /// <summary>
        /// String of mail addresses to be put as bcc separated by ','
        /// </summary>
        [JsonProperty(PropertyName = "bccEmail")]
        public string BccEmail { get; set; }
    }
}
