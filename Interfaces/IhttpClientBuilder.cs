namespace HttpClientManager.Interfaces;

/// <summary>Defines a factory for creating configured <see cref="HttpClient"/> instances.</summary>
public interface IHttpClientBuilder
{
    /// <summary>Creates an <see cref="HttpClient"/> with <c>Accept: application/json</c> configured.</summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="clientType"/> or <paramref name="basePath"/> is null or whitespace,
    /// or when <paramref name="basePath"/> is not a valid absolute URI.
    /// </exception>
    HttpClient CreateBasicClient(string clientType, string basePath);

    /// <summary>
    /// Creates an <see cref="HttpClient"/> with Bearer token authorization and <c>Accept: application/json</c> configured.
    /// </summary>
    /// <remarks>
    /// <para><paramref name="basePath"/> must use the <c>https://</c> scheme.
    /// Passing an <c>http://</c> base path throws <see cref="ArgumentException"/> to prevent the access token
    /// from being transmitted in plaintext.</para>
    /// <para><strong>Redirect warning:</strong> <see cref="HttpClient"/> follows 301/302 redirects by default.
    /// A redirect from <c>https://</c> to <c>http://</c> will silently transmit the Bearer token in plaintext.
    /// Disable automatic redirects on OAuth clients by configuring <c>AllowAutoRedirect = false</c> on the
    /// <c>HttpClientHandler</c> in your <c>IHttpClientFactory</c> named-client registration.</para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when any parameter is null or whitespace, when <paramref name="basePath"/> is not a valid
    /// absolute <c>https://</c> URI, or when <paramref name="accessToken"/> contains characters not
    /// permitted in a Bearer authorization header (space, tab, CR, LF, or null).
    /// </exception>
    HttpClient CreateOAuthClient(string clientType, string basePath, string accessToken);

    /// <summary>
    /// Creates an <see cref="HttpClient"/> with Bearer token authorization only, suitable for file upload operations.
    /// </summary>
    /// <remarks>
    /// <para><paramref name="basePath"/> must use the <c>https://</c> scheme.
    /// Passing an <c>http://</c> base path throws <see cref="ArgumentException"/> to prevent the access token
    /// from being transmitted in plaintext.</para>
    /// <para><strong>Redirect warning:</strong> <see cref="HttpClient"/> follows 301/302 redirects by default.
    /// A redirect from <c>https://</c> to <c>http://</c> will silently transmit the Bearer token in plaintext.
    /// Disable automatic redirects on OAuth clients by configuring <c>AllowAutoRedirect = false</c> on the
    /// <c>HttpClientHandler</c> in your <c>IHttpClientFactory</c> named-client registration.</para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when any parameter is null or whitespace, when <paramref name="basePath"/> is not a valid
    /// absolute <c>https://</c> URI, or when <paramref name="accessToken"/> contains characters not
    /// permitted in a Bearer authorization header (space, tab, CR, LF, or null).
    /// </exception>
    HttpClient CreateOAuthClientWithFile(string clientType, string basePath, string accessToken);
}
