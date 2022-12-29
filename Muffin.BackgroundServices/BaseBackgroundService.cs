using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            Logger = serviceProvider.GetRequiredService<ILogger<OneTimeBackgroundService>>();
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