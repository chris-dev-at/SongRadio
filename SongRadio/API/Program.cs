using Microsoft.AspNetCore.Mvc;
using Model.Entities;
using Model.Services;
using Model.Services.Storage;

namespace API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        var app = builder.Build();
        
        var cloudStorageConn = Environment.GetEnvironmentVariable("azureStorageServiceConnectionString", EnvironmentVariableTarget.Process)  ?? app.Configuration.GetValue<string>("azureStorageServiceConnectionString") ?? "UseDevelopmentStorage=true";
        var rabbitMqHost = app.Configuration.GetValue<string>("rabbitMqHostname") ?? "rabbitmq";

        Console.WriteLine($"Using Azure Storage Connection: {cloudStorageConn}");
        Console.WriteLine($"Using RabbitMQ Host: {rabbitMqHost}");
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        
        var eventDispatcher = new RabbitMqCommunication(rabbitMqHost);
        var songSS = new SongSS(cloudStorageConn);
        var statisticsSS = new StatisticsSS(cloudStorageConn);
        var playbackHistory = new PlayedSongSS(cloudStorageConn);
        
        app.MapPost("/song", ([FromBody] Song song) => songSS.InsertSong(song))
            .WithName("Add Song")
            .WithOpenApi();

        app.MapPut("/song", ([FromBody] Song song) =>
            {
                songSS.EditSong(song);
            })
            .WithName("Edit Song")
            .WithOpenApi();


        app.MapGet("/song", () => songSS.GetSongs())
            .WithName("List all songs")
            .WithOpenApi();

        app.MapPost("/song/{songId}/play", (string songId) =>
        {
            playbackHistory.AddPlayedSong(new PlayedSong(songId));
            var song = songSS.GetSong(songId);
            if (song == null)
                return Results.NotFound();
    
            //Execute RabbitMQ Query (Trigger Aggregator)
            eventDispatcher.PlaySongQuery(song);
            return Results.Ok();
        });

        app.MapPost("/categories/{categoryId}/songs", (String categoryId) =>
            {
                var mostViewedSongsStatisticsForCategory = statisticsSS.GetMostViewedSongsForCategory(categoryId);
                List<object> result = new();
                foreach (var statisticsItem in mostViewedSongsStatisticsForCategory)
                {
                    var song = songSS.GetSong(statisticsItem.RowKey);
                    if(song == null)
                        continue;
                    result.Add(new
                    {
                        Name = song.Title,
                        RowKey = song.RowKey,
                        Views = statisticsItem.Views
                    });
                }
    
    
                return result;
            })
            .WithName("View the most played songs per category")
            .WithOpenApi();
        
        app.MapGet("/", () => "Welcome to the Music Service API!");

        app.Run();
    }
}