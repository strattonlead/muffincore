using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.Services
{
    public interface IBackgroundServiceQueue
    {
        object Owner { get; }
    }
    public interface IQueueItem { }
    public interface IExecutionContext { }

    /// <summary>
    /// Queue um Bakcground Tasks einzufädeln und sequentiell asynchron auszuführen. Die Referenz auf dieses Objekt muss bestehen bleiben! Das ist kein Consumable Service!!
    /// </summary>
    public class BackgroundServiceQueue : IBackgroundServiceQueue
    {
        #region Properties

        public object Owner { get; private set; }
        private readonly List<QueueItem> Queue = new List<QueueItem>();
        private readonly CancellationTokenSource CancellationTokenSource;
        private CancellationToken CancellationToken => CancellationTokenSource?.Token ?? default;
        private Task WorkerTask { get; set; }
        private readonly IHostApplicationLifetime ApplicationLifetime;
        private readonly ManualResetEvent ResetEvent = new ManualResetEvent(false);
        private readonly IServiceProvider ServiceProvider;
        private readonly BackgroundServiceQueueEvents Events;

        #endregion

        #region Constructors

        public BackgroundServiceQueue(object owner, IServiceProvider serviceProvider)
        {
            Owner = owner;
            ServiceProvider = serviceProvider;
            Events = serviceProvider.GetRequiredService<BackgroundServiceQueueEvents>();
            ApplicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            CancellationTokenSource = new CancellationTokenSource();
            ApplicationLifetime.ApplicationStopping.Register(() => CancellationTokenSource.Cancel());
            WorkerTask = Task.Run(_doWork, CancellationToken);
        }

        ~BackgroundServiceQueue()
        {
            while (!WorkerTask.IsCompleted || !WorkerTask.IsCanceled)
            {
                CancellationTokenSource.Cancel();
            }
            WorkerTask = null;
        }

        #endregion

        #region Enqueue Logic

        public void Enqueue(Action<ExecutionContext> action)
        {
            Enqueue(null, null, action);
        }

        public void Enqueue(int? priority, Action<ExecutionContext> action)
        {
            Enqueue(priority, null, action);
        }

        public void Enqueue(object parameterContext, Action<ExecutionContext> action)
        {
            Enqueue(null, parameterContext, action);
        }

        public void Enqueue(int? priority, object parameterContext, Action<ExecutionContext> action)
        {
            var queueItem = new QueueItem()
            {
                //Key = key,
                Priority = priority,
                ParameterContext = parameterContext,
                Action = action
            };
#warning TODO merge handler
            Queue.Add(queueItem);
            Events.InvokeOnQueueItemEnqueued(this, queueItem);
            Notify();
        }

        public void Enqueue(Func<ExecutionContext, Task> asyncAction)
        {
            Enqueue(default, null, asyncAction);
        }

        public void Enqueue(int? priority, Func<ExecutionContext, Task> asyncAction)
        {
            Enqueue(priority, null, asyncAction);
        }

        public void Enqueue(object parameterContext, Func<ExecutionContext, Task> asyncAction)
        {
            Enqueue(null, parameterContext, asyncAction);
        }

        public void Enqueue(int? priority, object parameterContext, Func<ExecutionContext, Task> asyncAction)
        {
            var queueItem = new QueueItem()
            {
                Priority = priority,
                ParameterContext = parameterContext,
                AsyncAction = asyncAction
            };
#warning TODO merge handler
            Queue.Add(queueItem);
            Events.InvokeOnQueueItemEnqueued(this, queueItem);
            Notify();
        }

        #endregion

        #region Tasks

        public void Notify()
        {
            ResetEvent.Set();
        }

        private async Task _doWork()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                try
                {
                    while (!CancellationToken.IsCancellationRequested && Queue.Any())
                    {
                        Events.InvokeOnQueueStarted(this);

                        var queueItem = Queue.OrderByDescending(x => x.Priority).FirstOrDefault();
                        Queue.Remove(queueItem);
                        Events.InvokeOnQueueItemDequeued(this, queueItem);

                        var context = new ExecutionContext()
                        {
                            Queue = this,
                            Owner = Owner,
                            ServiceProvider = ServiceProvider,
                            QueueItem = queueItem
                        };

                        if (queueItem.AsyncAction != null)
                        {
                            Events.InvokeOnQueueItemExecuting(this, context);
                            await queueItem.AsyncAction(context);
                            Events.InvokeOnQueueItemExecuted(this, context);
                        }
                        else if (queueItem.Action != null)
                        {
                            Events.InvokeOnQueueItemExecuting(this, context);
                            queueItem.Action(context);
                            Events.InvokeOnQueueItemExecuted(this, context);
                        }
                    };

                    Events.InvokeOnQueueEnded(this);
                }
                catch { }

                if (!CancellationToken.IsCancellationRequested)
                {
                    ResetEvent.Reset();
                    ResetEvent.WaitOne();
                }
            }
        }

        #endregion

        #region Helper

        public class ExecutionContext : IExecutionContext
        {
            public IBackgroundServiceQueue Queue { get; set; }
            public object Owner { get; set; }
            public object ParameterContext => QueueItem?.ParameterContext;
            public IServiceProvider ServiceProvider { get; set; }
            public QueueItem QueueItem { get; set; }
        }

        public class QueueItem : IQueueItem
        {
            public int? Priority { get; internal set; }
            //public T Key { get; internal set; }
            public object ParameterContext { get; internal set; }
            internal Action<ExecutionContext> Action { get; set; }
            internal Func<ExecutionContext, Task> AsyncAction { get; set; }
        }

        #endregion
    }
}
