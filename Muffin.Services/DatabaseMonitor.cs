using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.Services
{
    public interface IDatabaseMonitor<TContext>
        where TContext : DbContext
    {
        bool CanConnect();
        bool DatabaseExists();
    }

    public class DatabaseMonitor<TContext> : BackgroundService, IDatabaseMonitor<TContext>
        where TContext : DbContext
    {
        #region Properties

        private readonly IServiceScopeFactory ServiceScopeFactory;
        private readonly DatabaseMonitorOptions<TContext> Options;
        private readonly DatabaseMonitorEvents<TContext> Events;
        private readonly DatabaseMonitorState<TContext> State;

        #endregion

        #region Constructor

        public DatabaseMonitor(IServiceProvider serviceProvider)
        {
            ServiceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            Options = serviceProvider.GetRequiredService<DatabaseMonitorOptions<TContext>>();
            Events = serviceProvider.GetRequiredService<DatabaseMonitorEvents<TContext>>();
            State = serviceProvider.GetRequiredService<DatabaseMonitorState<TContext>>();
        }

        #endregion

        #region IDatabaseMonitor

        private bool? _canConnect { get => State.CanConnect; set => State.CanConnect = value; }
        private bool? _databaseExists { get => State.DatabaseExists; set => State.DatabaseExists = value; }

        public bool CanConnect()
        {
            var canConnect = _invokeScoped(dbContext => dbContext.Database.CanConnect());
            if (!_canConnect.HasValue || (_canConnect.HasValue && _canConnect.Value != canConnect))
            {
                _canConnect = canConnect;
                Events.InvokeOnCanConnectChanged(this, new DatabaseMonitorChangedEventArgs<TContext>()
                {
                    CanConnect = _canConnect,
                    DatabaseExists = _databaseExists
                });
            }
            return canConnect;
        }

        public bool DatabaseExists()
        {
            var databaseExists = _invokeScoped(dbContext => (dbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists());
            if (!_databaseExists.HasValue || (_databaseExists.HasValue && _databaseExists.Value != databaseExists))
            {
                _databaseExists = databaseExists;
                Events.InvokeOnCanConnectChanged(this, new DatabaseMonitorChangedEventArgs<TContext>()
                {
                    CanConnect = _canConnect,
                    DatabaseExists = _databaseExists
                });
            }
            return databaseExists;
        }

        #region IHostedService

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                CanConnect();
                await Task.Delay(Options.Interval, stoppingToken);
            }
        }

        #endregion

        #endregion

        #region Helper

        private TResult _invokeScoped<TResult>(Func<TContext, TResult> action)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            using (var dbContext = scope.ServiceProvider.GetRequiredService<TContext>())
            {
                return action.Invoke(dbContext);
            }
        }

        #endregion

    }

    public class DatabaseMonitorState<TContext>
            where TContext : DbContext
    {
        public bool? CanConnect { get; internal set; }
        public bool? DatabaseExists { get; internal set; }
    }

    public class DatabaseMonitorEvents<TContext>
            where TContext : DbContext
    {
        public event DatabaseMonitorChangedEvent<TContext> OnCanConnectChanged;
        public event DatabaseMonitorChangedEvent<TContext> OnDatabaseCreated;

        internal void InvokeOnCanConnectChanged(object sender, DatabaseMonitorChangedEventArgs<TContext> args)
        {
            OnCanConnectChanged?.Invoke(sender, args);
        }

        internal void InvokeOnDatabaseCreated(object sender, DatabaseMonitorChangedEventArgs<TContext> args)
        {
            OnDatabaseCreated?.Invoke(sender, args);
        }
    }

    public delegate void DatabaseMonitorChangedEvent<TContext>(object sender, DatabaseMonitorChangedEventArgs<TContext> args)
            where TContext : DbContext;

    public class DatabaseMonitorChangedEventArgs<TContext>
            where TContext : DbContext
    {
        public bool? CanConnect { get; set; }
        public bool? DatabaseExists { get; set; }
    }

    public class DatabaseMonitorOptions<TContext>
    {
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);
    }

    public class DatabaseMonitorOptionsBuilder<TContext>
    {
        private DatabaseMonitorOptions<TContext> options = new DatabaseMonitorOptions<TContext>();

        public DatabaseMonitorOptionsBuilder<TContext> Interval(TimeSpan interval)
        {
            options.Interval = interval;
            return this;
        }

        public DatabaseMonitorOptions<TContext> Build()
        {
            return options;
        }
    }

    public static class DatabaseMonitorExtensions
    {
        public static void AddDatabaseMonitor<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            services.AddDatabaseMonitor<TContext>(null);
        }

        public static void AddDatabaseMonitor<TContext>(this IServiceCollection services, Action<DatabaseMonitorOptionsBuilder<TContext>> builder)
            where TContext : DbContext
        {
            var b = new DatabaseMonitorOptionsBuilder<TContext>();
            builder?.Invoke(b);
            var options = b.Build();

            services.AddSingleton(options);
            services.AddSingleton<DatabaseMonitorState<TContext>>();
            services.AddSingleton<DatabaseMonitorEvents<TContext>>();
            services.AddSingleton<IDatabaseMonitor<TContext>, DatabaseMonitor<TContext>>();
            services.AddHostedService(p => (DatabaseMonitor<TContext>)p.GetRequiredService<IDatabaseMonitor<TContext>>());
        }
    }
}
