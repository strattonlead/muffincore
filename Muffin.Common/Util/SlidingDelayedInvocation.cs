using System;
using System.Timers;

namespace Muffin.Common.Util
{
    public class SlidingDelayedInvocation
    {
        #region Properties

        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(10);
        private DelayedInvocation PossibleInvocation { get; set; }

        #endregion

        #region Constructor

        public SlidingDelayedInvocation() { }
        public SlidingDelayedInvocation(TimeSpan defaultTimeout)
        {
            DefaultTimeout = defaultTimeout;
        }

        #endregion

        #region Methods

        public void InvokeAfterDelay(Action action)
        {
            InvokeAfterDelay(DefaultTimeout, action);
        }

        private object _lock = new object();
        public void InvokeAfterDelay(TimeSpan delay, Action action)
        {
            lock (_lock)
            {
                if (PossibleInvocation != null)
                {
                    PossibleInvocation.PreventInvocation();
                    PossibleInvocation = null;
                }
                PossibleInvocation = new DelayedInvocation(this, delay, action);
            }
        }

        public void Cancel()
        {
            lock (_lock)
            {
                if (PossibleInvocation != null)
                {
                    PossibleInvocation.PreventInvocation();
                    PossibleInvocation = null;
                }
            }
        }

        #endregion

        #region Internal Classes

        internal class DelayedInvocation
        {
            public Action Action { get; set; }
            public Timer Timer { get; set; }
            public SlidingDelayedInvocation Sender { get; set; }
            private bool IsExecuting;
            private bool IsCancelled;
            private bool IsFired;

            public DelayedInvocation(SlidingDelayedInvocation sender, TimeSpan delay, Action action)
            {
                Sender = sender;
                Action = action;
                Timer = new Timer(delay.TotalMilliseconds);
                Timer.Elapsed += Timer_Elapsed;
                Timer.Start();
            }

            public void PreventInvocation()
            {
                IsCancelled = true;
                if (!IsExecuting)
                {
                    Timer.Stop();
                    Action = null;
                    Sender.PossibleInvocation = null;
                }
            }

            private void Timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                if (IsFired)
                {
                    return;
                }
                IsFired = true;

                IsExecuting = true;
                if (!IsCancelled)
                {
                    Action?.Invoke();
                }
                Timer.Stop();
                Action = null;
                Sender.PossibleInvocation = null;
            }
        }

        #endregion
    }
}


//public class ExpirableEntry<TKey, TValue>
//{
//    public TKey Key { get; set; }
//    public TValue Value { get; set; }
//    public Timer Timer { get; set; }
//    public ConcurrentDictionary<TKey, ExpirableEntry<TKey, TValue>> RefOfValue { get; set; }

//    public ExpirableEntry(TKey key, TValue value, ConcurrentDictionary<TKey, ExpirableEntry<TKey, TValue>> refOfValue, TimeSpan timeout)
//    {
//        Key = key;
//        Value = value;
//        RefOfValue = refOfValue;
//        Timer = new Timer(timeout.TotalMilliseconds);
//        Timer.Elapsed += Timer_Elapsed;
//        Timer.Start();
//    }

//    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
//    {
//        var retry = 0;
//        while (!RefOfValue.TryRemove(Key, out _) && retry < 10)
//        {
//            retry++;
//        }
//    }
//}