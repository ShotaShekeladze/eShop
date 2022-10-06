using System.Threading.Tasks;
using OrderDeliveryProcessor.Models;

namespace OrderDeliveryProcessor.Services;
public interface ICosmosDbService
{
    Task AddOrderAsync(Order order);
}
