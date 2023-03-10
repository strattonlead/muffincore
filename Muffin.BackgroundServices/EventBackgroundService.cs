using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muffin.Tenancy.Abstraction;
using Muffin.Tenancy.Abstraction.Services;
using Muffin.Tenancy.Services.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
#pragma warning disable CS8601 // Mögliche Nullverweiszuweisung.
namespace Muffin.BackgroundServices
{
    public abstract class EventBackgroundService : BaseBackgroundService
    {
        #region Properties

        protected readonly ManualResetEvent ResetEvent;
        protected readonly EventBackgroundServiceEvents Events;
        protected readonly EventBackgroundServiceOptions Options;
        protected readonly EventBackgroundServiceController Controller;

        protected bool RunOnStartup { get; set; }

        #endregion

        #region Constructor

        public EventBackgroundService(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Options = EventBackgroundServiceOptions.GetOptions(this, serviceProvider);
            Events = EventBackgroundServiceEvents.GetEvents(this, serviceProvider);
            ResetEvent = new ManualResetEvent(false);
            Controller = EventBackgroundServiceController.GetController(this, serviceProvider);
            if (Controller != null)
            {
                if (Options.TenancyEnabled)
                {
                    Controller._ForceRunWithTenancy += Controller__ForceRunWithTenancy;
                }
                else
                {
                    Controller._ForceRun += Controller__ForceRun;
                }
            }
        }



        #endregion

        #region Tenancy

        protected readonly List<long> TenantIds = new List<long>();
        protected bool RunAllTenants { get; set; }

        private void Controller__ForceRun(object sender, EventBackgroundServiceEventArgs args)
        {
            ForceRun();
        }

        private void Controller__ForceRunWithTenancy(object sender, EventBackgroundServiceEventWithTenancyArgs args)
        {
            ForceRun(args.Tenant);
        }

        #endregion

        #region IHostedService

        public virtual void ForceRun()
        {
            ResetEvent.Set();
            OnForceRun();
        }

        public virtual void ForceRun(ITenant tenant)
        {
            ForceRun(tenant?.Id);
        }

        public virtual void ForceRun(long? tenantId)
        {
            if (!tenantId.HasValue)
            {
                return;
            }

            if (Options.TenancyEnabled)
            {
                TenantIds.Add(tenantId.Value);
            }

            OnForceRun();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                Logger?.LogInformation($"{GetType()?.Name} started");
                OnStart();

                var isFirstRun = true;
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (isFirstRun && RunOnStartup)
                        {
                            isFirstRun = false;

                            using (var scope = ServiceScopeFactory.CreateScope())
                            {
                                await ExecuteScopedAsync(scope.ServiceProvider, stoppingToken);
                            }
                        }
                        isFirstRun = false;

                        OnWait();
                        Events?.InvokeOnWait(this);
                        ResetEvent.WaitOne();
                        ResetEvent.Reset();
                        OnRun();
                        Events?.InvokeOnRun(this);

                        using (var scope = ServiceScopeFactory.CreateScope())
                        {
                            await ExecuteScopedAsync(scope.ServiceProvider, stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex.ToString());
                        OnError(ex);
                        Events?.InvokeOnError(this, ex);
                    }
                }

                Logger?.LogInformation($"{GetType()?.Name} stopped");
            }, stoppingToken);
        }

        protected abstract Task ExecuteScopedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);

        #endregion
    }

    public abstract class EventBackgroundService<TContext> : EventBackgroundService
       where TContext : notnull
    {
        #region Constructor

        public EventBackgroundService(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        #endregion

        #region IHosted Service

        protected override async Task ExecuteScopedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var context = serviceProvider.GetRequiredService<TContext>();

            if (Options.TenancyEnabled)
            {
                var tenantScope = serviceProvider.GetRequiredService<ITenantScope>();
                var tenantEnumerator = serviceProvider.GetRequiredService<ITenantEnumerator>();

                IEnumerable<ITenant> tenants;
                if (RunAllTenants)
                {
                    tenants = tenantEnumerator.GetEnumerator();
                    TenantIds.Clear();
                }
                else
                {
                    tenants = tenantEnumerator.GetTenants(TenantIds);
                    TenantIds.Clear();
                }

                if (Options.ParallelExecutionEnabled)
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
            else
            {
                await ExecuteScopedAsync(context, cancellationToken);
            }
        }

        protected abstract Task ExecuteScopedAsync(TContext scope, CancellationToken cancellationToken);

        #endregion
    }

    public class EventBackgroundServiceEvents
    {
        internal void InvokeOnRun(object sender)
        {
            OnRun?.Invoke(sender, new EventBackgroundServiceEventArgs());
        }

        internal void InvokeOnWait(object sender)
        {
            OnWait?.Invoke(sender, new EventBackgroundServiceEventArgs());
        }

        internal void InvokeOnError(object sender, Exception error)
        {
            OnError?.Invoke(sender, new EventBackgroundServiceErrorEventArgs() { Error = error });
        }

        public event EventBackgroundServiceEvent OnRun;
        public event EventBackgroundServiceEvent OnWait;
        public event EventBackgroundServiceErrorEvent OnError;

        private static Type _getEventType(Type serviceType)
        {
            return typeof(EventBackgroundServiceEvents<>).MakeGenericType(serviceType);
        }

        internal static EventBackgroundServiceEvents GetEvents(EventBackgroundService backgroundService, IServiceProvider serviceProvider)
        {
            var type = _getEventType(backgroundService.GetType());
            return (EventBackgroundServiceEvents)serviceProvider.GetService(type);
        }
    }

    public class EventBackgroundServiceEvents<TService> : EventBackgroundServiceEvents
        where TService : EventBackgroundService
    { }

    internal delegate void EventBackgroundServiceEventWithTenancy(object sender, EventBackgroundServiceEventWithTenancyArgs args);
    public delegate void EventBackgroundServiceEvent(object sender, EventBackgroundServiceEventArgs args);
    public delegate void EventBackgroundServiceErrorEvent(object sender, EventBackgroundServiceErrorEventArgs args);
    public class EventBackgroundServiceEventArgs { }
    public class EventBackgroundServiceEventWithTenancyArgs
    {
        public ITenant Tenant { get; set; }
    }
    public class EventBackgroundServiceErrorEventArgs
    {
        public Exception Error { get; set; }
    }

    public abstract class EventBackgroundServiceController
    {
        public void ForceRun()
        {
            _ForceRun?.Invoke(this, new EventBackgroundServiceEventArgs());
        }
        public void ForceRun(ITenant tenant)
        {
            _ForceRunWithTenancy?.Invoke(this, new EventBackgroundServiceEventWithTenancyArgs()
            {
                Tenant = tenant
            });
        }

        internal event EventBackgroundServiceEvent _ForceRun;
        internal event EventBackgroundServiceEventWithTenancy _ForceRunWithTenancy;

        private static Type _getEventType(Type serviceType)
        {
            return typeof(EventBackgroundServiceController<>).MakeGenericType(serviceType);
        }

        internal static EventBackgroundServiceController GetController(EventBackgroundService backgroundService, IServiceProvider serviceProvider)
        {
            var type = _getEventType(backgroundService.GetType());
            return (EventBackgroundServiceController)serviceProvider.GetService(type);
        }
    }

    public class EventBackgroundServiceOptions
    {
        public bool TenancyEnabled { get; set; }
        public bool ParallelExecutionEnabled { get; set; }

        private static Type _getOptionsType(Type serviceType)
        {
            return typeof(EventBackgroundServiceOptions<>).MakeGenericType(serviceType);
        }

        internal static EventBackgroundServiceOptions GetOptions(EventBackgroundService backgroundService, IServiceProvider serviceProvider)
        {
            var type = _getOptionsType(backgroundService.GetType());
            return (EventBackgroundServiceOptions)serviceProvider.GetService(type) ?? new EventBackgroundServiceOptions();
        }
    }

    public class EventBackgroundServiceOptions<TService> : EventBackgroundServiceOptions
            where TService : EventBackgroundService
    { }

    public class EventBackgroundServiceOptionsBuilder<TService>
            where TService : EventBackgroundService
    {
        private EventBackgroundServiceOptions<TService> _options = new EventBackgroundServiceOptions<TService>();

        public EventBackgroundServiceOptionsBuilder<TService> UseTenancy()
        {
            _options.TenancyEnabled = true;
            return this;
        }

        public EventBackgroundServiceOptionsBuilder<TService> UseParallelExecution()
        {
            _options.ParallelExecutionEnabled = true;
            return this;
        }

        internal EventBackgroundServiceOptions<TService> Build()
        {
            return _options;
        }
    }

    public class EventBackgroundServiceController<TService> : EventBackgroundServiceController
       where TService : EventBackgroundService
    { }

    public static class EventBackgroundServiceExtensions
    {
        public static void AddEventBackgroundService<TService>(this IServiceCollection services)
            where TService : EventBackgroundService
        {
            services.AddEventBackgroundService<TService>(null);
        }

        public static void AddEventBackgroundService<TService>(this IServiceCollection services, Action<EventBackgroundServiceOptionsBuilder<TService>> builder)
            where TService : EventBackgroundService
        {
            var optionsBuilder = new EventBackgroundServiceOptionsBuilder<TService>();
            builder?.Invoke(optionsBuilder);
            var options = optionsBuilder.Build();

            services.AddSingleton(options);
            services.AddSingleton<EventBackgroundServiceEvents<TService>>();
            services.AddSingleton<EventBackgroundServiceController<TService>>();
            services.AddHostedService<TService>();
        }
    }
}
#pragma warning restore CS8601 // Mögliche Nullverweiszuweisung.
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.