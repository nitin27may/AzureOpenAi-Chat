public class CosmosDbContainerSettings
{
    public string Name { get; set; }
    public string PartitionKey { get; set; }
}

public class CosmosDbSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public List<CosmosDbContainerSettings> Containers { get; set; }
}