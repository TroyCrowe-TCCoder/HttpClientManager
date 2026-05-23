namespace HttpClientManager.Tests;

using System.Linq;
using System.Text;

public class RequestManagerTests
{
    [Fact]
    public void WhenRelativeUrlIsValidGetRequestReturnsGetRequest()
    {
        var sut = new RequestManager();

        var request = sut.GetRequest("v1/products/42");

        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("v1/products/42", request.RequestUri?.ToString());
        Assert.False(request.RequestUri!.IsAbsoluteUri);
    }

    [Fact]
    public void WhenUrlContainsWhitespaceGetRequestThrowsArgumentException()
    {
        var sut = new RequestManager();

        var action = () => sut.GetRequest(" https://api.example.com");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void WhenUrlIsProtocolRelativeGetRequestThrowsArgumentException()
    {
        var sut = new RequestManager();

        var action = () => sut.GetRequest("//evil.example.com/path");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void WhenUrlIsMalformedRelativeGetRequestThrowsArgumentException()
    {
        var sut = new RequestManager();

        var action = () => sut.GetRequest("http://[::1");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void WhenUrlUsesUnsupportedSchemeGetRequestThrowsArgumentException()
    {
        var sut = new RequestManager();

        var action = () => sut.GetRequest("ftp://files.example.com/report.csv");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public async Task WhenJsonContentIsValidPostRequestCreatesJsonRequestAsync()
    {
        var sut = new RequestManager();

        var request = sut.PostRequest("v1/products", "{\"id\":42}");
        var body = await request.Content!.ReadAsStringAsync();

        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("application/json", request.Content.Headers.ContentType?.MediaType);
        Assert.Equal(Encoding.UTF8.WebName, request.Content.Headers.ContentType?.CharSet);
        Assert.Equal("{\"id\":42}", body);
    }

    [Fact]
    public void WhenMultipartContentIsNullPostRequestThrowsArgumentNullException()
    {
        var sut = new RequestManager();

        var action = () => sut.PostRequest("v1/uploads", (MultipartFormDataContent)null!);

        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void WhenFileUploadRequestIsValidPostWithFileRequestUsesFixedMultipartFieldName()
    {
        var sut = new RequestManager();

        var request = sut.PostWithFileRequest("v1/documents", "{}", new ByteArrayContent([1, 2, 3]), "report.pdf");
        var multipart = Assert.IsType<MultipartFormDataContent>(request.Content);
        var parts = multipart.ToArray();
        var filePart = parts.Single(part => part.Headers.ContentDisposition?.FileName is not null);

        Assert.Equal("file", filePart.Headers.ContentDisposition?.Name?.Trim('"'));
        Assert.Equal("report.pdf", filePart.Headers.ContentDisposition?.FileName?.Trim('"'));
    }

    [Fact]
    public void WhenFileNameContainsControlCharacterPostWithFileRequestThrowsArgumentException()
    {
        var sut = new RequestManager();

        var action = () => sut.PostWithFileRequest("v1/documents", "{}", new ByteArrayContent([]), "bad\u0001name.txt");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public async Task WhenJsonContentIsValidPatchRequestCreatesJsonRequestAsync()
    {
        var sut = new RequestManager();

        var request = sut.PatchRequest("v1/products/42", "{\"name\":\"updated\"}");
        var body = await request.Content!.ReadAsStringAsync();

        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal("application/json", request.Content.Headers.ContentType?.MediaType);
        Assert.Equal(Encoding.UTF8.WebName, request.Content.Headers.ContentType?.CharSet);
        Assert.Equal("{\"name\":\"updated\"}", body);
    }
}
