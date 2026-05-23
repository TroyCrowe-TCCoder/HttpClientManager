namespace HttpClientManager.Tests;

using System.Net.Http.Headers;
using Microsoft.Extensions.Logging.Abstractions;

public class HttpClientBuilderTests
{
    [Fact]
    public void WhenBasePathIsValidCreateBasicClientSetsBaseAddressAndAcceptHeader()
    {
        var factory = new CountingHttpClientFactory(() => new HttpClient());
        var sut = new HttpClientBuilder(factory, NullLogger<HttpClientBuilder>.Instance);

        var client = sut.CreateBasicClient("MyApi", "https://api.example.com/v1");

        Assert.Equal(new Uri("https://api.example.com/v1/"), client.BaseAddress);
        Assert.Contains(client.DefaultRequestHeaders.Accept, h => h.MediaType == "application/json");
        Assert.Equal(1, factory.CreateClientCallCount);
        Assert.Equal("MyApi", factory.LastClientName);
    }

    [Fact]
    public void WhenBasePathIsMalformedCreateBasicClientThrowsArgumentExceptionAndDoesNotCreateClient()
    {
        var factory = new CountingHttpClientFactory(() => new HttpClient());
        var sut = new HttpClientBuilder(factory, NullLogger<HttpClientBuilder>.Instance);

        var action = () => sut.CreateBasicClient("MyApi", "%");

        Assert.Throws<ArgumentException>(action);
        Assert.Equal(0, factory.CreateClientCallCount);
    }

    [Fact]
    public void WhenBasePathUsesUnsupportedSchemeCreateBasicClientThrowsArgumentExceptionAndDoesNotCreateClient()
    {
        var factory = new CountingHttpClientFactory(() => new HttpClient());
        var sut = new HttpClientBuilder(factory, NullLogger<HttpClientBuilder>.Instance);

        var action = () => sut.CreateBasicClient("MyApi", "ftp://files.example.com");

        Assert.Throws<ArgumentException>(action);
        Assert.Equal(0, factory.CreateClientCallCount);
    }

    [Fact]
    public void WhenOAuthBasePathIsNotHttpsCreateOAuthClientThrowsArgumentExceptionAndDoesNotCreateClient()
    {
        var factory = new CountingHttpClientFactory(() => new HttpClient());
        var sut = new HttpClientBuilder(factory, NullLogger<HttpClientBuilder>.Instance);

        var action = () => sut.CreateOAuthClient("MyApi", "http://api.example.com", "token");

        Assert.Throws<ArgumentException>(action);
        Assert.Equal(0, factory.CreateClientCallCount);
    }

    [Fact]
    public void WhenAccessTokenContainsWhitespaceCreateOAuthClientThrowsArgumentExceptionAndDoesNotCreateClient()
    {
        var factory = new CountingHttpClientFactory(() => new HttpClient());
        var sut = new HttpClientBuilder(factory, NullLogger<HttpClientBuilder>.Instance);

        var action = () => sut.CreateOAuthClient("MyApi", "https://api.example.com", "bad token");

        Assert.Throws<ArgumentException>(action);
        Assert.Equal(0, factory.CreateClientCallCount);
    }

    [Fact]
    public void WhenOAuthInputIsValidCreateOAuthClientSetsAuthorizationAndAcceptHeader()
    {
        var factory = new CountingHttpClientFactory(() => new HttpClient());
        var sut = new HttpClientBuilder(factory, NullLogger<HttpClientBuilder>.Instance);

        var client = sut.CreateOAuthClient("MyApi", "https://api.example.com/v1", "placeholder-token");

        Assert.Equal(new Uri("https://api.example.com/v1/"), client.BaseAddress);
        Assert.Equal(new AuthenticationHeaderValue("Bearer", "placeholder-token"), client.DefaultRequestHeaders.Authorization);
        Assert.Contains(client.DefaultRequestHeaders.Accept, h => h.MediaType == "application/json");
    }

    [Fact]
    public void WhenOAuthFileInputIsValidCreateOAuthClientWithFileSetsAuthorizationWithoutAcceptHeader()
    {
        var factory = new CountingHttpClientFactory(() => new HttpClient());
        var sut = new HttpClientBuilder(factory, NullLogger<HttpClientBuilder>.Instance);

        var client = sut.CreateOAuthClientWithFile("MyApi", "https://api.example.com/files", "placeholder-token");

        Assert.Equal(new Uri("https://api.example.com/files/"), client.BaseAddress);
        Assert.Equal(new AuthenticationHeaderValue("Bearer", "placeholder-token"), client.DefaultRequestHeaders.Authorization);
        Assert.Empty(client.DefaultRequestHeaders.Accept);
    }
}
