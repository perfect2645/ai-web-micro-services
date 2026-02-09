// See https://aka.ms/new-console-template for more information
using RabbitMqClient.Clients;

await new TopicModeConsumer().ConsumeAsync();

Console.ReadLine();
