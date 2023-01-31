namespace HttpClientManager.Interfaces
{
    public interface IRequestManager
    {
        HttpRequestMessage GetRequest(string url);

        HttpRequestMessage PostRequest(string url, string content);

        HttpRequestMessage PutRequest(string url, string content);
    }
}
