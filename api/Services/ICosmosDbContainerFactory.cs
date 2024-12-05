using Microsoft.Azure.Cosmos;

namespace GenAI.Api.Services;


public interface ICosmosDbContainerFactory
{
    Container GetContainer(string containerName);
}