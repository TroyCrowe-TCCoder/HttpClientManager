namespace HttpClientManager;

using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using HttpClientManager.Interfaces;

/// <summary>Creates configured <see cref="HttpClient"/> instances via <see cref="IHttpClientFactory"/>.</summary>
public class HttpClientBuilder(IHttpClientFactory httpClientFactory, ILogger<HttpClientBuilder> logger) : IHttpClientBuilder
{
    private static readonly Action<ILogger, string, Exception?> LogCreatedBasicClientMessage =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(1, nameof(CreateBasicClient)), "Created basic HttpClient for named client {ClientType}.");
    private static readonly Action<ILogger, string, Exception?> LogCreatedOAuthClientMessage =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(2, nameof(CreateOAuthClient)), "Created OAuth HttpClient for named client {ClientType}.");
    private static readonly Action<ILogger, string, Exception?> LogCreatedOAuthFileClientMessage =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(3, nameof(CreateOAuthClientWithFile)), "Created OAuth file-upload HttpClient for named client {ClientType}.");

    private readonly IHttpClientFactory _clientFactory =
        httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<HttpClientBuilder> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc/>
    public HttpClient CreateBasicClient(string clientType, string basePath)
    {
        // Fail fast on missing named-client or base URI input before any client is created.
        ArgumentException.ThrowIfNullOrWhiteSpace(clientType);
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);

        // Validate and normalise the base URI before assigning it to the client.
        var baseUri = ParseAbsoluteUri(basePath);

        // Create the named client only after all caller input has been validated.
        var client = _clientFactory.CreateClient(clientType);
        client.BaseAddress = baseUri;
        // Default JSON accept header keeps API consumers from having to repeat this on every request.
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        LogCreatedBasicClientMessage(_logger, clientType, null);

        return client;
    }

    /// <inheritdoc/>
    public HttpClient CreateOAuthClient(string clientType, string basePath, string accessToken)
    {
        // Validate all caller-supplied values before constructing headers or creating the client.
        ArgumentException.ThrowIfNullOrWhiteSpace(clientType);
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        EnsureSafeBearerToken(accessToken);

        // OAuth clients must pin the base address to HTTPS so tokens are never sent over plaintext HTTP.
        var baseUri = ParseAndEnsureHttpsUri(basePath);
        AuthenticationHeaderValue authorization;
        try
        {
            // Construct the header up front so malformed tokens fail before the HttpClient exists.
            authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException(
                "accessToken is not a valid Bearer authorization header value.",
                nameof(accessToken), ex);
        }

        // Apply the prevalidated base URI and Bearer header to the named client.
        var client = _clientFactory.CreateClient(clientType);
        client.BaseAddress = baseUri;
        client.DefaultRequestHeaders.Authorization = authorization;
        // OAuth JSON APIs still expect an explicit Accept header.
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        LogCreatedOAuthClientMessage(_logger, clientType, null);

        return client;
    }

    /// <inheritdoc/>
    public HttpClient CreateOAuthClientWithFile(string clientType, string basePath, string accessToken)
    {
        // Validate all caller-supplied values before constructing headers or creating the client.
        ArgumentException.ThrowIfNullOrWhiteSpace(clientType);
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        EnsureSafeBearerToken(accessToken);

        // OAuth file-upload clients still require an HTTPS base URI.
        var baseUri = ParseAndEnsureHttpsUri(basePath);
        AuthenticationHeaderValue authorization;
        try
        {
            // Construct the header up front so malformed tokens fail before the HttpClient exists.
            authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException(
                "accessToken is not a valid Bearer authorization header value.",
                nameof(accessToken), ex);
        }

        // File-upload callers control the request content, so only the auth header is applied here.
        var client = _clientFactory.CreateClient(clientType);
        client.BaseAddress = baseUri;
        client.DefaultRequestHeaders.Authorization = authorization;
        LogCreatedOAuthFileClientMessage(_logger, clientType, null);

        return client;
    }

    // Parses basePath as an absolute http:// or https:// URI and returns it.
    // Throws ArgumentException on malformed input or non-HTTP(S) scheme so callers always receive a
    // consistent exception type and the error message never echoes the raw value (which may contain credentials).
    private static Uri ParseAbsoluteUri(string basePath)
    {
        if (!Uri.TryCreate(basePath, UriKind.Absolute, out var uri))
            throw new ArgumentException(
                "basePath must be a valid absolute URI (e.g. 'https://api.example.com').",
                nameof(basePath));

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"URI scheme '{uri.Scheme}' is not permitted. Only 'http' and 'https' are allowed.",
                nameof(basePath));
        }

        // Normalise trailing slash so relative request URIs combine correctly per RFC 3986 §5.2.2.
        // Without it: new Uri(new Uri("https://host/v1"), "users") → https://host/users (drops /v1).
        if (!uri.AbsolutePath.EndsWith('/'))
            uri = new UriBuilder(uri) { Path = uri.AbsolutePath + "/" }.Uri;

        return uri;
    }

    // Parses basePath as an absolute HTTPS URI and returns it; throws ArgumentException on malformed input
    // or non-HTTPS scheme so Bearer tokens are never transmitted in plaintext.
    private static Uri ParseAndEnsureHttpsUri(string basePath)
    {
        if (!Uri.TryCreate(basePath, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "OAuth clients require an HTTPS base path to prevent credentials from being transmitted in plaintext.",
                nameof(basePath));
        }

        // Normalise trailing slash so relative request URIs combine correctly per RFC 3986 §5.2.2.
        if (!uri.AbsolutePath.EndsWith('/'))
            uri = new UriBuilder(uri) { Path = uri.AbsolutePath + "/" }.Uri;

        return uri;
    }

    // Validates the token string before passing it to AuthenticationHeaderValue so that a malformed
    // token throws ArgumentException rather than FormatException (which would echo the raw token value
    // in the exception message). CR/LF in an access token is a header-injection attempt.
    private static void EnsureSafeBearerToken(string accessToken)
    {
        if (accessToken.IndexOfAny([' ', '\t', '\r', '\n', '\0']) >= 0)
            throw new ArgumentException(
                "accessToken contains characters that are not permitted in a Bearer authorization header.",
                nameof(accessToken));
    }
}
