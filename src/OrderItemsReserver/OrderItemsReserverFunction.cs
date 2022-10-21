using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using OrderItemsReserver.Services;
using System.Text;
using System;
using Newtonsoft.Json;
using OrderItemsReserver.Models;

namespace OrderItemsReserver
{
    public class OrderItemsReserverFunction
    {
        private readonly IStorage _blobStorage;

        public OrderItemsReserverFunction(IStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }

        [FunctionName("OrderItemsReserverFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var valueBytes = Convert.FromBase64String(requestBody);
            var json = Encoding.UTF8.GetString(valueBytes);

            Order order = JsonConvert.DeserializeObject<Order>(json);

            var orderJson = JsonConvert.SerializeObject(order);

            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(orderJson)))
            {
                await _blobStorage.Save(ms, $"order_{DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss_fff")}.json");
            }

            return new OkObjectResult(order != null ? $"order BuyerId: {order.BuyerId}" : "NULL ORDER");
        }
    }
}
