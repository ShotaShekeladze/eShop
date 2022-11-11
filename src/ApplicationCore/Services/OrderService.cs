using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Azure.Messaging.ServiceBus;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUriComposer _uriComposer;
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<CatalogItem> _itemRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    public OrderService(IRepository<Basket> basketRepository,
        IRepository<CatalogItem> itemRepository,
        IRepository<Order> orderRepository,
        IUriComposer uriComposer,
        IHttpClientFactory httpClientFactory)
    {
        _orderRepository = orderRepository;
        _uriComposer = uriComposer;
        _basketRepository = basketRepository;
        _itemRepository = itemRepository;
        _httpClientFactory = httpClientFactory;
    }

    public async Task CreateOrderAsync(int basketId, Address shippingAddress)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.GetBySpecAsync(basketSpec);

        Guard.Against.NullBasket(basketId, basket);
        Guard.Against.EmptyBasketOnCheckout(basket.Items);

        var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray());
        var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

        var items = basket.Items.Select(basketItem =>
        {
            var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
            var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
            var orderItem = new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
            return orderItem;
        }).ToList();

        var order = new Order(basket.BuyerId, shippingAddress, items);

        await _orderRepository.AddAsync(order);

        //await SendOrder(order);
        await DeliverOrder(order);
        await SendOrderToQueue(order);
    }

    private async Task SendOrderToQueue(Order order)
    {
        var orderJson = JsonConvert.SerializeObject(order);
        await SendMessageToQueue(orderJson);
    }

    private async Task SendMessageToQueue(string message)
    {
        await using var client = new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBusConnectionString"));
        await using ServiceBusSender sender = client.CreateSender(Environment.GetEnvironmentVariable("ServiceBusQueueName"));

        try
        {
            string messageBody = $"Dummy message";
            var sbMessage = new ServiceBusMessage(message);
            await sender.SendMessageAsync(sbMessage);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
        }
        finally
        {
            // Calling DisposeAsync on client types is required to ensure that network
            // resources and other unmanaged objects are properly cleaned up.
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }

    private async Task DeliverOrder(Order order)
    {
        var functionUrl = $"https://hometask-functions.azurewebsites.net/api/OrderDeliveryProcessorFunction?code={Environment.GetEnvironmentVariable("OrderDeliveryProcessorFunctionKey")}";

        var httpClient = _httpClientFactory.CreateClient();

        var orderJson = JsonConvert.SerializeObject(order);

        var requestContent = new StringContent(orderJson, Encoding.UTF8, "application/json");

        await httpClient.PostAsync(functionUrl, requestContent);
    }

    private async Task SendOrder(Order order)
    {
        var functionUrl = $"https://module05-orderitemsreserver.azurewebsites.net/api/OrderItemsReserverFunction?code={Environment.GetEnvironmentVariable("OrderItemsReserverFunctionKey")}";

        var httpClient = _httpClientFactory.CreateClient();

        var orderJson = JsonConvert.SerializeObject(order);

        var requestContent = new StringContent(orderJson, Encoding.UTF8, "application/json");

        await httpClient.PostAsync(functionUrl, requestContent);
    }
}
