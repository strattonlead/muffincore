using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Muffin.Common.Util
{
    public class MreDispatcher : IDisposable
    {
        #region Properties

        public string Name { get; private set; }
        public Thread DispatcherThread { get; private set; }
        public ConcurrentQueue<Action> ActionQuene { get; private set; } = new ConcurrentQueue<Action>();

        #endregion

        #region Factory

        private MreDispatcher(string name)
        {
            Name = name;
            DispatcherThread = new Thread(new ThreadStart(_run));
            DispatcherThread.Start();
        }

        private static Dictionary<string, MreDispatcher> _cache = new Dictionary<string, MreDispatcher>();
        private static object _createLock = new object();
        public static MreDispatcher GetDispatcher(string name)
        {
            lock (_createLock)
            {
                if (_cache.ContainsKey(name))
                    return _cache[name];
                var model = new MreDispatcher(name);
                _cache.Add(name, model);
                return model;
            }
        }

        #endregion

        #region Dispatch

        private ManualResetEvent mre = new ManualResetEvent(false);
        private bool running = true;
        private bool waiting = false;
        private static object _lock = new object();
        private void _run()
        {
            while (running)
            {
                lock (_lock)
                    if (!ActionQuene.Any())
                    {
                        waiting = true;
                        mre.WaitOne();
                        waiting = false;
                        mre.Reset();
                    }

                Action action;
                while (ActionQuene.TryDequeue(out action))
                {
                    action();
                }
            }
        }

        public void DispatchAsync(Action action)
        {
            if (action == null)
                return;
            ActionQuene.Enqueue(action);
            if (waiting)
                mre.Set();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    DispatcherThread = null;
                    if (_cache.ContainsKey(Name))
                        _cache.Remove(Name);
                }
                disposed = true;
            }
        }

        ~MreDispatcher()
        {
            Dispose(false);
        }

        #endregion
    }
}
