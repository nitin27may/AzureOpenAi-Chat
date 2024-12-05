using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace GenAI.Api.Services;

public class OpenApiService : IOpenApiService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly AzureOpenAIClient _azureClient;

    public OpenApiService(IConfiguration configuration, ILogger<OpenApiService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Initialize OpenAIClient using configuration values
        string endpoint = _configuration["OpenAI:Endpoint"];
        string apiKey = _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI endpoint or API key is not configured properly.");
        }

        _azureClient = new(new Uri(endpoint), new AzureKeyCredential(apiKey));

    }

    public async Task<string> GetChatCompletion(string prompt)
    {
        try
        {
            ChatClient chatClient = _azureClient.GetChatClient("gpt-4o");

            // Create the ChatCompletionsOptions with messages
            ChatCompletion completion = chatClient.CompleteChat(
            [
                    new SystemChatMessage("You are a helpful assistant that talks like a pirate."),
                         // User messages represent user input, whether historical or the most recent input
                        new UserChatMessage(prompt),
            ]
            );

            return completion.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting a response from OpenAI.");
            throw;
        }
    }

    public async Task GetChatStreamCompletion(string prompt, Stream outputStream, ILogger logger)
    {
        try
        {
            ChatClient chatClient = _azureClient.GetChatClient("gpt-4o");

            // Call the OpenAI API
            AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreamingAsync(
                new[]
                {
                new UserChatMessage(prompt),
                });

            await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
            {
                foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
                {
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(contentPart.Text);
                    await outputStream.WriteAsync(data, 0, data.Length);
                    await outputStream.FlushAsync(); // Ensure the chunk is sent to the client
                }
            }

            logger.LogInformation("Streaming completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while streaming the chat response.");
            throw;
        }
    }
}