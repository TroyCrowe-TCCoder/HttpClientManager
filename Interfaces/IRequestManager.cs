namespace HttpClientManager.Interfaces
{
    public interface IRequestManager
    {
        HttpRequestMessage GetRequest(string url);

        HttpRequestMessage PostRequest(string url, string content);

        HttpRequestMessage PostRequest(string url, MultipartFormDataContent content);

        HttpRequestMessage PostWithFileRequest(string url, string jsonString, ByteArrayContent byteArray, string fileName);

        HttpRequestMessage PutRequest(string url, string content);
    }
}
