using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muffin.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Delegates;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace Muffin.SqlDependency
{
    /// <summary>
    /// Lauscht auf der TIDE Pro Datenbank MONATSSALDEN + TAGESSALDEN um Änderungen zu erfassen. Singleton Instanzen
    /// </summary>
    public class GenericChangeListener<T> : IHostedService, IGenericChangeListener<T>
        where T : class, new()
    {
        #region Properties

        private readonly ILogger Logger;
        private readonly IConfigurationRoot Configuration;
        private readonly SlidingDelayedInvocation ReconnectDelayedInvocation;
        private readonly GenericChangeListenerEvents<T> Events;
        private readonly IHostApplicationLifetime ApplicationLifetime;
        private readonly GenericChangeListenerOptions<T> Options;

        private SqlTableDependency<T> TableDependency;

        private bool IsStopping;
        public ReadyState State { get; private set; }
        private Task FallbackCheckerTask;

        #endregion

        #region Constructor

        public GenericChangeListener(IServiceProvider serviceProvider)
        {
            Logger = serviceProvider.GetRequiredService<ILogger<GenericChangeListener<T>>>();
            Configuration = serviceProvider.GetRequiredService<IConfigurationRoot>();
            ApplicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            Events = serviceProvider.GetRequiredService<GenericChangeListenerEvents<T>>();
            Options = serviceProvider.GetService<GenericChangeListenerOptions<T>>();
            if (Options == null)
            {
                Options = new GenericChangeListenerOptions<T>();
            }

            ReconnectDelayedInvocation = new SlidingDelayedInvocation(Options.ReconnectTimeout);

            GenericChangeListenerHelper.AddGenericChangeListenerInstance(this);
        }

        #endregion

        #region Actions

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Start Service {nameof(GenericChangeListener<T>)}<{typeof(T).Name}>");

            ApplicationLifetime?.ApplicationStopping.Register(() =>
            {
                IsStopping = true;
            });

            if (Options.UseReInitializeTimeout)
            {
                var isFirstRun = true;
                FallbackCheckerTask = Task.Run(async () =>
                {
                    Logger?.LogInformation($"Info SqlTableDependency on Table {typeof(T).Name} fallback task started...");
                    while (!IsStopping)
                    {
                        if (isFirstRun)
                        {
                            Logger?.LogInformation($"Info SqlTableDependency on Table {typeof(T).Name} first run, skip restart...");
                        }

                        try
                        {
                            if (!isFirstRun)
                            {
                                await RestartTableDependencies();
                            }
                            isFirstRun = false;
                        }
                        catch (Exception e)
                        {
                            Logger?.LogError($"Error SqlTableDependency on Table {typeof(T).Name} in fallback task...");
                            Logger?.LogError(e.ToString());
                        }

                        if (!IsStopping)
                        {
                            try
                            {
                                await Task.Delay(Options.ReInitializeTimeout);
                            }
                            catch { }
                        }

                    }
                    Logger?.LogInformation($"Info SqlTableDependency on Table {typeof(T).Name} fallback task stopped.");
                }, ApplicationLifetime.ApplicationStopping);
            }


            RestartTableDependencies();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Logger?.LogInformation($"Stop SqlTableDependency on Table {typeof(T).Name}...");
                TableDependency?.Stop();
                Logger?.LogInformation($"Stoped SqlTableDependency on Table {typeof(T).Name}...");
            }, cancellationToken);
        }

        private void TableDependency_OnError(object sender, ErrorEventArgs e)
        {
            State = ReadyState.NotReady;
            Events.InvokeOnError(sender, e);
            Logger?.LogError(e.Error.Message);
            if (!IsStopping)
            {
                Logger?.LogInformation($"Try reconnect SqlTableDependency after {ReconnectDelayedInvocation.DefaultTimeout.TotalSeconds} s");
                ReconnectDelayedInvocation.InvokeAfterDelay(() =>
                {
                    Logger?.LogInformation($"Try reconnect SqlTableDependency");
                    RestartTableDependencies().ContinueWith(x =>
                    {
                        Logger?.LogInformation($"Reconnected SqlTableDependency");

                    });
                });
            }
        }

        private Task RestartTableDependencies()
        {
            return Task.Run(() =>
            {
                if (TableDependency != null
                    && TableDependency.Status != TableDependencyStatus.Started
                    && TableDependency.Status != TableDependencyStatus.Starting)
                {
                    try
                    {
                        Logger?.LogInformation($"Stop SqlTableDependency on Table {typeof(T).Name}...");
                        State = ReadyState.NotReady;
                        TableDependency.Stop();
                        Logger?.LogInformation($"Stoped SqlTableDependency on Table {typeof(T).Name}...");
                    }
                    catch (Exception e)
                    {
                        Logger?.LogError($"Error on SqlTableDependency on Table {typeof(T).Name}...");
                        Logger?.LogError(e.Message);
                    }
                }

                var connectionString = Options.ConnectionString;
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    connectionString = Configuration.GetConnectionString("DefaultConnection");
                }

                while (true)
                {
                    try
                    {
                        Logger?.LogInformation($"Initialize SqlTableDependency on Table {typeof(T).Name}...");
                        TableDependency = new SqlTableDependency<T>(connectionString, typeof(T).Name, includeOldValues: true);
                        TableDependency.OnChanged += TableDependency_OnChanged;
                        TableDependency.OnError += TableDependency_OnError;
                        TableDependency.OnStatusChanged += TableDependency_OnStatusChanged;
                        TableDependency.Start();
                        Logger?.LogInformation($"SqlTableDependency started on Table {typeof(T).Name}...");
                        State = ReadyState.Ready;
                        break;
                    }
                    catch (Exception e)
                    {
                        Logger?.LogError($"Error on SqlTableDependency on Table {typeof(T).Name}...");
                        Logger?.LogError(e.Message);
                        Logger?.LogError($"Retry connect SqlTableDependency on Table {typeof(T).Name} in 30 seconds...");
                        Thread.Sleep(TimeSpan.FromSeconds(30));
                    }
                }
            }, ApplicationLifetime.ApplicationStopping);
        }

        private void TableDependency_OnStatusChanged(object sender, StatusChangedEventArgs e)
        {

            if (e.Status == TableDependencyStatus.WaitingForNotification)
            {
                State = ReadyState.Ready;
            }
            else
            {
                State = ReadyState.NotReady;
            }

            GenericChangeListenerHelper.InvokeStatusChange();
            Events.InvokeOnStatusChanged(sender, e);
        }

        private void TableDependency_OnChanged(object sender, RecordChangedEventArgs<T> e)
        {
            Logger?.LogInformation($"TableDependency<{typeof(T).Name}> -> {e.ChangeType}");
            Events.InvokeOnChangedOriginal(sender, e);
            Events.InvokeOnChanged(sender, e.ChangeType, e.Entity, e.EntityOldValues);
            switch (e.ChangeType)
            {
                case ChangeType.Insert:
                    Events.InvokeOnInsert(sender, e.Entity);
                    break;
                case ChangeType.Update:
                    Events.InvokeOnUpdate(sender, e.Entity, e.EntityOldValues);
                    break;
                case ChangeType.Delete:
                    Events.InvokeOnDelete(sender, e.Entity);
                    break;
                case ChangeType.None:
                default:
                    break;
            }
        }

        #endregion
    }

    public enum ReadyState
    {
        NotReady = 0,
        Ready = 1
    }

    public interface IGenericChangeListener
    {
        ReadyState State { get; }
    }

    public delegate void ReadyStateEvent(ReadyState readyState);

    public interface IGenericChangeListener<T> : IGenericChangeListener
        where T : class, new()
    { }

    /// <summary>
    /// Die Events werden in den IHostedService injected und in die Dienste die die Events brauchen. Die Events sind dann ein Singleton!!
    /// Das wird beides via GenericChangeListenerHelper.AddGenericChangeListener<T> hinzugefügt.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericChangeListenerEvents<T>
        where T : class, new()
    {
        #region Events

        public event ErrorEventHandler OnError;
        public event ChangedEventHandler<T> OnChangedOriginal;
        public event EntityChangedEventHandler OnChanged;
        public event EntityInsertEventHandler OnInsert;
        public event EntityUpdateEventHandler OnUpdate;
        public event EntityDeleteEventHandler OnDelete;
        public event StatusEventHandler OnStatusChanged;

        #endregion

        #region Invoke

        public void InvokeOnError(object sender, ErrorEventArgs args) { OnError?.Invoke(sender, args); }
        public void InvokeOnChangedOriginal(object sender, RecordChangedEventArgs<T> args) { OnChangedOriginal?.Invoke(sender, args); }
        public void InvokeOnChanged(object sender, ChangeType changeType, T entity, T oldEntity) { OnChanged?.Invoke(sender, changeType, entity, oldEntity); }
        public void InvokeOnInsert(object sender, T entity) { OnInsert?.Invoke(sender, entity); }
        public void InvokeOnUpdate(object sender, T entity, T oldEntity) { OnUpdate?.Invoke(sender, entity, oldEntity); }
        public void InvokeOnDelete(object sender, T entity) { OnDelete?.Invoke(sender, entity); }
        public void InvokeOnStatusChanged(object sender, StatusChangedEventArgs args) { OnStatusChanged?.Invoke(sender, args); }

        #endregion

        #region Delegates

        public delegate void EntityChangedEventHandler(object sender, ChangeType changeType, T entity, T oldEntity);
        public delegate void EntityInsertEventHandler(object sender, T entity);
        public delegate void EntityUpdateEventHandler(object sender, T entity, T oldEntity);
        public delegate void EntityDeleteEventHandler(object sender, T entity);

        #endregion
    }

    public static class GenericChangeListenerHelper
    {
        private static List<IGenericChangeListener> RegisteredChangeListeners = new List<IGenericChangeListener>();
        public static event ReadyStateEvent OnListenerStatesChanged;

        public static void AddGenericChangeListenerInstance(IGenericChangeListener genericChangeListener)
        {
            RegisteredChangeListeners.Add(genericChangeListener);
        }

        public static bool AreAllServicesReady()
        {
            return RegisteredChangeListeners.All(x => x.State == ReadyState.Ready);
        }

        public static void InvokeStatusChange()
        {
            if (AreAllServicesReady())
            {
                OnListenerStatesChanged?.Invoke(ReadyState.Ready);
            }
            else
            {
                OnListenerStatesChanged?.Invoke(ReadyState.NotReady);
            }
        }

        public static void AddGenericChangeListener<T>(this IServiceCollection services)
            where T : class, new()
        {
            services.AddGenericChangeListener<T>(null);
        }

        public static void AddGenericChangeListener<T>(this IServiceCollection services, Action<GenericChangeListenerOptionsBuilder<T>> optionsBuilder)
            where T : class, new()
        {
            if (optionsBuilder != null)
            {
                var builder = new GenericChangeListenerOptionsBuilder<T>();
                optionsBuilder(builder);
                var options = builder.Build();
                services.AddSingleton(options);
            }
            services.AddSingleton<GenericChangeListenerEvents<T>>();
            services.AddHostedService<GenericChangeListener<T>>();
        }
    }

    public class GenericChangeListenerOptions<T>
            where T : class, new()
    {
        public string ConnectionString { get; set; }
        public TimeSpan ReconnectTimeout { get; set; } = TimeSpan.FromSeconds(181);
        public bool UseReInitializeTimeout { get; set; }
        public TimeSpan ReInitializeTimeout { get; set; } = TimeSpan.FromHours(6);
    }

    public class GenericChangeListenerOptionsBuilder<T>
            where T : class, new()
    {
        private GenericChangeListenerOptions<T> Options = new GenericChangeListenerOptions<T>();

        /// <summary>
        /// Der Datenbank Connection String zu dem Verbunden werden soll
        /// </summary>
        public GenericChangeListenerOptionsBuilder<T> UseConnectionString(string connectionString)
        {
            Options.ConnectionString = connectionString;
            return this;
        }

        /// <summary>
        /// Default sind 181 Sekunden
        /// </summary>
        public GenericChangeListenerOptionsBuilder<T> UseReconnectTimeout(TimeSpan reconnectTimeout)
        {
            Options.ReconnectTimeout = reconnectTimeout;
            return this;
        }

        /// <summary>
        /// Gibt an ob der Listener alle X Minuten neu aufgebaut werden soll
        /// </summary>
        public GenericChangeListenerOptionsBuilder<T> UseReInitializeTimeout(TimeSpan reconnectTimeout)
        {
            Options.UseReInitializeTimeout = true;
            Options.ReInitializeTimeout = reconnectTimeout;
            return this;
        }

        public GenericChangeListenerOptions<T> Build()
        {
            return Options;
        }
    }
}
