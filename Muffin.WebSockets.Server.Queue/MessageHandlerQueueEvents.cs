using System;
using System.Collections.Generic;
using System.Linq;

namespace Muffin.WebSockets.Server.Queue
{
    public class MessageHandlerQueueEvents<TClientInterface>
    {
        public event MessageHandlerQueueEvent<TClientInterface> OnStartRunning;
        internal void InvokeOnStartRunning(object sender, MessageHandlerQueueEventArgs<TClientInterface> args)
        {
            OnStartRunning?.Invoke(sender, args);
        }

        public event MessageHandlerQueueEvent<TClientInterface> OnEndRunning;
        internal void InvokeOnEndRunning(object sender, MessageHandlerQueueEventArgs<TClientInterface> args)
        {
            OnEndRunning?.Invoke(sender, args);
        }

        public event MessageHandlerQueueEvent<TClientInterface> OnBeforeEnqueue;
        internal void InvokeOnBeforeEnqueue(object sender, MessageHandlerQueueEventArgs<TClientInterface> args)
        {
            OnBeforeEnqueue?.Invoke(sender, args);
        }

        public event MessageHandlerQueueEvent<TClientInterface> OnEnqueue;
        internal void InvokeOnEnqueue(object sender, MessageHandlerQueueEventArgs<TClientInterface> args)
        {
            OnEnqueue?.Invoke(sender, args);
        }

        public event MessageHandlerQueueEvent<TClientInterface> OnDequeue;
        internal void InvokeOnDequeue(object sender, MessageHandlerQueueEventArgs<TClientInterface> args)
        {
            OnDequeue?.Invoke(sender, args);
        }

        public event MessageHandlerQueueEvent<TClientInterface> OnSkip;
        internal void InvokeOnSkip(object sender, MessageHandlerQueueEventArgs<TClientInterface> args)
        {
            OnSkip?.Invoke(sender, args);
        }

        public event MessageHandlerQueueEvent<TClientInterface> OnBeforeSend;
        internal void InvokeOnBeforeSend(object sender, MessageHandlerQueueEventArgs<TClientInterface> args)
        {
            OnBeforeSend?.Invoke(sender, args);
        }

        public event MessageHandlerQueueEvent<TClientInterface> OnSent;
        internal void InvokeOnSent(object sender, MessageHandlerQueueEventArgs<TClientInterface> args)
        {
            OnSent?.Invoke(sender, args);
        }

        public event MessageHandlerQueueEvent<TClientInterface> OnError;
        internal void InvokeOnError(object sender, MessageHandlerQueueEventArgs<TClientInterface> args)
        {
            OnError?.Invoke(sender, args);
        }
    }

    public class MessageHandlerQueueEventArgs<TClientInterface>
    {
        #region Properties

        public QueueItem<TClientInterface> QueueItem { get; set; }
        public string[] Receivers { get; set; }
        public Exception Exception { get; set; }

        #endregion

        #region Constructor

        public MessageHandlerQueueEventArgs() { }
        public MessageHandlerQueueEventArgs(QueueItem<TClientInterface> queueItem, Exception exception)
        {
            QueueItem = queueItem;
            Exception = exception;
        }
        public MessageHandlerQueueEventArgs(Exception exception)
        {
            Exception = exception;
        }
        public MessageHandlerQueueEventArgs(QueueItem<TClientInterface> queueItem)
        {
            QueueItem = queueItem;
        }
        public MessageHandlerQueueEventArgs(QueueItem<TClientInterface> queueItem, IEnumerable<string> receivers)
            : this(queueItem)
        {
            Receivers = receivers?.ToArray();
        }

        #endregion
    }

    public delegate void MessageHandlerQueueEvent<TClientInterface>(object sender, MessageHandlerQueueEventArgs<TClientInterface> args);
}
