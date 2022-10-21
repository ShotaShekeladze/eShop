using Azure.Messaging.ServiceBus;

string sbConnectionString = "???";
string sbQueueName = "incomming_orders";

await using var client = new ServiceBusClient(sbConnectionString);
await using ServiceBusSender sender = client.CreateSender(sbQueueName);

try
{
    string messageBody = $"Dummy message";
    var message = new ServiceBusMessage(messageBody);
    Console.WriteLine($"Sending message: {messageBody}");
    await sender.SendMessageAsync(message);
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

Console.WriteLine("Done!");
