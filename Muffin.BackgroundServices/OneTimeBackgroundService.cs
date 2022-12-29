using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muffin.Tenancy.Abstraction.Services;
using Muffin.Tenancy.Services.Abstraction;

namespace Muffin.BackgroundServices
{
    public abstract class OneTimeBackgroundService : BaseBackgroundService
    {
        #region Constructor

        public OneTimeBackgroundService(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        #endregion

        #region IHostedService

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                Logger?.LogInformation($"{GetType()?.Name} started");

                try
                {
                    using (var scope = ServiceScopeFactory.CreateScope())
                    {
                        await ExecuteScopedAsync(scope.ServiceProvider, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex.ToString());
                }

                Logger?.LogInformation($"{GetType()?.Name} stopped");
            }, stoppingToken);
        }

        protected abstract Task ExecuteScopedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);

        #endregion
    }

    public abstract class OneTimeBackgroundService<TContext> : OneTimeBackgroundService
        where TContext : notnull
    {
        #region Constructor

        public OneTimeBackgroundService(IServiceProvider serviceProvider)
            : base(serviceProvider)
        { }

        #endregion

        protected override async Task ExecuteScopedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var context = serviceProvider.GetRequiredService<TContext>();
            await ExecuteScopedAsync(context, cancellationToken);
        }

        protected abstract Task ExecuteScopedAsync(TContext scope, CancellationToken cancellationToken);
    }

    public abstract class OneTimeBackgroundServiceWithTenancy<TContext> : OneTimeBackgroundService<TContext>
       where TContext : notnull
    {
        #region Properties

        protected bool ParallelExecution { get; set; }

        #endregion

        #region Constructor

        public OneTimeBackgroundServiceWithTenancy(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        #endregion

        #region IHosted Service

        protected override async Task ExecuteScopedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var context = serviceProvider.GetRequiredService<TContext>();
            var tenantScope = serviceProvider.GetRequiredService<ITenantScope>();
            var tenantEnumerator = serviceProvider.GetRequiredService<ITenantEnumerator>();
            var tenants = tenantEnumerator.GetEnumerator();

            if (ParallelExecution)
            {
                var tasks = tenants.Select(tenant => tenantScope.InvokeScopedAsync<TContext>(tenant, async context => await ExecuteScopedAsync(context, cancellationToken)));
                await Task.WhenAll(tasks);
            }
            else
            {
                foreach (var tenant in tenants)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    await tenantScope.InvokeScopedAsync<TContext>(tenant, async context =>
                    {
                        await ExecuteScopedAsync(context, cancellationToken);
                    });
                }
            }
        }

        #endregion
    }
}