namespace HttpClientManager;

/// <summary>
/// Settings for HttpClientManager. The consuming application supplies every value through
/// <see cref="ServiceCollectionExtensions.AddHttpClientManager"/>; the library defines no defaults.
/// </summary>
public sealed class HttpClientManagerOptions
{
    /// <summary>
    /// Maximum response body size, in bytes, that the body-reading methods buffer into memory. It also sets
    /// <see cref="HttpClient.MaxResponseContentBufferSize"/> on every client the factory creates. Streaming
    /// methods are not bound by it.
    /// </summary>
    public long MaxResponseBodyBytes { get; set; }
}
