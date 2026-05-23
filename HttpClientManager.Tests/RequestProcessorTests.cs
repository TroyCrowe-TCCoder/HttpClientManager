namespace HttpClientManager.Tests;

using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;

public class RequestProcessorTests
{
    private const int MaxResponseBodyBytes = 10 * 1024 * 1024;

    [Fact]
    public async Task WhenGetAsyncIsCalledReturnsStatusCodeBodyAndPreservesHeadersAsync()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("payload", Encoding.UTF8, "text/plain")
            };

            return response;
        });
        using var client = new HttpClient(handler);
        var sut = new RequestProcessor(NullLogger<RequestProcessor>.Instance);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/items");
        request.Headers.Add("X-Test", "value");

        var result = await sut.GetAsync(client, request);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("payload", result.Body);
        Assert.True(handler.LastRequest?.Headers.Contains("X-Test"));
    }

    [Fact]
    public async Task WhenDeleteAsyncIsCalledReturnsStatusCodeAndBodyAsync()
    {
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.Accepted)
        {
            Content = new StringContent("deleted", Encoding.UTF8, "text/plain")
        });
        using var client = new HttpClient(handler);
        var sut = new RequestProcessor(NullLogger<RequestProcessor>.Instance);
        using var request = new HttpRequestMessage(HttpMethod.Delete, "https://api.example.com/items/42");

        var result = await sut.DeleteAsync(client, request);

        Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
        Assert.Equal("deleted", result.Body);
    }

    [Fact]
    public async Task WhenPostAsyncIsCalledReturnsStatusCodeAndBodyAsync()
    {
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("created", Encoding.UTF8, "text/plain")
        });
        using var client = new HttpClient(handler);
        var sut = new RequestProcessor(NullLogger<RequestProcessor>.Instance);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/items")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };

        var result = await sut.PostAsync(client, request);

        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.Equal("created", result.Body);
    }

    [Fact]
    public async Task WhenPutAsyncIsCalledReturnsStatusCodeAndBodyAsync()
    {
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("updated", Encoding.UTF8, "text/plain")
        });
        using var client = new HttpClient(handler);
        var sut = new RequestProcessor(NullLogger<RequestProcessor>.Instance);
        using var request = new HttpRequestMessage(HttpMethod.Put, "https://api.example.com/items/42")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };

        var result = await sut.PutAsync(client, request);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("updated", result.Body);
    }

    [Fact]
    public async Task WhenDeclaredContentLengthExceedsLimitGetAsyncThrowsInvalidOperationExceptionWithoutReadingContentAsync()
    {
        var oversizedContent = new ThrowIfReadContent(MaxResponseBodyBytes + 1L);
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = oversizedContent
        });
        using var client = new HttpClient(handler);
        var sut = new RequestProcessor(NullLogger<RequestProcessor>.Instance);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/items");

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetAsync(client, request));

        Assert.False(oversizedContent.ReadAttempted);
    }

    [Fact]
    public async Task WhenChunkedBodyExceedsLimitGetAsyncThrowsInvalidOperationExceptionAsync()
    {
        var oversizedContent = new BufferedTestContent(new byte[MaxResponseBodyBytes + 1], declareLength: false);
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = oversizedContent
        });
        using var client = new HttpClient(handler);
        var sut = new RequestProcessor(NullLogger<RequestProcessor>.Instance);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/items");

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetAsync(client, request));

        Assert.True(oversizedContent.ReadAttempted);
    }

    [Fact]
    public async Task WhenQuotedCharsetIsPresentGetAsyncDecodesUsingNormalizedEncodingAsync()
    {
        var content = new BufferedTestContent("hello ✓", Encoding.Unicode, charSet: "\"utf-16\"", declareLength: false);
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = content
        });
        using var client = new HttpClient(handler);
        var sut = new RequestProcessor(NullLogger<RequestProcessor>.Instance);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/items");

        var result = await sut.GetAsync(client, request);

        Assert.Equal("hello ✓", result.Body);
    }

    [Fact]
    public async Task WhenGetFileAsyncIsCalledReturnsRawResponseAsync()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("stream", Encoding.UTF8, "text/plain")
        };
        var handler = new StubHttpMessageHandler((_, _) => response);
        using var client = new HttpClient(handler);
        var sut = new RequestProcessor(NullLogger<RequestProcessor>.Instance);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/file");

        var result = await sut.GetFileAsync(client, request);

        Assert.Same(response, result);
    }

    [Fact]
    public async Task WhenPostFileRequestAsyncIsCalledReturnsRawResponseAsync()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("stream", Encoding.UTF8, "text/plain")
        };
        var handler = new StubHttpMessageHandler((_, _) => response);
        using var client = new HttpClient(handler);
        var sut = new RequestProcessor(NullLogger<RequestProcessor>.Instance);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/file")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };

        var result = await sut.PostFileRequestAsync(client, request);

        Assert.Same(response, result);
    }

    [Fact]
    public async Task WhenPatchAsyncIsCalledReturnsStatusCodeBodyAndHeadersAsync()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("patched", Encoding.UTF8, "text/plain")
            };
            response.Headers.Add("X-Custom", "header-value");
            return response;
        });
        using var client = new HttpClient(handler);
        var sut = new RequestProcessor(NullLogger<RequestProcessor>.Instance);
        using var request = new HttpRequestMessage(HttpMethod.Patch, "https://api.example.com/items/42")
        {
            Content = new StringContent("{\"name\":\"updated\"}", Encoding.UTF8, "application/json")
        };

        var result = await sut.PatchAsync(client, request);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("patched", result.Body);
        Assert.True(result.ResponseHeaders.Contains("X-Custom"));
    }

    [Fact]
    public async Task WhenGetAsyncIsCalledResponseHeadersAreReturnedAsync()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("body", Encoding.UTF8, "text/plain")
            };
            response.Headers.Add("ETag", "\"abc123\"");
            return response;
        });
        using var client = new HttpClient(handler);
        var sut = new RequestProcessor(NullLogger<RequestProcessor>.Instance);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/items");

        var result = await sut.GetAsync(client, request);

        Assert.True(result.ResponseHeaders.Contains("ETag"));
    }
}
