using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderItemsReserverQueueTriggered.Services;

[assembly: FunctionsStartup(typeof(OrderItemsReserverQueueTriggered.Startup))]
namespace OrderItemsReserverQueueTriggered;
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddOptions<AzureStorageConfig>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("AzureStorageConfig").Bind(settings);
            });

        builder.Services.AddScoped<IStorage, BlobStorage>();
    }
}
