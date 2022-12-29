using Muffin.EntityFrameworkCore.Entity;
using Muffin.EntityFrameworkCore.Entity.Abstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muffin.EntityFrameworkCore.Mail
{
    public interface ISendMailJob : IBaseEntity
    {

        long? SmtpCredentialId { get; set; }

        long? MailTemplateId { get; set; }

        string Subject { get; set; }
        string Body { get; set; }

        string LanguageId { get; set; }

        DateTime? SendDateUtc { get; set; }
        DateTime? SentDateUtc { get; set; }
        DateTime? LastErrorDateUtc { get; set; }
        int SendAttemptsCount { get; set; }
        SendMailStateTypes SendState { get; set; }
    }

    public class SendMailJob : BaseEntity, ISendMailJob
    {
        #region Properties

        [ForeignKey("SmtpCredentialId")]
        public virtual SmtpCredential SmtpCredential { get; set; }
        public virtual long? SmtpCredentialId { get; set; }

        [ForeignKey("MailTemplateId")]
        public virtual MailTemplate MailTemplate { get; set; }
        public long? MailTemplateId { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }

        public string LanguageId { get; set; }

        [ForeignKey("SendMailJobId")]
        public virtual ICollection<SendMailJobRecipient> Recipients { get; set; }
        [ForeignKey("SendMailJobId")]
        public virtual ICollection<SendMailJobError> Errors { get; set; }

        public DateTime? SendDateUtc { get; set; }
        public DateTime? SentDateUtc { get; set; }
        public DateTime? LastErrorDateUtc { get; set; }
        public int SendAttemptsCount { get; set; }
        public SendMailStateTypes SendState { get; set; }

        #endregion
    }

    public enum SendMailStateTypes
    {
        NotSent = 0,
        Sent = 1,
        TooManyInvalidSentAttempts = 2
    }
}
