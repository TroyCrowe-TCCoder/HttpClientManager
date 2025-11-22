namespace HttpClientManager.Interfaces
{
    using System.Net;

    public interface IRequestProcessor
    {
        Task<Tuple<HttpStatusCode, string>> Delete(HttpClient client, HttpRequestMessage request);

        Task<Tuple<HttpStatusCode, string>> Get(HttpClient client, HttpRequestMessage request);

        Task<HttpResponseMessage> GetFile(HttpClient client, HttpRequestMessage request);

        Task<Tuple<HttpStatusCode, string>> Post(HttpClient client, HttpRequestMessage request);

        Task<HttpResponseMessage> PostFileRequest(HttpClient client, HttpRequestMessage request);

        Task<Tuple<HttpStatusCode, string>> Put(HttpClient client, HttpRequestMessage request);
    }
}
