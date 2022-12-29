using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Muffin.EntityFrameworkCore.Entity;
using Muffin.Tenancy.Abstraction;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muffin.EntityFrameworkCore.Globalization
{
    public class LocalizedStringValueEntity : BaseEntity, ITenantOwned
    {
        #region Properties

        [NotMapped]
        public override long Id { get; set; }

        public string LanguageId { get; set; }
        public LanguageEntity Language { get; set; }

        public long LocalizedStringId { get; set; }
        [DbAfterSaveIgnore]
        public LocalizedStringEntity LocalizedString { get; set; }

        public string Value { get; set; }
        public long? TenantId { get; set; }

        #endregion

        #region Constructor

        public LocalizedStringValueEntity() { }

        #endregion
    }

    public class LocalizedStringValueEntityTypeConfiguration : IEntityTypeConfiguration<LocalizedStringValueEntity>
    {
        public void Configure(EntityTypeBuilder<LocalizedStringValueEntity> builder)
        {
            builder.HasKey(x => new { x.LanguageId, x.LocalizedStringId });
        }
    }

    public class DbAfterSaveIgnoreAttribute : Attribute { }
}
