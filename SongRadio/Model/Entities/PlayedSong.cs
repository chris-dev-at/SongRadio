using Azure;
using Azure.Data.Tables;

namespace Model.Entities;

public class PlayedSong(string songId):ITableEntity
{
    public string PartitionKey { get; set; } = "playedsong";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset? Timestamp { get; set; } = DateTimeOffset.Now;
    public ETag ETag { get; set; } = new ETag("playedsong");

    public string SongId { get; set; } = songId;
}