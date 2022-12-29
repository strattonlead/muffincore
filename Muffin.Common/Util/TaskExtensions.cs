using System;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.Common.Util
{
    public static class MoreTaskExtensions
    {
        public static async Task Delay(TimeSpan delay, params CancellationToken[] tokens)
        {
            using (var source = CancellationTokenSource.CreateLinkedTokenSource(tokens))
            {
                await Task.Delay(delay, source.Token);
            }
        }
    }
}
