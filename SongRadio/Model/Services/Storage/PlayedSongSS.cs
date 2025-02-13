using Azure;
using Azure.Data.Tables;
using Model.Entities;

namespace Model.Services.Storage;


public class PlayedSongSS(string connectionString):GenericAzureStorageService(connectionString, "playedsong")
{
    public PlayedSong AddPlayedSong(PlayedSong playedSong)
    {
        tableClient.AddEntity(playedSong);
        return playedSong;
    }
}