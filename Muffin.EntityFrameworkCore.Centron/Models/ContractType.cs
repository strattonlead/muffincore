using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muffin.EntityFrameworkCore.Centron.Models
{
    [Table("VertragsArt")]
    public class ContractType
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<Contract> Contracts { get; set; }
    }

    public class ContractTypeConfiguration : IEntityTypeConfiguration<ContractType>
    {
        public void Configure(EntityTypeBuilder<ContractType> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("I3D");
            builder.Property(x => x.Name).HasColumnName("Bezeichnung");

            builder.HasMany(x => x.Contracts)
                .WithOne(x => x.ContractType)
                .HasForeignKey(x => x.ContractTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
