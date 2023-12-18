// See https://aka.ms/new-console-template for more information
using ActiveMQ.Artemis.Client;
//using Amqp;

Console.WriteLine("Hello, World!");

var connectionFactory = new ConnectionFactory();

var endpoint = Endpoint.Create("localhost", 5672, "guest", "guest");
var connection =  await connectionFactory.CreateAsync(endpoint);

var consumer = await connection.CreateConsumerAsync("a1", RoutingType.Anycast);

var cts = new CancellationTokenSource();


while (!cts.IsCancellationRequested)
{
    var message = await consumer.ReceiveAsync(cts.Token);

    Console.WriteLine(message.ContentType);
    Console.WriteLine($"{message}");
    Console.WriteLine($"Subject {message.Subject}");
    Console.WriteLine($"body {message.GetBody<string>()}");
    await consumer.AcceptAsync(message);
}


await connection.DisposeAsync();
Console.WriteLine("Good bye...");