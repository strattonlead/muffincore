using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Muffin.EntityFrameworkCore.Entity;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muffin.EntityFrameworkCore.Globalization
{
    [Index(nameof(LanguageCode), IsUnique = true)]
    public class LanguageEntity : BaseEntity
    {
        #region Properties

        /// <summary>
        /// ISO 639-1 Language Code
        /// </summary>
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None), JsonProperty(PropertyName = "languageCode")]
        public virtual string LanguageCode { get; set; }

        [NotMapped, JsonIgnore]
        public override long Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public LocalizedStringEntity Name { get; set; }
        [JsonIgnore]
        public long? NameId { get; set; }

        [JsonIgnore]
        public ICollection<LocalizedStringValueEntity> LocalizedStringValues { get; set; }

        #endregion

        #region Constructor

        public LanguageEntity() { }

        #endregion
    }

    public class LanguageEntityTypeConfiguration : IEntityTypeConfiguration<LanguageEntity>
    {
        public void Configure(EntityTypeBuilder<LanguageEntity> builder)
        {
            builder.LocalizedStringProperty(x => x.Name, x => x.NameId);

            builder.HasMany(x => x.LocalizedStringValues)
                .WithOne(x => x.Language)
                .HasForeignKey(x => x.LanguageId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
