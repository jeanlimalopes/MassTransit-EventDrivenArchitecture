using MassTransit;
using Shared;

var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
{
    cfg.Host("rabbitmq://localhost", h =>
    {
        h.Username("guest");
        h.Password("guest");
    });
});

await busControl.StartAsync();
try
{
    Console.WriteLine("Publishing order created events every 2 seconds...");

    while (true) // Infinite loop to keep publishing
    {
        // Create and publish an event (create a new order)
        var orderCreated = new OrderCreated
        {
            OrderId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        await busControl.Publish(orderCreated);
        Console.WriteLine($"Order created event published: {orderCreated.OrderId} at {orderCreated.CreatedAt}");

        // Wait for 2 seconds before publishing the next event
        await Task.Delay(TimeSpan.FromSeconds(2));
    }
}
finally
{
    await busControl.StopAsync();
}
