using System.Text;
using Model.Entities;
using Model.Services.Storage;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Aggregator;

class Program
{
    static void Main(string[] args)
    {
        // Set up a way to keep the application running
        ManualResetEvent _quitEvent = new ManualResetEvent(false);
        Console.CancelKeyPress += (sender, eArgs) =>
        {
            _quitEvent.Set();
            eArgs.Cancel = true;
        };
        
        string MessageFromString(BasicDeliverEventArgs ea) => Encoding.UTF8.GetString(ea.Body.ToArray());
        
        
        var azureStorageAccountConnectionString = 
            Environment.GetEnvironmentVariable("rabbitMqHostname", EnvironmentVariableTarget.Process) ?? "localhost";
        var rabbitMqHost = 
            Environment.GetEnvironmentVariable("azureStorageServiceConnectionString", EnvironmentVariableTarget.Process) ?? "UseDevelopmentStorage=true;";
        
        
        var statisticsSS = new StatisticsSS(azureStorageAccountConnectionString);
        var factory = new ConnectionFactory { HostName = rabbitMqHost };
        
        var connection = factory.CreateConnection();
        var model = connection.CreateModel();
        
        
        // Set up the queue
        model.QueueDeclare(queue: "aggregator.song", 
            durable: true, 
            exclusive: false, 
            autoDelete: false, 
            arguments: null);
        Console.WriteLine("Setup RabbitMQ queue");
        
        
        // Set up Listening
        var consumer = new EventingBasicConsumer(model);

        consumer.Received += (m, ea) =>
        {
            try
            {
                var song = JsonConvert.DeserializeObject<Song>(MessageFromString(ea));
                Console.WriteLine($"Received song: {song?.Title} by {song?.Artist}, Category: {song?.Genre}");
                statisticsSS.AddSongView(song);
                model.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                model.BasicNack(ea.DeliveryTag, false, true);  // Requeue the message (maybe dead letter it)
            }
        };
        model.BasicConsume("aggregator.song", autoAck: false, consumer: consumer); // Don't auto-acknowledge messages (important for error handling)
        
        
        
        //keep the application running
        _quitEvent.WaitOne();
    }
}