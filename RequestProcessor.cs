namespace HttpClientManager;

using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using HttpClientManager.Interfaces;

/// <summary>Executes HTTP requests against a provided <see cref="HttpClient"/> and returns normalised results.</summary>
public class RequestProcessor(ILogger<RequestProcessor> logger) : IRequestProcessor
{
    private static readonly Action<ILogger, string, string, Exception?> LogSendingRequestMessage =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(1, "SendingRequest"), "Sending {Method} request to {RequestUri}.");
    private static readonly Action<ILogger, HttpStatusCode, string, string, Exception?> LogReceivedResponseMessage =
        LoggerMessage.Define<HttpStatusCode, string, string>(LogLevel.Debug, new EventId(2, "ReceivedResponse"), "Received {StatusCode} for {Method} request to {RequestUri}.");
    private static readonly Action<ILogger, string, string, Exception?> LogSendingStreamingRequestMessage =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new EventId(3, "SendingStreamingRequest"), "Sending streaming {Method} request to {RequestUri}.");
    private static readonly Action<ILogger, long, long, Exception?> LogRejectedDeclaredResponseMessage =
        LoggerMessage.Define<long, long>(LogLevel.Warning, new EventId(4, "RejectedDeclaredResponse"), "Rejected response with declared Content-Length {ContentLength} bytes because it exceeds the {MaxResponseBodyBytes} byte limit.");
    private static readonly Action<ILogger, long, long, Exception?> LogRejectedChunkedResponseMessage =
        LoggerMessage.Define<long, long>(LogLevel.Warning, new EventId(5, "RejectedChunkedResponse"), "Rejected chunked response body after reading {BytesRead} bytes because it exceeds the {MaxResponseBodyBytes} byte limit.");

    private readonly ILogger<RequestProcessor> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    // Maximum response body size buffered into memory by the body-reading methods.
    // Protects against OOM when a server advertises a large Content-Length or streams a chunked response.
    private const long MaxResponseBodyBytes = 10 * 1024 * 1024; // 10 MB

    /// <inheritdoc/>
    public async Task<HttpOperationResult> DeleteAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default)
    {
        // Fail fast on null execution dependencies before issuing network I/O.
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);

        LogSendingRequest(request);
        // SendAsync preserves all headers on the request message; client.DeleteAsync would silently drop them.
        // ResponseHeadersRead returns before the body is buffered, allowing the size check below to abort early.
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        LogReceivedResponse(request, response);
        EnsureResponseBodyWithinLimit(response);
        // Capture headers before the response is disposed; they are already materialised in memory.
        var headers = response.Headers;
        return new HttpOperationResult(response.StatusCode, await ReadBodyWithLimitAsync(response.Content, ct).ConfigureAwait(false), headers);
    }

    /// <inheritdoc/>
    public async Task<HttpOperationResult> GetAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default)
    {
        // Fail fast on null execution dependencies before issuing network I/O.
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);

        LogSendingRequest(request);
        // SendAsync preserves all headers on the request message; client.GetAsync would silently drop them.
        // ResponseHeadersRead returns before the body is buffered, allowing the size check below to abort early.
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        LogReceivedResponse(request, response);
        EnsureResponseBodyWithinLimit(response);
        // Capture headers before the response is disposed; they are already materialised in memory.
        var headers = response.Headers;
        return new HttpOperationResult(response.StatusCode, await ReadBodyWithLimitAsync(response.Content, ct).ConfigureAwait(false), headers);
    }

    /// <inheritdoc/>
    public Task<HttpResponseMessage> GetFileAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default)
    {
        // Fail fast on null execution dependencies before issuing network I/O.
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);

        LogSendingStreamingRequest(request);
        // Caller owns and must dispose the returned HttpResponseMessage.
        return client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    /// <inheritdoc/>
    public async Task<HttpOperationResult> PostAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default)
    {
        // Fail fast on null execution dependencies before issuing network I/O.
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);

        LogSendingRequest(request);
        // ResponseHeadersRead returns before the body is buffered, allowing the size check below to abort early.
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        LogReceivedResponse(request, response);
        // Reject oversized declared bodies before allocating the response payload in memory.
        EnsureResponseBodyWithinLimit(response);
        // Capture headers before the response is disposed; they are already materialised in memory.
        var headers = response.Headers;
        return new HttpOperationResult(response.StatusCode, await ReadBodyWithLimitAsync(response.Content, ct).ConfigureAwait(false), headers);
    }

    /// <inheritdoc/>
    public Task<HttpResponseMessage> PostFileRequestAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default)
    {
        // Fail fast on null execution dependencies before issuing network I/O.
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);

        LogSendingStreamingRequest(request);
        // Caller owns and must dispose the returned HttpResponseMessage.
        return client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
    }

    /// <inheritdoc/>
    public async Task<HttpOperationResult> PutAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default)
    {
        // Fail fast on null execution dependencies before issuing network I/O.
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);

        LogSendingRequest(request);
        // SendAsync preserves all headers on the request message; client.PutAsync would silently drop them.
        // ResponseHeadersRead returns before the body is buffered, allowing the size check below to abort early.
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        LogReceivedResponse(request, response);
        EnsureResponseBodyWithinLimit(response);
        // Capture headers before the response is disposed; they are already materialised in memory.
        var headers = response.Headers;
        return new HttpOperationResult(response.StatusCode, await ReadBodyWithLimitAsync(response.Content, ct).ConfigureAwait(false), headers);
    }

    /// <inheritdoc/>
    public async Task<HttpOperationResult> PatchAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default)
    {
        // Fail fast on null execution dependencies before issuing network I/O.
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);

        LogSendingRequest(request);
        // SendAsync preserves all headers on the request message and is required for PATCH.
        // ResponseHeadersRead returns before the body is buffered, allowing the size check below to abort early.
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        LogReceivedResponse(request, response);
        EnsureResponseBodyWithinLimit(response);
        // Capture headers before the response is disposed; they are already materialised in memory.
        var headers = response.Headers;
        return new HttpOperationResult(response.StatusCode, await ReadBodyWithLimitAsync(response.Content, ct).ConfigureAwait(false), headers);
    }

    // Throws if the server declares a Content-Length that exceeds MaxResponseBodyBytes.
    // This guards against OOM attacks via large declared payloads. Chunked-encoding responses
    // have no Content-Length; those are bounded by ReadBodyWithLimitAsync below.
    private void EnsureResponseBodyWithinLimit(HttpResponseMessage response)
    {
        var contentLength = response.Content.Headers.ContentLength;
        if (contentLength > MaxResponseBodyBytes)
        {
            LogRejectedDeclaredResponseMessage(_logger, contentLength.Value, MaxResponseBodyBytes, null);
            throw new InvalidOperationException(
                $"Response Content-Length ({contentLength} bytes) exceeds the maximum permitted size of {MaxResponseBodyBytes / (1024 * 1024)} MB. " +
                "Use GetFileAsync or PostFileRequestAsync to stream large responses.");
        }
    }

    // Streams the response body in 8 KB chunks, accumulating into a MemoryStream capped at
    // MaxResponseBodyBytes. This enforces the size limit for chunked-encoding responses where
    // no Content-Length header is present and EnsureResponseBodyWithinLimit cannot fire in advance.
    // Encoding is derived from Content-Type charset, falling back to UTF-8 as ReadAsStringAsync does.
    private async Task<string> ReadBodyWithLimitAsync(HttpContent content, CancellationToken ct)
    {
        // Detect the declared response encoding before consuming the stream.
        var encoding = DetectEncoding(content);
        await using var networkStream = await content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        long totalRead = 0;
        int bytesRead;

        while ((bytesRead = await networkStream.ReadAsync(chunk, ct).ConfigureAwait(false)) > 0)
        {
            // Track the accumulated body size so chunked responses cannot grow without bound.
            totalRead += bytesRead;
            if (totalRead > MaxResponseBodyBytes)
            {
                LogRejectedChunkedResponseMessage(_logger, totalRead, MaxResponseBodyBytes, null);
                throw new InvalidOperationException(
                    $"Response body exceeds the maximum permitted size of {MaxResponseBodyBytes / (1024 * 1024)} MB. " +
                    "Use GetFileAsync or PostFileRequestAsync to stream large responses.");
            }
            // Copy only the populated region of the reusable buffer into the bounded memory stream.
            await buffer.WriteAsync(chunk.AsMemory(0, bytesRead), ct).ConfigureAwait(false);
        }

        // Decode only the bytes that were written; MemoryStream.GetBuffer may expose unused capacity.
        return encoding.GetString(buffer.GetBuffer(), 0, (int)buffer.Length);
    }

    // Reads charset from Content-Type (e.g. "application/json; charset=utf-8") and returns the
    // corresponding Encoding, falling back to UTF-8 for absent or unrecognised charset values.
    private static Encoding DetectEncoding(HttpContent content)
    {
        var charSet = content.Headers.ContentType?.CharSet;
        if (string.IsNullOrEmpty(charSet))
            return Encoding.UTF8;

        // Some servers quote charset values even though Encoding.GetEncoding expects the bare token.
        charSet = charSet.Trim().Trim('"');
        if (charSet.Length == 0)
            return Encoding.UTF8;

        try
        {
            return Encoding.GetEncoding(charSet);
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
        catch (NotSupportedException)
        {
            return Encoding.UTF8;
        }
    }

    private void LogSendingRequest(HttpRequestMessage request)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            LogSendingRequestMessage(_logger, request.Method.Method, request.RequestUri?.ToString() ?? "<null>", null);
    }

    private void LogReceivedResponse(HttpRequestMessage request, HttpResponseMessage response)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            LogReceivedResponseMessage(_logger, response.StatusCode, request.Method.Method, request.RequestUri?.ToString() ?? "<null>", null);
    }

    private void LogSendingStreamingRequest(HttpRequestMessage request)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            LogSendingStreamingRequestMessage(_logger, request.Method.Method, request.RequestUri?.ToString() ?? "<null>", null);
    }
}
