using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using OrderDeliveryProcessor.Services;

[assembly: FunctionsStartup(typeof(OrderDeliveryProcessor.Startup))]
namespace OrderDeliveryProcessor;
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<ICosmosDbService>(
            InitializeCosmosClientInstanceAsync().GetAwaiter().GetResult()
        );
    }

    private async Task<CosmosDbService> InitializeCosmosClientInstanceAsync()
    {
        string databaseName = Environment.GetEnvironmentVariable("CosmosDb_DatabaseName");
        string containerName = Environment.GetEnvironmentVariable("CosmosDb_ContainerName");
        string account = Environment.GetEnvironmentVariable("CosmosDb_Account");
        string key = Environment.GetEnvironmentVariable("CosmosDb_Key");
        CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
        CosmosDbService cosmosDbService = new CosmosDbService(client, databaseName, containerName);
        DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
        await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

        return cosmosDbService;
    }
}
