namespace HttpClientManager;

using Microsoft.Extensions.DependencyInjection;

/// <summary>Provides DI service registration for the HttpClientManager library.</summary>
public static class HostBuilder
{
    // Aligns the default HttpClient response buffer with RequestProcessor.MaxResponseBodyBytes.
    // Applied via ConfigureHttpClientDefaults so all named clients in this collection inherit the limit.
    // This covers chunked-encoding responses where no Content-Length header is present and the
    // per-response Content-Length check in RequestProcessor cannot fire before buffering begins.
    private const long MaxResponseContentBufferSize = 10 * 1024 * 1024; // 10 MB

    /// <summary>
    /// Registers all HttpClientManager services into a new <see cref="IServiceCollection"/>.
    /// </summary>
    /// <returns>A configured <see cref="IServiceCollection"/> with all HttpClientManager services registered.</returns>
    public static IServiceCollection ConfigureServices()
    {
        // Create an isolated service collection so consumers can compose it into their host as needed.
        IServiceCollection services = new ServiceCollection();

        // Register Microsoft.Extensions.Logging abstractions so the library's own services can resolve ILogger<T>.
        services.AddLogging();
        // Register IHttpClientFactory support before the library's own typed abstractions.
        services.AddHttpClient();
        // Keep the default client-side response buffer aligned with the library's 10 MB body limit.
        services.ConfigureHttpClientDefaults(b =>
            b.ConfigureHttpClient(c => c.MaxResponseContentBufferSize = MaxResponseContentBufferSize));

        // Register the three focused layers behind their public interfaces.
        services.AddScoped<Interfaces.IHttpClientBuilder, HttpClientBuilder>();
        services.AddScoped<Interfaces.IRequestManager, RequestManager>();
        services.AddScoped<Interfaces.IRequestProcessor, RequestProcessor>();

        return services;
    }
}