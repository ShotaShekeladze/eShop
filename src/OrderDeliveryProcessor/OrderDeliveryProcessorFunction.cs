using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OrderDeliveryProcessor.Models;
using OrderDeliveryProcessor.Services;

namespace OrderDeliveryProcessor
{
    public class OrderDeliveryProcessorFunction
    {
        private readonly ICosmosDbService _cosmosDbService;

        public OrderDeliveryProcessorFunction(ICosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        [FunctionName("OrderDeliveryProcessorFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Order order = JsonConvert.DeserializeObject<Order>(requestBody);

            await _cosmosDbService.AddOrderAsync(order);
            
            return new OkObjectResult(order != null ? $"order Id: {order.Id}" : "NULL ORDER");
        }
    }
}
