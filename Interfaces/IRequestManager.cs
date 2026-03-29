namespace HttpClientManager.Interfaces;

/// <summary>Defines a factory for building <see cref="HttpRequestMessage"/> objects.</summary>
public interface IRequestManager
{
    /// <summary>Creates an HTTP GET message for the specified URL.</summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="url"/> is null or whitespace, contains unencoded whitespace,
    /// is not a valid relative or absolute URI, begins with <c>//</c>, or uses a URI scheme other
    /// than <c>http</c> or <c>https</c>.
    /// </exception>
    HttpRequestMessage GetRequest(string url);

    /// <summary>Creates an HTTP POST message with a JSON string body.</summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="url"/> or <paramref name="content"/> is null or whitespace,
    /// or when <paramref name="url"/> is not a valid relative or absolute URI or fails scheme validation.
    /// </exception>
    HttpRequestMessage PostRequest(string url, string content);

    /// <summary>Creates an HTTP POST message with a multipart form data body.</summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="url"/> is null or whitespace or is not a valid relative or
    /// absolute URI or fails scheme validation.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="content"/> is null.
    /// </exception>
    HttpRequestMessage PostRequest(string url, MultipartFormDataContent content);

    /// <summary>Creates an HTTP POST message with a combined file and JSON metadata body.</summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="url"/>, <paramref name="jsonString"/>, or <paramref name="fileName"/>
    /// is null or whitespace, when <paramref name="url"/> is not a valid relative or absolute URI
    /// or fails scheme validation, or when
    /// <paramref name="fileName"/> contains characters invalid in a <c>Content-Disposition</c> header
    /// (ASCII control characters, DEL, <c>"</c>, <c>\</c>, <c>/</c>, or <c>:</c>).
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="byteArray"/> is null.
    /// </exception>
    HttpRequestMessage PostWithFileRequest(string url, string jsonString, ByteArrayContent byteArray, string fileName);

    /// <summary>Creates an HTTP PUT message with a JSON string body.</summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="url"/> or <paramref name="content"/> is null or whitespace,
    /// or when <paramref name="url"/> is not a valid relative or absolute URI or fails scheme validation.
    /// </exception>
    HttpRequestMessage PutRequest(string url, string content);
}
