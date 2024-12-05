using OpenAI.Chat;

namespace GenAI.Api.Models;

public class ChatSession
{
    public string SessionId { get; set; }
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}