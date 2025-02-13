using Azure.Data.Tables;

namespace Model.Services.Storage;

public abstract class GenericAzureStorageService
{
    public readonly TableClient tableClient;
    protected GenericAzureStorageService(string connectionString, string tableName)
    {
        TableServiceClient serviceClient = new TableServiceClient(connectionString);
        tableClient = serviceClient.GetTableClient(tableName);
        tableClient.CreateIfNotExists();
    }
}