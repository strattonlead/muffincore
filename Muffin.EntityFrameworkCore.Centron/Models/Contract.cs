using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muffin.EntityFrameworkCore.Centron.Models
{
    [Table("VertragKopf")]
    public class Contract
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "contractId")]
        public int ContractId { get; set; }

        [JsonProperty(PropertyName = "customerId")]
        public int CustomerId { get; set; }

        [JsonProperty(PropertyName = "contractTypeId")]
        public int ContractTypeId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "version")]
        public int? Version { get; set; }

        [JsonIgnore]
        public ContractType ContractType { get; set; }

        [JsonIgnore]
        public Customer Customer { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty(PropertyName = "endDate")]
        public DateTime EndDate { get; set; }
    }

    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("I3D");
            builder.Property(x => x.ContractId).HasColumnName("Nummer");
            builder.Property(x => x.CustomerId).HasColumnName("KundenID");
            builder.Property(x => x.ContractTypeId).HasColumnName("VertragsArtI3D");
            builder.Property(x => x.StartDate).HasColumnName("Beginn");
            builder.Property(x => x.EndDate).HasColumnName("Ende");
        }
    }
}
