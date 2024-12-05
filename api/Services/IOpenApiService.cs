namespace GenAI.Api.Services;

public interface IOpenApiService
{
    Task<string> GetChatCompletion(string prompt);

    Task GetChatStreamCompletion(string prompt, Stream outputStream, ILogger logger);
}