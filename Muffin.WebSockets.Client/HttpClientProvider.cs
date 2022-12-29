using System.Net.Http;

namespace Muffin.WebSockets.Client
{
    public class HttpClientProvider
    {
        #region Properties

        public HttpClient HttpClient { get; private set; }

        #endregion

        #region Constructor

        #endregion

        #region Actions

        public HttpClient GetClient(HttpClientHandler handler)
        {
            if (HttpClient == null)
            {
                HttpClient = new HttpClient(handler);
            }
            return HttpClient;
        }

        #endregion
    }
}
