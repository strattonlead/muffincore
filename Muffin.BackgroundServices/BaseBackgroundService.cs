using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Muffin.BackgroundServices
{
    public abstract class BaseBackgroundService : BackgroundService
    {
        #region Properties

        protected readonly IServiceScopeFactory ServiceScopeFactory;
        protected readonly ILogger Logger;

        #endregion

        #region Constructor

        public BaseBackgroundService(IServiceProvider serviceProvider)
        {
            ServiceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            var type = typeof(ILogger<>).MakeGenericType(GetType());
            Logger = (ILogger)serviceProvider.GetService(type);
        }

        #endregion

        #region Actions

        protected virtual void OnStart() { }
        protected virtual void OnForceRun() { }
        protected virtual void OnRun() { }
        protected virtual void OnWait() { }
        protected virtual void OnError(Exception e) { }

        #endregion
    }
}