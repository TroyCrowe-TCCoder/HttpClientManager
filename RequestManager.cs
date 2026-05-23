namespace HttpClientManager;

using System.Text;
using HttpClientManager.Interfaces;

/// <summary>Builds <see cref="HttpRequestMessage"/> objects for use with <see cref="IRequestProcessor"/>.</summary>
public class RequestManager : IRequestManager
{
    /// <inheritdoc/>
    public HttpRequestMessage GetRequest(string url)
    {
        // Validate caller input before constructing the request message.
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        EnsurePermittedScheme(url);

        return new(HttpMethod.Get, url);
    }

    /// <inheritdoc/>
    public HttpRequestMessage PostRequest(string url, string content)
    {
        // Validate the target URI and JSON payload before allocating request content.
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        EnsurePermittedScheme(url);

        // Use UTF-8 JSON content so downstream APIs receive a predictable payload encoding.
        HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
        return new(HttpMethod.Post, url) { Content = httpContent };
    }

    /// <inheritdoc/>
    public HttpRequestMessage PostRequest(string url, MultipartFormDataContent content)
    {
        // Validate the URI before attaching caller-owned multipart content.
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentNullException.ThrowIfNull(content);
        EnsurePermittedScheme(url);

        return new(HttpMethod.Post, url) { Content = content };
    }

    /// <inheritdoc/>
    public HttpRequestMessage PostWithFileRequest(string url, string jsonString, ByteArrayContent byteArray, string fileName)
    {
        // Validate every caller-supplied input before assembling the multipart payload.
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonString);
        ArgumentNullException.ThrowIfNull(byteArray);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        EnsurePermittedScheme(url);
        EnsureSafeFileName(fileName);

        // Build a fresh multipart container so each request owns its own content graph.
        var content = new MultipartFormDataContent();
        // Use a fixed form-field control name ("file") so user-supplied fileName cannot control
        // the Content-Disposition name parameter — only the filename attribute is caller-supplied.
        content.Add(byteArray, "file", fileName);
        // Add the metadata payload as JSON alongside the file part.
        content.Add(new StringContent(jsonString, Encoding.UTF8, "application/json"));

        return new(HttpMethod.Post, url) { Content = content };
    }

    /// <inheritdoc/>
    public HttpRequestMessage PutRequest(string url, string content)
    {
        // Validate the target URI and JSON payload before allocating request content.
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        EnsurePermittedScheme(url);

        // Use UTF-8 JSON content so downstream APIs receive a predictable payload encoding.
        HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
        return new(HttpMethod.Put, url) { Content = httpContent };
    }

    /// <inheritdoc/>
    public HttpRequestMessage PatchRequest(string url, string content)
    {
        // Validate the target URI and JSON payload before allocating request content.
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        EnsurePermittedScheme(url);

        // Use UTF-8 JSON content so downstream APIs receive a predictable payload encoding.
        HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
        return new(HttpMethod.Patch, url) { Content = httpContent };
    }

    // Blocks non-HTTP(S) absolute URIs (e.g. file://, ftp://) and protocol-relative URLs (e.g. //evil.com)
    // to prevent SSRF to unexpected transports. Relative URIs are allowed, but they must still parse
    // successfully so malformed values fail here as ArgumentException instead of later as UriFormatException.
    private static void EnsurePermittedScheme(string url)
    {
        // Unencoded whitespace is illegal in HTTP URIs (RFC 3986 §2). A whitespace-prefixed absolute URL
        // fails Uri.TryCreate(Absolute) and is misclassified as a relative URI, bypassing the scheme check.
        if (url.IndexOfAny([' ', '\t', '\r', '\n']) >= 0)
            throw new ArgumentException(
                "URL must not contain unencoded whitespace characters.",
                nameof(url));

        // Protocol-relative URLs (//host/path) are parsed as relative by Uri.TryCreate with UriKind.Absolute,
        // bypassing the scheme check below — reject them explicitly.
        if (url.StartsWith("//", StringComparison.Ordinal))
            throw new ArgumentException(
                "Protocol-relative URLs are not permitted. Use an explicit scheme (e.g. 'https://').",
                nameof(url));

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            // Absolute URIs are allowed only for the two expected HTTP transport schemes.
            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"URL scheme '{uri.Scheme}' is not permitted. Only 'http' and 'https' are allowed.",
                    nameof(url));
            }

            return;
        }

        // Relative URIs are allowed, but malformed ones must still fail here with a consistent exception type.
        if (!Uri.TryCreate(url, UriKind.Relative, out _))
            throw new ArgumentException(
                "URL must be a valid relative or absolute URI.",
                nameof(url));
    }

    // Rejects all ASCII control characters (C0: 0x00–0x1F; DEL: 0x7F) which are forbidden in
    // Content-Disposition quoted-string values (RFC 7230 §3.2.6), and additionally rejects
    // characters that corrupt header structure ('"', '\') or enable path traversal ('/') and
    // NTFS Alternate Data Streams / Windows drive separators (':').
    private static void EnsureSafeFileName(string fileName)
    {
        foreach (var c in fileName)
        {
            // Control characters corrupt header values even when they are not one of the common CR/LF cases.
            if (c < 0x20 || c == 0x7F)
                throw new ArgumentException(
                    "File name contains characters that are invalid in a Content-Disposition header.",
                    nameof(fileName));
        }

        // Structural characters are checked separately because they sit outside the control-character range.
        if (fileName.IndexOfAny(['"', '\\', '/', ':']) >= 0)
            throw new ArgumentException(
                "File name contains characters that are invalid in a Content-Disposition header.",
                nameof(fileName));
    }
}
