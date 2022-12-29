using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Muffin.Services
{
    public class FileProxyHandler
    {
        public async Task HandleAsync(string url, int bytesCount, string tempFileName, Func<string, Task> action)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), tempFileName);

            try
            {
                var client = new HttpClient();
                using (var stream = await client.GetStreamAsync(url))
                {
                    var buffer = new byte[bytesCount];
                    await stream.ReadAsync(buffer, 0, bytesCount);

                    File.WriteAllBytes(tempPath, buffer);
                    await action?.Invoke(tempPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error on {nameof(HandleAsync)}", ex);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }
    }

    public static class FileProxyHandlerExtensions
    {
        public static void AddFileProxyHandler(this IServiceCollection services)
        {
            services.AddScoped<FileProxyHandler>();
        }
    }
}
