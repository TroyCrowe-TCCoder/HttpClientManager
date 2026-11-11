namespace HttpClientManager;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Extension methods for registering HttpClientManager services into an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all HttpClientManager services to the specified <see cref="IServiceCollection"/>. Options are validated
    /// at start-up, so a missing response size limit stops the host instead of rejecting every response.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configure">Configures <see cref="HttpClientManagerOptions"/>; <see cref="HttpClientManagerOptions.MaxResponseBodyBytes"/> is required.</param>
    /// <returns>The same <see cref="IServiceCollection"/> so calls can be chained.</returns>
    public static IServiceCollection AddHttpClientManager(this IServiceCollection services, Action<HttpClientManagerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<HttpClientManagerOptions>()
            .Configure(configure)
            .Validate(options => options.MaxResponseBodyBytes > 0, "HttpClientManagerOptions.MaxResponseBodyBytes must be above 0.")
            .ValidateOnStart();

        services.AddLogging();
        services.AddHttpClient();
        // Aligns the HttpClient response buffer with the RequestProcessor limit for every client in this collection.
        services.ConfigureHttpClientDefaults(b =>
            b.ConfigureHttpClient((provider, client) =>
                client.MaxResponseContentBufferSize = provider.GetRequiredService<IOptions<HttpClientManagerOptions>>().Value.MaxResponseBodyBytes));

        services.AddScoped<Interfaces.IHttpClientBuilder, HttpClientBuilder>();
        services.AddScoped<Interfaces.IRequestManager, RequestManager>();
        services.AddScoped<Interfaces.IRequestProcessor, RequestProcessor>();

        return services;
    }
}
