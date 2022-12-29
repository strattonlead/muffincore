using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MuffinServices
{
    public class RecurringWorker
    {
        public Task ScheduleAsync(DateTime firstInvocationUtc, TimeSpan interval, Action someWork, CancellationToken token)
        {
            return Task.Run(() =>
            {

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var ts = firstInvocationUtc - DateTime.UtcNow;
                        Task.Delay(ts).Wait(token);
                        firstInvocationUtc = firstInvocationUtc.Add(interval);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    someWork();
                }

            }, token);
        }
    }

    public static class RecurringWorkerHelper
    {
        public static void AddRecurringWorker(this IServiceCollection services)
        {
            services.AddSingleton<RecurringWorker>();
        }
    }
}
