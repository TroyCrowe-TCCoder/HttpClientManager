namespace HttpClientManager
{
    using System.Net.Http.Headers;
    public class HttpClientBuilder : Interfaces.IhttpClientBuilder
    {
        private readonly IHttpClientFactory _clientFactory;

        public HttpClientBuilder(IHttpClientFactory httpClientFactory)
        {
            _clientFactory = httpClientFactory;
        }

        public HttpClient CreateBasicClient(string clientType, string basePath)
        {
            var client = _clientFactory.CreateClient(clientType);
            client.BaseAddress = new Uri(basePath);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        public HttpClient CreateOAuthClient(string clientType, string basePath, string accessToken)
        {
            var client = _clientFactory.CreateClient(clientType);
            client.BaseAddress = new Uri(basePath);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        public HttpClient CreateOAuthClientWithFile(string clientType, string basePath, string accessToken)
        {
            var client = _clientFactory.CreateClient(clientType);
            client.BaseAddress = new Uri(basePath);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return client;
        }
    }
}
