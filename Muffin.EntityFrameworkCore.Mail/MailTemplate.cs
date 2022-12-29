using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Muffin.EntityFrameworkCore.Entity;
using Muffin.EntityFrameworkCore.Entity.Abstraction;
using Muffin.EntityFrameworkCore.Globalization;

namespace Muffin.EntityFrameworkCore.Mail
{
    public interface IMailTemplate : IBaseEntity
    {
        long? NameId { get; set; }
        LocalizedStringEntity Name { get; set; }

        long? SubjectId { get; set; }
        LocalizedStringEntity Subject { get; set; }

        long? BodyId { get; set; }
        LocalizedStringEntity Body { get; set; }
        int? MaxSendAttemptsCount { get; set; }
    }
    public class MailTemplate : BaseEntity, IMailTemplate
    {
        #region Properties

        public long? NameId { get; set; }
        public LocalizedStringEntity Name { get; set; }

        public long? SubjectId { get; set; }
        public LocalizedStringEntity Subject { get; set; }

        public long? BodyId { get; set; }
        public LocalizedStringEntity Body { get; set; }
        public int? MaxSendAttemptsCount { get; set; }

        #endregion
    }

    public class MailTemplateTypeConfiguration : IEntityTypeConfiguration<MailTemplate>
    {
        public void Configure(EntityTypeBuilder<MailTemplate> builder)
        {
            builder.LocalizedStringProperty(x => x.Name, x => x.NameId);
            builder.LocalizedStringProperty(x => x.Subject, x => x.SubjectId);
            builder.LocalizedStringProperty(x => x.Body, x => x.BodyId);
        }
    }
}
