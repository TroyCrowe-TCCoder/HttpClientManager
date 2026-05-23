namespace HttpClientManager;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering HttpClientManager services into an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    // Aligns the default HttpClient response buffer with RequestProcessor.MaxResponseBodyBytes.
    // Applied via ConfigureHttpClientDefaults so all named clients in this collection inherit the limit.
    private const long MaxResponseContentBufferSize = 10 * 1024 * 1024; // 10 MB

    /// <summary>
    /// Adds all HttpClientManager services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same <see cref="IServiceCollection"/> so calls can be chained.</returns>
    public static IServiceCollection AddHttpClientManager(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLogging();
        services.AddHttpClient();
        services.ConfigureHttpClientDefaults(b =>
            b.ConfigureHttpClient(c => c.MaxResponseContentBufferSize = MaxResponseContentBufferSize));

        services.AddScoped<Interfaces.IHttpClientBuilder, HttpClientBuilder>();
        services.AddScoped<Interfaces.IRequestManager, RequestManager>();
        services.AddScoped<Interfaces.IRequestProcessor, RequestProcessor>();

        return services;
    }
}
