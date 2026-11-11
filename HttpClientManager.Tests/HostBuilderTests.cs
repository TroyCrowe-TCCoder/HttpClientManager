namespace HttpClientManager.Tests;

using HttpClientManager.Interfaces;
using Interfaces = HttpClientManager.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ServiceCollectionExtensionsTests
{
    private static ServiceProvider Build(long maxResponseBodyBytes = 2048)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddHttpClientManager(options => options.MaxResponseBodyBytes = maxResponseBodyBytes);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void WhenOptionsAreSuppliedAppliesLimitToOptionsAndHttpClients()
    {
        using var provider = Build(4096);

        var options = provider.GetRequiredService<IOptions<HttpClientManagerOptions>>().Value;
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();

        Assert.Equal(4096, options.MaxResponseBodyBytes);
        Assert.Equal(4096, client.MaxResponseContentBufferSize);
    }

    [Fact]
    public void WhenMaxResponseBodyBytesIsNotSuppliedOptionsValidationFails()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddHttpClientManager(_ => { });
        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<HttpClientManagerOptions>>().Value);
    }

    [Fact]
    public void WhenConfigureIsNullAddHttpClientManagerThrows()
    {
        IServiceCollection services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.AddHttpClientManager(null!));
    }

    [Fact]
    public void WhenAddHttpClientManagerIsCalledResolvesLibraryInterfaces()
    {
        using var provider = Build();

        var clientBuilder = provider.GetRequiredService<Interfaces.IHttpClientBuilder>();
        var requestManager = provider.GetRequiredService<IRequestManager>();
        var requestProcessor = provider.GetRequiredService<IRequestProcessor>();

        Assert.NotNull(clientBuilder);
        Assert.NotNull(requestManager);
        Assert.NotNull(requestProcessor);
    }

    [Fact]
    public void WhenAddHttpClientManagerIsCalledRegistersHttpClientFactory()
    {
        using var provider = Build();

        var factory = provider.GetRequiredService<IHttpClientFactory>();

        Assert.NotNull(factory);
    }

    [Fact]
    public void WhenAddHttpClientManagerIsCalledRegistersLoggerFactory()
    {
        using var provider = Build();

        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

        Assert.NotNull(loggerFactory);
    }
}
