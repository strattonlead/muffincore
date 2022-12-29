using Microsoft.Extensions.DependencyInjection;
using Muffin.Hetzner.Robot.Api.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Muffin.Hetzner.Robot.Api
{
    public class StorageBoxClient
    {
        #region Properties 

        private readonly StorageBoxClientOptions Options;
        private HttpClient HttpClient
        {
            get
            {
                var handler = new HttpClientHandler { Credentials = new NetworkCredential(Options.Username, Options.Password) };
                handler.PreAuthenticate = true;
                var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(5);
                return client;
            }
        }

        #endregion

        #region Constructor

        public StorageBoxClient(StorageBoxClientOptions options)
        {
            Options = options;
        }

        #endregion

        #region Helper

        private async Task<T> HandleWebRequestAsync<T>(string url, string method)
        {
            using (var request = new HttpRequestMessage(new HttpMethod(method), url))
            {
                var responseMessage = await HttpClient.SendAsync(request);
                var resultString = await responseMessage.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(resultString);
            }
        }

        #endregion

        #region Actions

        /// <summary>
        /// https://robot.your-server.de/doc/webservice/de.html#get-storagebox
        /// </summary>
        public async Task<StorageBoxesResult> GetStorageBoxesAsync()
        {
            using (HttpClient)
            {
                try
                {
                    return new StorageBoxesResult()
                    {
                        StorageBoxes = (await HandleWebRequestAsync<StorageBoxWrapper[]>("https://robot-ws.your-server.de/storagebox", "GET"))
                            .Select(x => x.StorageBox)
                            .ToArray()
                    };
                }
                catch (Exception e)
                {
                    return StorageBoxesResult.Fail(e);
                }
            }
        }

        /// <summary>
        /// https://robot.your-server.de/doc/webservice/de.html#post-storagebox-storagebox-id
        /// </summary>
        public async Task<StorageBoxResult> GetStorageBoxAsync(long storageBoxId)
        {
            using (HttpClient)
            {
                try
                {
                    return new StorageBoxResult()
                    {
                        StorageBox = (await HandleWebRequestAsync<StorageBoxWrapper>($"https://robot-ws.your-server.de/storagebox/{storageBoxId}", "GET")).StorageBox
                    };
                }
                catch (Exception e)
                {
                    return StorageBoxResult.Fail(e);
                }
            }
        }

        #endregion
    }

    public abstract class HetznerResult<T>
        where T : HetznerResult<T>
    {
        public bool Success => string.IsNullOrWhiteSpace(Error);
        public string Error { get; set; }

        public static T Fail(Exception e)
        {
            var result = Activator.CreateInstance<T>();
            result.Error = e.Message;
            return result;
        }
    }

    public class StorageBoxResult : HetznerResult<StorageBoxResult>
    {
        public StorageBox StorageBox { get; set; }
    }

    public class StorageBoxesResult : HetznerResult<StorageBoxesResult>
    {
        public StorageBox[] StorageBoxes { get; set; }
    }

    public class StorageBoxClientOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public static class StorageBoxClientExtensions
    {
        public static void AddStorageBoxClient(this IServiceCollection services, string username, string password)
        {
            services.AddSingleton(new StorageBoxClientOptions()
            {
                Username = username,
                Password = password
            });
            services.AddSingleton<StorageBoxClient>();
        }
    }
}
