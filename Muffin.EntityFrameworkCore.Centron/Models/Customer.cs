using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Muffin.EntityFrameworkCore.Centron.Models
{
    [Table("Kunden")]
    public class Customer
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<Contract> Contracts { get; set; }

        [JsonIgnore, NotMapped]
        public Dictionary<int, Contract[]> ContractsByContractId => Contracts?
            .GroupBy(x => x.ContractId)?
            .ToDictionary(x => x.Key, x => x.ToArray());
    }

    public class CustomerTypeConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("I3D");

            builder.HasMany(x => x.Contracts)
                .WithOne(x => x.Customer)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
