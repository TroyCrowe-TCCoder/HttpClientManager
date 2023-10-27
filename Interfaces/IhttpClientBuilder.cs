namespace HttpClientManager.Interfaces
{
    public interface IhttpClientBuilder
    {
        HttpClient CreateBasicClient(string clientType, string basePath);
        HttpClient CreateOAuthClient(string clientType, string basePath, string accessToken);
        HttpClient CreateOAuthClientWithFile(string clientType, string basePath, string accessToken);
    }
}
