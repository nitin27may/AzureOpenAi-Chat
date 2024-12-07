namespace GenAI.Api.Models;

public class Message
{
    // Required by Cosmos DB for unique identity
    public string id { get; set; }

    // Consider making UserId and ConversationId strings to ease usage,
    // but Guid is fine. Just convert them when needed.
    public Guid UserId { get; set; }
    public Guid ConversationId { get; set; }
    public string Role { get; set; }    // "user" or "assistant"
    public string Content { get; set; }
}