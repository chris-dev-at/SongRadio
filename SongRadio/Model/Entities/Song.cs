using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace Model.Entities;

public class Song:ITableEntity
{
    public Song(string title, string artist, TimeSpan duration, string genre)
    {
        Title = title;
        Artist = artist;
        Duration = duration;
        Genre = genre;
        RowKey = Guid.NewGuid().ToString();

        PartitionKey = "song";
        Timestamp = DateTimeOffset.Now;
        ETag = new ETag(title);
    }

    public string Title { get; set; }
    public string Artist { get; set; }
    
    public string DurationString
    {
        get => Duration.ToString("c");
        set => Duration = TimeSpan.Parse(value);
    }

    // Don't store this property in the data store, only use the DurationString
    [JsonIgnore, IgnoreDataMember]
    public TimeSpan Duration { get; set; }
    public string Genre { get; set; }
    
    
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public static Song FromTableEntity(TableEntity tableEntity)
    {
        Song song = new(
            tableEntity.GetString("Title"),
            tableEntity.GetString("Artist"),
            TimeSpan.Parse(tableEntity.GetString("DurationString")),
            tableEntity.GetString("Genre")
        );
        song.RowKey = tableEntity.RowKey;
        song.PartitionKey = tableEntity.PartitionKey;
        song.ETag = tableEntity.ETag;
        
        return song;
    }

}