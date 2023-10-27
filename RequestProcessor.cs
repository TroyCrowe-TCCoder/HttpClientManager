namespace HttpClientManager
{
    using HttpClientManager.Interfaces;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class RequestProcessor : IRequestProcessor
    {
        public async Task<Tuple<HttpStatusCode, string>> Delete(HttpClient client, HttpRequestMessage request)
        {
            var response = await client.DeleteAsync(request.RequestUri);

            return Tuple.Create(response.StatusCode, response.Content.ReadAsStringAsync().Result);
        }

        public async Task<Tuple<HttpStatusCode, string>> Get(HttpClient client, HttpRequestMessage request)
        {
            var response = await client.GetAsync(request.RequestUri);

            return Tuple.Create(response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        public async Task<Tuple<HttpStatusCode, string>> Post(HttpClient client, HttpRequestMessage request)
        {
            var response = await client.SendAsync(request);
            return  Tuple.Create(response.StatusCode, response.Content.ReadAsStringAsync().Result);
        }

        public async Task<Tuple<HttpStatusCode, string>> Put(HttpClient client, HttpRequestMessage request)
        {
            var response = await client.PutAsync(request.RequestUri, request.Content);

            return Tuple.Create(response.StatusCode, response.Content.ReadAsStringAsync().Result);
        }
    }
}
