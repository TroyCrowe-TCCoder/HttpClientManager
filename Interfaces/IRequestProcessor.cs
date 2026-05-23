namespace HttpClientManager.Interfaces;

/// <summary>Defines methods for executing HTTP requests and returning normalised results.</summary>
public interface IRequestProcessor
{
    /// <summary>Sends an HTTP DELETE request and returns the status code, response body, and response headers.</summary>
    /// <remarks>
    /// Throws <see cref="InvalidOperationException"/> if the response body exceeds 10 MB, whether the
    /// limit is known up front via <c>Content-Length</c> or encountered while streaming a chunked response.
    /// For larger responses use <see cref="GetFileAsync"/> or <see cref="PostFileRequestAsync"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the response body exceeds 10 MB, or when <paramref name="request"/> has already been dispatched by a previous call.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails due to a network error (DNS failure, connection refused, or TLS handshake failure).</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="ct"/> is canceled or <see cref="HttpClient.Timeout"/> elapses.</exception>
    Task<HttpOperationResult> DeleteAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default);

    /// <summary>Sends an HTTP GET request and returns the status code, response body, and response headers.</summary>
    /// <remarks>
    /// Throws <see cref="InvalidOperationException"/> if the response body exceeds 10 MB, whether the
    /// limit is known up front via <c>Content-Length</c> or encountered while streaming a chunked response.
    /// For larger responses use <see cref="GetFileAsync"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the response body exceeds 10 MB, or when <paramref name="request"/> has already been dispatched by a previous call.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails due to a network error (DNS failure, connection refused, or TLS handshake failure).</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="ct"/> is canceled or <see cref="HttpClient.Timeout"/> elapses.</exception>
    Task<HttpOperationResult> GetAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default);

    /// <summary>Sends an HTTP GET request and returns the raw response for stream access.</summary>
    /// <remarks>
    /// The caller is responsible for disposing the returned <see cref="HttpResponseMessage"/>.
    /// Wrap the return value in a <c>using</c> block or <c>await using</c> to ensure the
    /// underlying connection is returned to the pool promptly.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="request"/> has already been dispatched by a previous call.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails due to a network error (DNS failure, connection refused, or TLS handshake failure).</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="ct"/> is canceled or <see cref="HttpClient.Timeout"/> elapses.</exception>
    Task<HttpResponseMessage> GetFileAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default);

    /// <summary>Sends an HTTP PATCH request and returns the status code, response body, and response headers.</summary>
    /// <remarks>
    /// Throws <see cref="InvalidOperationException"/> if the response body exceeds 10 MB, whether the
    /// limit is known up front via <c>Content-Length</c> or encountered while streaming a chunked response.
    /// For larger responses use <see cref="PostFileRequestAsync"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the response body exceeds 10 MB, or when <paramref name="request"/> has already been dispatched by a previous call.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails due to a network error (DNS failure, connection refused, or TLS handshake failure).</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="ct"/> is canceled or <see cref="HttpClient.Timeout"/> elapses.</exception>
    Task<HttpOperationResult> PatchAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default);

    /// <summary>Sends an HTTP POST request and returns the status code, response body, and response headers.</summary>
    /// <remarks>
    /// Throws <see cref="InvalidOperationException"/> if the response body exceeds 10 MB, whether the
    /// limit is known up front via <c>Content-Length</c> or encountered while streaming a chunked response.
    /// For larger responses use <see cref="PostFileRequestAsync"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the response body exceeds 10 MB, or when <paramref name="request"/> has already been dispatched by a previous call.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails due to a network error (DNS failure, connection refused, or TLS handshake failure).</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="ct"/> is canceled or <see cref="HttpClient.Timeout"/> elapses.</exception>
    Task<HttpOperationResult> PostAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default);

    /// <summary>Sends an HTTP POST request and returns the raw response for stream access.</summary>
    /// <remarks>
    /// The caller is responsible for disposing the returned <see cref="HttpResponseMessage"/>.
    /// Wrap the return value in a <c>using</c> block or <c>await using</c> to ensure the
    /// underlying connection is returned to the pool promptly.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="request"/> has already been dispatched by a previous call.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails due to a network error (DNS failure, connection refused, or TLS handshake failure).</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="ct"/> is canceled or <see cref="HttpClient.Timeout"/> elapses.</exception>
    Task<HttpResponseMessage> PostFileRequestAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default);

    /// <summary>Sends an HTTP PUT request and returns the status code, response body, and response headers.</summary>
    /// <remarks>
    /// Throws <see cref="InvalidOperationException"/> if the response body exceeds 10 MB, whether the
    /// limit is known up front via <c>Content-Length</c> or encountered while streaming a chunked response.
    /// For larger responses use <see cref="PostFileRequestAsync"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="client"/> or <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the response body exceeds 10 MB, or when <paramref name="request"/> has already been dispatched by a previous call.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails due to a network error (DNS failure, connection refused, or TLS handshake failure).</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="ct"/> is canceled or <see cref="HttpClient.Timeout"/> elapses.</exception>
    Task<HttpOperationResult> PutAsync(HttpClient client, HttpRequestMessage request, CancellationToken ct = default);
}
