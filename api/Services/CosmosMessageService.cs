using GenAI.Api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
namespace GenAI.Api.Services;


public class CosmosMessageService : ICosmosMessageService
{
    private readonly Container _container;
    private readonly CosmosDbSettings _settings;

    public CosmosMessageService(IOptionsMonitor<CosmosDbSettings> cosmosDbSettingsMonitor)
    {
        _settings = cosmosDbSettingsMonitor.CurrentValue;
        var client = new CosmosClient(_settings.ConnectionString);
        var database = client.CreateDatabaseIfNotExistsAsync(_settings.DatabaseId).Result;
        _container = database.Database.CreateContainerIfNotExistsAsync(_settings.ContainerId, partitionKeyPath: "/ConversationId", throughput: 400).Result;
    }

    public async Task<Message> AddMessageAsync(Message message)
    {
        // Ensure an id
        if (string.IsNullOrEmpty(message.id))
            message.id = Guid.NewGuid().ToString();

        ItemResponse<Message> response = await _container.CreateItemAsync(message, new PartitionKey(message.ConversationId.ToString()));
        return response.Resource;
    }

    public async Task<List<Message>> GetMessagesByConversationAsync(Guid conversationId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.ConversationId = @conversationId")
            .WithParameter("@conversationId", conversationId.ToString());

        var iterator = _container.GetItemQueryIterator<Message>(query);
        List<Message> results = new List<Message>();

        while (iterator.HasMoreResults)
        {
            FeedResponse<Message> currentResultSet = await iterator.ReadNextAsync();
            results.AddRange(currentResultSet);
        }

        return results.OrderBy(m => m.id).ToList(); // If you need sorting by insertion order, consider a timestamp field
    }

    public async Task<Message> GetMessageAsync(Guid conversationId, string messageId)
    {
        try
        {
            ItemResponse<Message> response = await _container.ReadItemAsync<Message>(
                messageId,
                new PartitionKey(conversationId.ToString())
            );
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<Message>> GetMessagesByUserIdAsync(Guid userId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.UserId = @userId")
            .WithParameter("@userId", userId.ToString());

        var iterator = _container.GetItemQueryIterator<Message>(query);
        List<Message> results = new List<Message>();

        while (iterator.HasMoreResults)
        {
            FeedResponse<Message> response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }
    public async Task<Message> UpdateMessageAsync(Message message)
    {
        // Requires that message.id and message.ConversationId are set correctly
        ItemResponse<Message> response = await _container.ReplaceItemAsync(
            message,
            message.id,
            new PartitionKey(message.ConversationId.ToString())
        );
        return response.Resource;
    }

    public async Task DeleteMessageAsync(Guid conversationId, string messageId)
    {
        await _container.DeleteItemAsync<Message>(messageId, new PartitionKey(conversationId.ToString()));
    }
}