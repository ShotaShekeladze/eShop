using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using OrderDeliveryProcessor.Models;

namespace OrderDeliveryProcessor.Services;
public class CosmosDbService : ICosmosDbService
{
    private Container _container;

    public CosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
    {
        this._container = dbClient.GetContainer(databaseName, containerName);
    }

    public async Task AddOrderAsync(Order order)
    {
        await this._container.CreateItemAsync<Order>(order, new PartitionKey(order.Id));
    }
}
