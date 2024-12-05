using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICosmosDbService
{
    Task<IEnumerable<T>> GetItemsAsync<T>(string queryString, string containerName);
    Task<T> GetItemAsync<T>(string id, string partitionKey, string containerName);
    Task AddItemAsync<T>(T item, string containerName);
    Task UpdateItemAsync<T>(string id, T item, string containerName);
    Task DeleteItemAsync(string id, string partitionKey, string containerName);
}