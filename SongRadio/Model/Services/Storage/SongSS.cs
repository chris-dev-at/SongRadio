using Azure;
using Azure.Data.Tables;
using Model.Entities;

namespace Model.Services.Storage;


public class SongSS(string connectionString) : GenericAzureStorageService(connectionString, "songs")
{
    public Song InsertSong(Song song)
    { 
        tableClient.AddEntity(song);
        return song;
    }
    public void EditSong(Song song)
    {
        tableClient.UpsertEntity(song);
    }
    public List<Song> GetSongs()
    {
        List<Song> songs = new();
        Pageable<TableEntity> results = tableClient.Query<TableEntity>();
        
        foreach (TableEntity tableEntity in results)
        {
            songs.Add(Song.FromTableEntity(tableEntity));
        }
        return songs;
    }
    public Song? GetSong(string rowKey)
    {
        Pageable<TableEntity> results = tableClient.Query<TableEntity>(filter: $"PartitionKey eq 'song' and RowKey eq '{rowKey}'");
        if (!results.Any())
            return null;
        var tableEntity = results.First();
        return Song.FromTableEntity(tableEntity);
    }
}