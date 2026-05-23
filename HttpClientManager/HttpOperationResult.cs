namespace HttpClientManager;

using System.Net;
using System.Net.Http.Headers;

/// <summary>
/// Represents the normalised result of an HTTP operation executed by <see cref="Interfaces.IRequestProcessor"/>.
/// </summary>
/// <param name="StatusCode">The HTTP status code returned by the server.</param>
/// <param name="Body">The decoded response body. Empty string when the response carried no body.</param>
/// <param name="ResponseHeaders">
/// The response headers returned by the server. Headers are materialised before the underlying
/// <see cref="HttpResponseMessage"/> is disposed and are safe to read after the call returns.
/// </param>
public sealed record HttpOperationResult(
    HttpStatusCode StatusCode,
    string Body,
    HttpResponseHeaders ResponseHeaders);
