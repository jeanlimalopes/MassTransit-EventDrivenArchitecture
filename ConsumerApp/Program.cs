using MassTransit;
using Microsoft.Extensions.Hosting;
using Shared;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // MassTransit setup
                services.AddMassTransit(x =>
                {
                    // Register the consumer
                    x.AddConsumer<OrderCreatedConsumer>(config =>
                    {
                        // This sets up the error queue for the consumer
                        config.UseMessageRetry(r => r.Interval(2, TimeSpan.FromSeconds(1))); // Retry policy
                    });

                    // Configure RabbitMQ
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host("localhost", "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        // Register the consumer's endpoint
                        cfg.ReceiveEndpoint("order-created-event", e =>
                        {
                            // Ensure the context parameter is passed to correctly configure the consumer
                            e.ConfigureConsumer<OrderCreatedConsumer>(context);

                            e.UseMessageRetry(r => r.Immediate(5)); // Retry 5 times immediately
                        });
                    });
                });

                // Add MassTransit Hosted Service to automatically manage bus lifecycle
                services.AddMassTransitHostedService(true);
            });
}

// Define your consumer class
public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    public Task Consume(ConsumeContext<OrderCreated> context)
    {
        var message = context.Message;

        // Intentional error creation so that the Exchange and Error Queue are created automatically
        if (message.OrderId.ToString().StartsWith("5"))
        {
            throw new Exception("Order ID starts with 5");
        }
        Console.WriteLine($"Order Received: {message.OrderId} at {message.CreatedAt}");
        return Task.CompletedTask;
    }
}
