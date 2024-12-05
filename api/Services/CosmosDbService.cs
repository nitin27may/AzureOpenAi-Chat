using Microsoft.Azure.Cosmos;
namespace GenAI.Api.Services;


public class CosmosDbService : ICosmosDbService
{
    private readonly ICosmosDbContainerFactory _containerFactory;

    public CosmosDbService(ICosmosDbContainerFactory containerFactory)
    {
        _containerFactory = containerFactory;
    }

    public async Task<IEnumerable<T>> GetItemsAsync<T>(string queryString, string containerName)
    {
        var container = _containerFactory.GetContainer(containerName);
        var query = container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
        var results = new List<T>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task<T> GetItemAsync<T>(string id, string partitionKey, string containerName)
    {
        var container = _containerFactory.GetContainer(containerName);
        var response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
        return response.Resource;
    }

    public async Task AddItemAsync<T>(T item, string containerName)
    {
        var container = _containerFactory.GetContainer(containerName);
        await container.CreateItemAsync(item);
    }

    public async Task UpdateItemAsync<T>(string id, T item, string containerName)
    {
        var container = _containerFactory.GetContainer(containerName);
        await container.ReplaceItemAsync(item, id);
    }

    public async Task DeleteItemAsync(string id, string partitionKey, string containerName)
    {
        var container = _containerFactory.GetContainer(containerName);
        await container.DeleteItemAsync<object>(id, new PartitionKey(partitionKey));
    }
}
