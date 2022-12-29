using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        private readonly SqlScriptRunnerOptions<TContext> Options;

        #endregion

        #region Constructor

        public SqlScriptRunnerBackgroundService(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Options = serviceProvider.GetRequiredService<SqlScriptRunnerOptions<TContext>>();
        }

        #endregion

        #region IHostedService

        protected override Task ExecuteScopedAsync(ISqlScriptRunner<TContext> scope, CancellationToken cancellationToken)
        {
            scope.RunAllScriptsInAssembly();
            //var assembly = typeof(TContext).Assembly;
            //var resourceNames = assembly
            //    .GetManifestResourceNames()
            //    .Where(x => x.EndsWith(".sql"))
            //    .ToArray();

            //foreach (var resourceName in resourceNames)
            //{
            //    using (var stream = assembly.GetManifestResourceStream(resourceName))
            //    {
            //        using (var reader = new StreamReader(stream))
            //        {
            //            var sql = reader.ReadToEnd();
            //            if (!string.IsNullOrWhiteSpace(sql))
            //            {
            //                scope.RunSqlScript(sql);
            //            }
            //        }
            //    }
            //}

            return Task.CompletedTask;
        }

        //public void RunSqlScript(TContext dbContext, string sql)
        //{
        //    try
        //    {
        //        var batches = sql.Split(new[] { "\nGO" }, StringSplitOptions.None);
        //        foreach (string batch in batches)
        //        {
        //            dbContext.Database.ExecuteSqlRaw(batch);
        //        }
        //    }
        //    catch { }
        //}

        #endregion
    }

    public class SqlScriptRunner<TContext> : ISqlScriptRunner<TContext>
        where TContext : DbContext
    {
        #region Properties

        private readonly TContext DbContext;

        #endregion

        #region Constructor

        public SqlScriptRunner(IServiceProvider serviceProvider)
        {
            DbContext = serviceProvider.GetRequiredService<TContext>();
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

                foreach (var resourceName in resourceNames)
                {
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
                var batches = sql.Split(new[] { "\nGO" }, StringSplitOptions.None);
                foreach (string batch in batches)
                {
                    DbContext.Database.ExecuteSqlRaw(batch);
                }
                return true;
            }
            catch { }
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
