using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace GenAI.Api.Services;

public class CosmosDbContainerFactory : ICosmosDbContainerFactory
{
    private readonly CosmosClient _cosmosClient;
    private readonly string _databaseName;
    private readonly Dictionary<string, Container> _containers;

    public CosmosDbContainerFactory(IOptions<CosmosDbSettings> cosmosDbSettings)
    {
        var settings = cosmosDbSettings.Value;
        _cosmosClient = new CosmosClient(settings.ConnectionString);
        _databaseName = settings.DatabaseName;

        _containers = new Dictionary<string, Container>();
        foreach (var container in settings.Containers)
        {
            _containers[container.Name] = _cosmosClient.GetContainer(_databaseName, container.Name);
        }
    }

    public Container GetContainer(string containerName)
    {
        if (_containers.TryGetValue(containerName, out var container))
        {
            return container;
        }

        throw new ArgumentException($"Container with name '{containerName}' is not configured.");
    }
}