using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muffin.BackgroundServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.Services
{
    public interface ISqlScriptRunner<TContext>
        where TContext : DbContext
    {
        bool RunAllScriptsInAssembly();
        bool RunAllScriptsInAssemblies(params Assembly[] assemblies);
        bool RunSqlScript(string sql);
    }

    public class SqlScriptRunnerBackgroundService<TContext> : OneTimeBackgroundServiceWithTenancy<ISqlScriptRunner<TContext>>
        where TContext : DbContext
    {
        #region Properties

        private readonly SqlScriptRunnerOptions<TContext> _options;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public SqlScriptRunnerBackgroundService(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _options = serviceProvider.GetRequiredService<SqlScriptRunnerOptions<TContext>>();
            _logger = serviceProvider.GetService<ILogger<SqlScriptRunnerBackgroundService<TContext>>>();
        }

        #endregion

        #region IHostedService

        protected override Task ExecuteScopedAsync(ISqlScriptRunner<TContext> scope, CancellationToken cancellationToken)
        {
            if (_options.Assemblies != null && _options.Assemblies.Any())
            {
                foreach (var assembly in _options.Assemblies)
                {
                    _logger?.LogInformation($"Run ExecuteScopedAsync in assembly: {assembly.FullName}");
                }

                scope.RunAllScriptsInAssemblies(_options.Assemblies.ToArray());
            }
            else
            {
                var assembly = typeof(TContext).Assembly;
                _logger?.LogInformation($"Run ExecuteScopedAsync in own assembly: {assembly.FullName}");
                scope.RunAllScriptsInAssembly();
            }

            return Task.CompletedTask;
        }

        #endregion
    }

    public class SqlScriptRunner<TContext> : ISqlScriptRunner<TContext>
        where TContext : DbContext
    {
        #region Properties

        private readonly TContext _dbContext;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public SqlScriptRunner(IServiceProvider serviceProvider)
        {
            _dbContext = serviceProvider.GetRequiredService<TContext>();
            _logger = serviceProvider.GetService<ILogger<SqlScriptRunner<TContext>>>();
        }

        #endregion

        #region ISqlScriptRunner<TContext>

        public bool RunAllScriptsInAssemblies(params Assembly[] assemblies)
        {
            var success = true;
            foreach (var assembly in assemblies)
            {
                var resourceNames = assembly
                    .GetManifestResourceNames()
                    .Where(x => x.EndsWith(".sql"))
                    .ToArray();

                _logger?.LogInformation($"Found {resourceNames.Length} sql files in assembly: {assembly.FullName}");
                foreach (var resourceName in resourceNames)
                {
                    _logger?.LogInformation(resourceName);
                }

                foreach (var resourceName in resourceNames)
                {
                    _logger?.LogInformation($"Execute {resourceName}");
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var sql = reader.ReadToEnd();
                            if (!string.IsNullOrWhiteSpace(sql))
                            {
                                success &= RunSqlScript(sql);
                            }
                        }
                    }
                }
            }
            return success;
        }

        public bool RunAllScriptsInAssembly()
        {
            var assembly = typeof(TContext).Assembly;
            return RunAllScriptsInAssemblies(assembly);
        }

        public bool RunSqlScript(string sql)
        {
            try
            {
                var batches = sql.Split(new[] { "\nGO" }, StringSplitOptions.None).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                foreach (string batch in batches)
                {
                    _logger?.LogInformation($"Execute SQL: {batch}");
                    _dbContext.Database.ExecuteSqlRaw(batch);
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to execure sql: {e.Message}");
            }
            return false;
        }

        #endregion
    }

    public class SqlScriptRunnerOptions<TContext>
    {
        internal List<Assembly> Assemblies = new List<Assembly>();

    }

    public class SqlScriptRunnerOptionsBuilder<TContext>
    {
        internal SqlScriptRunnerOptions<TContext> Options { get; set; } = new SqlScriptRunnerOptions<TContext>();

        public SqlScriptRunnerOptionsBuilder<TContext> AddAssembly(Assembly assembly)
        {
            if (!Options.Assemblies.Contains(assembly))
            {
                Options.Assemblies.Add(assembly);
            }
            return this;
        }
    }

    public static class SqlScriptRunnerExtensions
    {
        public static void AddSqlScriptRunner<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            services.AddSqlScriptRunner<TContext>(x => { });
        }

        public static void AddSqlScriptRunner<TContext>(this IServiceCollection services, Action<SqlScriptRunnerOptionsBuilder<TContext>> builder)
            where TContext : DbContext
        {
            var optionsBuilder = new SqlScriptRunnerOptionsBuilder<TContext>();
            builder?.Invoke(optionsBuilder);

            services.AddSingleton(optionsBuilder.Options);
            services.AddScoped<ISqlScriptRunner<TContext>, SqlScriptRunner<TContext>>();
            services.AddHostedService<SqlScriptRunnerBackgroundService<TContext>>();
        }
    }
}
