using GenAI.Api.Models;

public interface ICosmosMessageService
{
    Task<Message> AddMessageAsync(Message message);
    Task<List<Message>> GetMessagesByConversationAsync(Guid conversationId);
    Task<Message> GetMessageAsync(Guid conversationId, string messageId);
    Task<List<Message>> GetMessagesByUserIdAsync(Guid userId);
    Task<Message> UpdateMessageAsync(Message message);
    Task DeleteMessageAsync(Guid conversationId, string messageId);
}