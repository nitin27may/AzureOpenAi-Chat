using System.Text.Json;
using GenAI.Api.Models;
using OpenAI.Chat;

namespace GenAI.Api.Services;

public class ChatSessionService : IChatSessionService
{

    private readonly ILogger<ChatSessionService> _logger;
    public ChatSessionService(ILogger<ChatSessionService> logger)
    {
        _logger = logger;
    }

    private readonly Dictionary<string, ChatSession> _sessions = new Dictionary<string, ChatSession>();
    private const int MaxTokens = 100000; // Adjust based on your model's token limit.

    public ChatSession GetOrCreateSession(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            session = new ChatSession { SessionId = sessionId };
            session.Messages.Add(new Models.ChatMessage
            {
                Role = "system",
                Content = "You are a helpful assistant."
            });
            _sessions[sessionId] = session;
        }
        return session;
    }

    public async Task<List<Models.ChatMessage>> GetRelevantHistory(string sessionId, string currentPrompt)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            return new List<Models.ChatMessage>();

        if (!string.IsNullOrWhiteSpace(currentPrompt))
        {
            AddMessage(sessionId, new Models.ChatMessage { Role = "user", Content = currentPrompt });
        }
        // Clone the updated messages for token trimming and return
        var messages = new List<Models.ChatMessage>(session.Messages);

        // Estimate token count
        

        return messages;
    }


    private int EstimateTokenCount(List<Models.ChatMessage> messages)
    {
        // A simple token estimation logic. Replace with actual token calculation if needed.
        return messages.Sum(m => m.Content.Length / 4); // Approx. 4 characters per token
    }

    public void AddMessage(string sessionId, Models.ChatMessage message)
    {
        var session = GetOrCreateSession(sessionId);

        session.Messages.Add(message);

        var serializedMessages = JsonSerializer.Serialize(session.Messages, new JsonSerializerOptions
        {
            WriteIndented = true // Makes the JSON output formatted (optional)
        });
        _logger.LogInformation("All messages.");
        _logger.LogInformation(serializedMessages);


    }
}