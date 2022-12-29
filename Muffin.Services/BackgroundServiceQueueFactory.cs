using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Muffin.Services
{
    public class BackgroundServiceQueueFactory
    {
        #region Properties

        private readonly IServiceProvider ServiceProvider;
        private readonly BackgroundServiceQueueEvents Events;
        private Dictionary<object, IBackgroundServiceQueue> Queues = new Dictionary<object, IBackgroundServiceQueue>();

        #endregion

        #region Constructor

        public BackgroundServiceQueueFactory(IServiceProvider serviceProvider)
        {
            Events = serviceProvider.GetRequiredService<BackgroundServiceQueueEvents>();
            ServiceProvider = serviceProvider;

            Events.OnQueueEnded += Events_OnQueueEnded;
        }

        private void Events_OnQueueEnded(IBackgroundServiceQueue queue)
        {
            if (queue.Owner != null && Queues.ContainsKey(queue.Owner))
            {
                Queues.Remove(queue.Owner);
            }
        }

        #endregion

        #region Actions

        public BackgroundServiceQueue CreateQueue(object owner)
        {
            if (Queues.TryGetValue(owner, out var queue))
            {
                return queue as BackgroundServiceQueue;
            }

            queue = new BackgroundServiceQueue(owner, ServiceProvider);
            Queues[owner] = queue;
            return queue as BackgroundServiceQueue;
        }

        #endregion
    }

    public class BackgroundServiceQueueEvents
    {
        public event BackgroundServiceQueueItemEvent OnQueueItemDequeued;
        internal void InvokeOnQueueItemDequeued(IBackgroundServiceQueue queue, IQueueItem queueItem)
        {
            OnQueueItemDequeued?.Invoke(queue, queueItem);
        }

        public event BackgroundServiceQueueItemEvent OnQueueItemEnqueued;
        internal void InvokeOnQueueItemEnqueued(IBackgroundServiceQueue queue, IQueueItem queueItem)
        {
            OnQueueItemEnqueued?.Invoke(queue, queueItem);
        }

        public event BackgroundServiceQueueEvent OnQueueStarted;
        internal void InvokeOnQueueStarted(IBackgroundServiceQueue queue)
        {
            OnQueueStarted?.Invoke(queue);
        }

        public event BackgroundServiceQueueEvent OnQueueEnded;
        internal void InvokeOnQueueEnded(IBackgroundServiceQueue queue)
        {
            OnQueueEnded?.Invoke(queue);
        }

        public event BackgroundServiceQueueExecutionContextEvent OnQueueItemExecuting;
        internal void InvokeOnQueueItemExecuting(IBackgroundServiceQueue queue, IExecutionContext context)
        {
            OnQueueItemExecuting?.Invoke(queue, context);
        }

        public event BackgroundServiceQueueExecutionContextEvent OnQueueItemExecuted;
        internal void InvokeOnQueueItemExecuted(IBackgroundServiceQueue queue, IExecutionContext context)
        {
            OnQueueItemExecuted?.Invoke(queue, context);
        }
    }

    public delegate void BackgroundServiceQueueEvent(IBackgroundServiceQueue queue);
    public delegate void BackgroundServiceQueueExecutionContextEvent(IBackgroundServiceQueue queue, IExecutionContext context);
    public delegate void BackgroundServiceQueueItemEvent(IBackgroundServiceQueue queue, IQueueItem queueItem);

    public static class BackgroundServiceQueueFactoryExtensions
    {
        public static void AddBackgroundServiceQueueFactory(this IServiceCollection services)
        {
            services.AddSingleton<BackgroundServiceQueueEvents>();
            services.AddSingleton<BackgroundServiceQueueFactory>();
        }
    }
}
