using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.Common.Util
{
    public interface IInvocationLimiter
    {
        public string UniqueIdentifier { get; set; }
        public decimal? InvocationsPerMinute { get; set; }
        public decimal? InvocationsPerHour { get; set; }
        DateTime? LastInvocation { get; }
        Task InvokeAsync(Action action, CancellationToken cancellationToken);
    }

    public class InvocationLimiter : IInvocationLimiter
    {
        public string UniqueIdentifier { get; set; }
        public decimal? InvocationsPerMinute { get; set; }
        public decimal? InvocationsPerHour { get; set; }
        public DateTime? LastInvocation { get; private set; }

        public InvocationLimiter()
        {
            UniqueIdentifier = Guid.NewGuid().ToString();
        }

        private TimeSpan _getDelay()
        {
            if (!LastInvocation.HasValue || (!InvocationsPerMinute.HasValue && !InvocationsPerHour.HasValue))
            {
                return TimeSpan.Zero;
            }

            TimeSpan delay = TimeSpan.Zero;
            if (InvocationsPerHour.HasValue && InvocationsPerMinute.HasValue)
            {
                var hourDelay = TimeSpan.FromHours((double)(1m / InvocationsPerHour.Value));
                var minuteDelay = TimeSpan.FromMinutes((double)(1m / InvocationsPerMinute.Value));
                if (LastInvocation.Value.Add(hourDelay) >= LastInvocation.Value.Add(minuteDelay))
                {
                    delay = hourDelay;
                }
                else
                {
                    delay = minuteDelay;
                }
            }
            else if (InvocationsPerHour.HasValue)
            {
                delay = TimeSpan.FromHours((double)(1m / InvocationsPerHour.Value));
            }
            else if (InvocationsPerMinute.HasValue)
            {
                delay = TimeSpan.FromMinutes((double)(1m / InvocationsPerMinute.Value));
            }

            if (LastInvocation.Value.Add(delay) < DateTime.UtcNow)
            {
                return TimeSpan.Zero;
            }

            var gap = DateTime.UtcNow - LastInvocation.Value;
            if (gap < delay)
            {
                return delay - gap;
            }

            return delay;
        }

        public async Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
        {
            var delay = _getDelay();
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            LastInvocation = DateTime.UtcNow;
            action?.Invoke();
        }
    }
}
