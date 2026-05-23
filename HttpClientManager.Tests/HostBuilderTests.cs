namespace HttpClientManager.Tests;

using HttpClientManager.Interfaces;
using Interfaces = HttpClientManager.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void WhenAddHttpClientManagerIsCalledResolvesLibraryInterfaces()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddHttpClientManager();
        using var provider = services.BuildServiceProvider();

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
        IServiceCollection services = new ServiceCollection();
        services.AddHttpClientManager();
        using var provider = services.BuildServiceProvider();

        var factory = provider.GetRequiredService<IHttpClientFactory>();

        Assert.NotNull(factory);
    }

    [Fact]
    public void WhenAddHttpClientManagerIsCalledRegistersLoggerFactory()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddHttpClientManager();
        using var provider = services.BuildServiceProvider();

        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

        Assert.NotNull(loggerFactory);
    }
}
