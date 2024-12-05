namespace GenAI.Api.Models;
public class ChatMessage
{
    public string Role { get; set; } // "user" or "assistant"
    public string Content { get; set; }
}