using Azure;
using Azure.Data.Tables;
using Model.Entities;

namespace Model.Services.Storage;


public class StatisticsSS(string connectionString):GenericAzureStorageService(connectionString, "statistics")
{
    public SongStatistic GetSongViews(string category, string songRowKey)
    {
        Pageable<TableEntity> results = tableClient.Query<TableEntity>(filter: $"RowKey eq '{songRowKey}'");
        if (!results.Any())
            return new SongStatistic(category, songRowKey);
        return SongStatistic.FromTableEntity(results.First());
    }
    public void AddSongView(Song song)
    {
        var statisticsItem = GetSongViews(song.Genre, song.RowKey);
        statisticsItem.Views++;
        tableClient.UpsertEntity(statisticsItem);
    }

    public List<SongStatistic> GetMostViewedSongsForCategory(string category)
    {
        Pageable<TableEntity> results = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{category}'");
        var topThreeResults = results
            .OrderByDescending(e => e.GetInt32("Views"))
            .Take(3)
            .ToList();

        return topThreeResults.Select(item => SongStatistic.FromTableEntity(item)).ToList();
    }
}