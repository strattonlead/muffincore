using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muffin.Common.Util;
using Muffin.Tenancy.Abstraction.Services;
using Muffin.Tenancy.Services.Abstraction;

namespace Muffin.BackgroundServices
{
    public abstract class ScheduledBackgroundService : BaseBackgroundService
    {
        #region Properties

        protected readonly ScheduledBackgroundServiceOptions? Options;
        private CancellationTokenSource InstantCancellationTokenSource;

        #endregion

        #region Constructor

        public ScheduledBackgroundService(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            var type = typeof(ScheduledBackgroundServiceOptions<>);
            var genericType = type.MakeGenericType(GetType());

            Options = serviceProvider.GetService(genericType) as ScheduledBackgroundServiceOptions;
            InstantCancellationTokenSource = new CancellationTokenSource();
        }

        #endregion

        #region IHostedService

        public void ForceRun()
        {
            InstantCancellationTokenSource.Cancel();
            InstantCancellationTokenSource.TryReset();
            OnForceRun();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                Logger?.LogInformation($"{GetType()?.Name} started");
                OnStart();
                while (!stoppingToken.IsCancellationRequested)
                {
                    OnRun();
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

                    OnWait();
                    if (Options != null)
                    {
                        await MoreTaskExtensions.Delay(Options.Interval, stoppingToken, InstantCancellationTokenSource.Token);
                    }
                    else
                    {
                        await MoreTaskExtensions.Delay(ScheduledBackgroundServiceOptions.DefaultInterval, stoppingToken, InstantCancellationTokenSource.Token);
                    }
                }
                Logger?.LogInformation($"{GetType()?.Name} stopped");

            }, stoppingToken);
        }

        protected abstract Task ExecuteScopedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);

        #endregion
    }

    public abstract class ScheduledBackgroundService<TContext> : ScheduledBackgroundService
       where TContext : notnull
    {
        #region Constructor

        public ScheduledBackgroundService(IServiceProvider serviceProvider)
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

    public abstract class ScheduledBackgroundServiceWithTenancy<TContext> : ScheduledBackgroundService<TContext>
       where TContext : notnull
    {
        #region Properties

        protected bool ParallelExecution { get; set; }

        #endregion

        #region Constructor

        public ScheduledBackgroundServiceWithTenancy(IServiceProvider serviceProvider)
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

    public class ScheduledBackgroundServiceOptions
    {
        public TimeSpan Interval { get; set; }
        public static readonly TimeSpan DefaultInterval = TimeSpan.FromMinutes(1);
    }
    public class ScheduledBackgroundServiceOptions<TService> : ScheduledBackgroundServiceOptions { }
    public class ScheduledBackgroundServiceOptionsBuilder<TService>
    {
        private ScheduledBackgroundServiceOptions<TService> Options = new ScheduledBackgroundServiceOptions<TService>();

        public ScheduledBackgroundServiceOptionsBuilder<TService> UseInterval(TimeSpan interval)
        {
            Options.Interval = interval;
            return this;
        }

        public ScheduledBackgroundServiceOptions<TService> Build()
        {
            return Options;
        }
    }
    public static class ScheduledBackgroundServiceExtensions
    {
        public static void AddScheduledBackgroundService<TService>(this IServiceCollection services, Action<ScheduledBackgroundServiceOptionsBuilder<TService>> builder)
            where TService : ScheduledBackgroundService
        {
            if (builder != null)
            {
                var optionsBuilder = new ScheduledBackgroundServiceOptionsBuilder<TService>();
                builder(optionsBuilder);

                var options = optionsBuilder.Build();
                services.AddSingleton(options);
            }

            services.AddHostedService<TService>();
        }
    }
}
