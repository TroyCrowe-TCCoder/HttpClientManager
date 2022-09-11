namespace HttpClientManager
{
    using Microsoft.Extensions.DependencyInjection;

    public static class HostBuilder
    {
        public static IServiceCollection ConfigureSerices()
        {
            IServiceCollection services = new ServiceCollection()
                .AddHttpClient()
                .AddScoped(typeof(Interfaces.IRequestManager), typeof(RequestManager))
                .AddScoped(typeof(Interfaces.IRequestProcessor), typeof(RequestProcessor));

            HttpClientFactoryServiceCollectionExtensions.AddHttpClient(services);

            services.BuildServiceProvider();

            return services;
        }
    }
}