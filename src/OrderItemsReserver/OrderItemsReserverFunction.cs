using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OrderItemsReserver.Models;
using OrderItemsReserver.Services;
using System.Text;
using System;

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
            
            Order order = JsonConvert.DeserializeObject<Order>(requestBody);

            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(requestBody)))
            {
                await _blobStorage.Save(ms, $"order_{DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss_fff")}.json");
            }

            return new OkObjectResult(order != null ? $"order BuyerId: {order.BuyerId}" : "NULL ORDER");
        }
    }
}
