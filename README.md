# MassTransit - Event Driven Architecture

## Project Structure

1. **Producer App**: Publishes messages (events) to RabbitMQ.
2. **Consumer App**: Consumes messages (events) from RabbitMQ.

#### Step 1: Create Two Console Projects

Create a new solution to contain both the producer and consumer projects:

```bash
dotnet new sln -n MassTransitExample
```

Create the producer project:

```bash
dotnet new console -n ProducerApp
```

Create the consumer project:

```bash
dotnet new console -n ConsumerApp
```

Add both projects to the solution:

```bash
dotnet sln add ProducerApp/ProducerApp.csproj
dotnet sln add ConsumerApp/ConsumerApp.csproj
```

#### Step 2: Install Required NuGet Packages
You need to install the MassTransit and RabbitMQ packages in both projects.

**ProducerApp**:

```bash
cd ProducerApp
dotnet add package MassTransit
dotnet add package MassTransit.RabbitMQ
```

**ConsumerApp**:

```bash
cd ../ConsumerApp
dotnet add package MassTransit
dotnet add package MassTransit.RabbitMQ
```

#### Step 3: Define a Shared Event (Message)
You’ll need a shared project or a shared class for the event message. Let’s create a simple class to represent the OrderCreated event.

In the solution root directory, create a new folder called Shared and inside that folder create a Messages.cs file:

```bash
mkdir Shared
cd Shared
touch Messages.cs
```

Add the following code to define the event in Messages.cs:

```csharp
namespace Shared
{
    public class OrderCreated
    {
        public Guid OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

Add a reference to the shared project in both **ProducerApp** and **ConsumerApp**:
Open ProducerApp/ProducerApp.csproj and add the shared project reference:

```xml
<ItemGroup>
  <ProjectReference Include="../Shared/Shared.csproj" />
</ItemGroup>
```

Repeat the same steps for **ConsumerApp**.

#### Step 4: Implement the Producer (Publisher)
Open the ProducerApp/Program.cs and update it to publish the OrderCreated event to RabbitMQ using MassTransit.

```csharp
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
    Console.WriteLine("Publishing an order created event...");

    // Publish an event (create a new order)
    await busControl.Publish(new OrderCreated
    {
        OrderId = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow
    });

    Console.WriteLine("Order created event published.");
}
finally
{
    await busControl.StopAsync();
}
```

This code sets up the RabbitMQ connection and publishes the OrderCreated event to the RabbitMQ broker.

#### Step 5: Implement the Consumer (Listener)
Open the ConsumerApp/Program.cs and update it to consume the OrderCreated event from RabbitMQ using MassTransit.

```csharp
using MassTransit;
using Shared;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    public Task Consume(ConsumeContext<OrderCreated> context)
    {
        var message = context.Message;
        Console.WriteLine($"Order Received: {message.OrderId} at {message.CreatedAt}");
        return Task.CompletedTask;
    }
}

var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
{
    cfg.Host("rabbitmq://localhost", h =>
    {
        h.Username("guest");
        h.Password("guest");
    });

    cfg.ReceiveEndpoint("order-created-event", e =>
    {
        e.Consumer<OrderCreatedConsumer>();
    });
});

await busControl.StartAsync();
try
{
    Console.WriteLine("Listening for order created events...");
    Console.ReadLine();
}
finally
{
    await busControl.StopAsync();
}
```

This code configures a consumer that listens for OrderCreated events and prints the event details when received.

#### Step 6: Running the Applications

Ensure RabbitMQ is running (use Docker or install it locally). If using Docker, you can run:

```bash
docker run -d --hostname rabbitmq --name rabbitmq -p 5672:5672 rabbitmq:3-management
```
Run the **ConsumerApp** first:

```bash
cd ConsumerApp
dotnet run
```
The consumer will start listening for events and wait for OrderCreated messages.

Run the **ProducerApp**:

```bash
cd ../ProducerApp
dotnet run
```

When the producer runs, it will publish an OrderCreated event, which the consumer will receive and process.

You should see the following in the consumer's console output:

```bash
Order Received: f47ac10b-58cc-4372-a567-0e02b2c3d479 at 10/08/2024 14:25:00
```

#### Step 7: Add Resiliency (Optional)
To add retry policies or other resiliency features, you can modify the consumer configuration.

For example, in ConsumerApp/Program.cs, add the retry policy:

```csharp
cfg.ReceiveEndpoint("order-created-event", e =>
{
    e.Consumer<OrderCreatedConsumer>();
    
    // Retry policy
    e.UseMessageRetry(r => r.Immediate(5)); // Retry 5 times immediately
});
```

### Conclusion
By following these steps, you have two console applications communicating via MassTransit and RabbitMQ in a producer-consumer model. The producer sends messages (events) to RabbitMQ, and the consumer listens for and processes those messages. This setup demonstrates an event-driven architecture using two separate, decoupled applications. You can extend this system by adding more consumers, improving error handling, and scaling the system as needed.#   M a s s T r a n s i t - E v e n t D r i v e n A r c h i t e c t u r e 
 
 
