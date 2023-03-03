using Muffin.SevDesk.Api.Models.Billing;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Muffin.SevDesk.Api
{
    public class SevDeskService : ISevDeskService, IDisposable
    {
        #region Properties

        private const string BASE_URL = "https://my.sevdesk.de/api/v1";
        private readonly HttpClient _httpClient;

        #endregion

        #region Constructor

        public SevDeskService(IServiceProvider serviceProvider)
        {
            _httpClient = new HttpClient();
        }

        #endregion

        #region Helper

        private string _buildUrl(string urlPart, object query)
        {
            var url = string.Join("/", BASE_URL, urlPart);
            if (query != null)
            {
#warning TODO
            }
            return url;
        }

        private HttpRequestMessage _buildHttpRequestMessage(string method, string url)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), url);
#warning TODO options
            request.Headers.Authorization = new AuthenticationHeaderValue("", "");
            return request;
        }

        public async Task<T> GetAsync<T>(string urlPart, object query = null)
        {
            var response = await GetAsync(urlPart, query);
            return JsonConvert.DeserializeObject<T>(response);
        }

        public async Task<string> GetAsync(string urlPart, object query = null)
        {
            var url = _buildUrl(urlPart, query);
            using (var request = _buildHttpRequestMessage("GET", url))
            {
                var response = await _httpClient.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> PostAsync(string urlPart, object query, object body)
        {
            var url = _buildUrl(urlPart, query);
            using (var request = _buildHttpRequestMessage("POST", url))
            {
                var content = JsonConvert.SerializeObject(body);
                request.Content = new StringContent(content);

                var response = await _httpClient.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
        }

        public void Put() { }

        public void Delete() { }

        #endregion

        //        GET(retrieve a resource)
        //POST(create a resource)
        //PUT(update a resource)
        //DELETE(delete a resource)

        #region ISevDeskService

        public async Task<string> GetNextInvoiceNumber(string objectType, string type)
        {
            return await GetAsync(string.Join("/", "SevSequence", "Factory", "getByType"), new
            {
                objectType,
                type
            });
        }

        public async Task<MemoryFile> GetInvoicePdf(int invoiceId)
        {
            return await GetAsync<MemoryFile>(string.Join("/", invoiceId, "getPfd"), new
            {
                download = true,
                preventSendBy = true
            });
        }

        public async Task SendInvoiceViaEmail(int invoiceId, SendMailData sendMailData)
        {
            await PostAsync(string.Join("/", "Invoice", invoiceId, "sendViaEmail"), null, sendMailData);
        }

        public async Task<bool> CheckCustomerNumberAvailability(string customerNumber)
        {
            return await GetAsync<bool>(string.Join(",", "Contact", "Mapper", "checkCustomerNumberAvailability"), new
            {
                customerNumber
            });
        }

        public async Task<string> GetNextCustomerNumber()
        {
            return await GetAsync(string.Join(",", "Contact", "Factory", "getNextCustomerNumber"));
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        #endregion
    }
}