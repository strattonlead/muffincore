using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Muffin.EntityFrameworkCore.Centron.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.EntityFrameworkCore.Centron
{
    public class CentronDbContext : DbContext
    {
        #region Constructor

        public CentronDbContext(IServiceProvider serviceProvider)
            : base(serviceProvider.GetRequiredService<DbContextOptions<CentronDbContext>>())
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.Tracked += ChangeTracker_Tracked;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CentronDbContext).Assembly);
        }

        #endregion

        #region Read Only

        private void ChangeTracker_Tracked(object sender, EntityTrackedEventArgs e)
        {
            throw new Exception("The dbcontext is in readonly mode.");
        }

        public override int SaveChanges()
        {
            throw new Exception("The dbcontext is in readonly mode.");
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            throw new Exception("The dbcontext is in readonly mode.");
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            throw new Exception("The dbcontext is in readonly mode.");
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new Exception("The dbcontext is in readonly mode.");
        }

        #endregion

        #region DbSets

        public DbSet<Customer> Customers { get; set; }
        public DbSet<ContractType> ContractTypes { get; set; }
        public DbSet<Contract> Contracts { get; set; }

        #endregion
    }
}