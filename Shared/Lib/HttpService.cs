using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Shared.Lib
{
    public class HttpService
    {
        private readonly HttpClient httpClient;

        public HttpService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<TReturn> GetAsync<TReturn>(string url)
        {
            HttpResponseMessage res = await httpClient.GetAsync(url);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<TReturn>();
            }
            else
            {
                string msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine(msg);
                throw new Exception(msg);
            }
        }

        public async Task<TReturn> PostAsync<TReturn, TRequest>(string url, TRequest request)
        {
            HttpResponseMessage res = await httpClient.PostAsJsonAsync<TRequest>(url, request);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<TReturn>();
            }
            else
            {
                string msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine(msg);
                throw new Exception(msg);
            }
        }
    }
}
