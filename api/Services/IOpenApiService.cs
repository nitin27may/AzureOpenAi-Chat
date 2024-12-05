using OpenAI.Chat;

namespace GenAI.Api.Services;

public interface IOpenApiService
{
    Task<string> GetChatCompletion(string prompt);

    Task GetChatStreamCompletion(string prompt, Stream outputStream, ILogger logger);

    Task GetChatStreamCompletionWithHistory(List<Models.ChatMessage> messages, Stream outputStream, string sessionId, ILogger logger);
}