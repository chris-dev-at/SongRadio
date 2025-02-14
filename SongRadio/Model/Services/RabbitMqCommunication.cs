using System.Text;
using Model.Entities;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Model.Services;

public class RabbitMqCommunication
{
    private IModel model;
    public RabbitMqCommunication(string hostname)
    {
        var factory = new ConnectionFactory { HostName = hostname };
        var connection = factory.CreateConnection();
        this.model = connection.CreateModel();
    }

    public void PlaySongQuery(Song song)
    {
        var songJson = JsonConvert.SerializeObject(song);
        model.BasicPublish(exchange: "", routingKey: "aggregator.song", body: Encoding.UTF8.GetBytes(songJson));;
    }
    
}