using Azure;
using Azure.Data.Tables;

namespace Model.Entities;

public class SongStatistic:ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public int Views { get; set; }
    public SongStatistic(string category, string songRowKey, int views = 0)
    {
        this.Views = views;
        PartitionKey = category;
        RowKey = songRowKey;
        Timestamp = DateTimeOffset.Now;
        ETag = new ETag(category);
    }

    public static SongStatistic FromTableEntity(TableEntity tableEntity)
    {
        return new(tableEntity.PartitionKey, tableEntity.RowKey, tableEntity.GetInt32("Views").GetValueOrDefault(0));
    }

}