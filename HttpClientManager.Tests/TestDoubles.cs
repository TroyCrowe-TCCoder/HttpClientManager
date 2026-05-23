namespace HttpClientManager.Tests;

using System.Net;
using System.Net.Http.Headers;
using System.Text;

internal sealed class CountingHttpClientFactory(Func<HttpClient> clientFactory) : IHttpClientFactory
{
    private readonly Func<HttpClient> _clientFactory = clientFactory;

    public int CreateClientCallCount { get; private set; }

    public string? LastClientName { get; private set; }

    public HttpClient CreateClient(string name)
    {
        CreateClientCallCount++;
        LastClientName = name;
        return _clientFactory();
    }
}

internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler) : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _handler = handler;

    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(_handler(request, cancellationToken));
    }
}

internal sealed class ThrowIfReadContent : HttpContent
{
    private readonly long _contentLength;

    public ThrowIfReadContent(long contentLength)
    {
        _contentLength = contentLength;
        Headers.ContentLength = contentLength;
        Headers.ContentType = new MediaTypeHeaderValue("text/plain") { CharSet = Encoding.UTF8.WebName };
    }

    public bool ReadAttempted { get; private set; }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        ReadAttempted = true;
        throw new InvalidOperationException("Content should not be read when Content-Length exceeds the processor limit.");
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _contentLength;
        return true;
    }

    protected override Task<Stream> CreateContentReadStreamAsync()
    {
        ReadAttempted = true;
        throw new InvalidOperationException("Content should not be read when Content-Length exceeds the processor limit.");
    }
}

internal sealed class BufferedTestContent : HttpContent
{
    private readonly byte[] _bytes;
    private readonly bool _declareLength;

    public BufferedTestContent(byte[] bytes, string mediaType = "text/plain", string? charSet = null, bool declareLength = false)
    {
        _bytes = bytes;
        _declareLength = declareLength;
        Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        if (charSet is not null)
            Headers.ContentType.CharSet = charSet;

        if (declareLength)
            Headers.ContentLength = _bytes.Length;
    }

    public BufferedTestContent(string value, Encoding encoding, string mediaType = "text/plain", string? charSet = null, bool declareLength = false)
        : this(encoding.GetBytes(value), mediaType, charSet ?? encoding.WebName, declareLength)
    {
    }

    public bool ReadAttempted { get; private set; }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        ReadAttempted = true;
        return stream.WriteAsync(_bytes, 0, _bytes.Length);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _bytes.Length;
        return _declareLength;
    }

    protected override Task<Stream> CreateContentReadStreamAsync()
    {
        ReadAttempted = true;
        return Task.FromResult<Stream>(new MemoryStream(_bytes, writable: false));
    }
}
