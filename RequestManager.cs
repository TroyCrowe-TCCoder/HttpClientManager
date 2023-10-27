namespace HttpClientManager
{
    using HttpClientManager.Interfaces;
    using System.IO;
    using System.Text;

    public class RequestManager : IRequestManager
    {
        public HttpRequestMessage GetRequest(string url)
        {
            return new HttpRequestMessage(HttpMethod.Get, url);
        }

        public HttpRequestMessage PostRequest(string url, string jsonString)
        {
            HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            return request;
        }

        public HttpRequestMessage PostRequest(string url, MultipartFormDataContent content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

            return request;
        }

        public HttpRequestMessage PostWithFileRequest(string url, string jsonString, ByteArrayContent byteArray, string fileName)
        {
            var content = new MultipartFormDataContent();
            content.Add(byteArray, fileName, fileName);
            content.Add(new StringContent(jsonString, Encoding.UTF8, "application/json"));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

            return request;
        }

        public HttpRequestMessage PutRequest(string url, string jsonString)
        {
            HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url) { Content = content };
            return request;
        }
    }
}
