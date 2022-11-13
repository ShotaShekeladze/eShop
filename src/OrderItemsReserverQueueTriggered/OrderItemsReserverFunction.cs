using System.IO;
using System.Text;
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderItemsReserverQueueTriggered.Models;
using OrderItemsReserverQueueTriggered.Services;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace OrderItemsReserverQueueTriggered
{
    public class OrderItemsReserverFunction
    {
        private readonly IStorage _blobStorage;
        
        public OrderItemsReserverFunction(IStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }

        [FunctionName("OrderItemsReserverFunction")]
        public async Task Run([ServiceBusTrigger("orders", Connection = "QueueConnectionString")]string myQueueItem, ILogger log)
        {
            Order order = JsonConvert.DeserializeObject<Order>(myQueueItem);
            var orderJson = JsonConvert.SerializeObject(order);

            try 
            {
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(orderJson)))
                {
                    await _blobStorage.Save(ms, $"order_{DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss_fff")}.json");
                }
            }
            catch
            {
                var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
                var url = config["LogicAppUrl"].ToString();

                using (HttpClient httpClient = new HttpClient())
                {
                    await httpClient.PostAsync(url, new StringContent(orderJson, Encoding.UTF8));
                }
            }
        }
    }
}
