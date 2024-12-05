using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace GenAI.Api.Services;

public class OpenApiService : IOpenApiService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenApiService> _logger;
    private readonly AzureOpenAIClient _azureClient;
    private readonly IChatSessionService _chatSessionService;

    public OpenApiService(IConfiguration configuration, ILogger<OpenApiService> logger, IChatSessionService chatSessionService)
    {
        _configuration = configuration;
        _logger = logger;
        _chatSessionService = chatSessionService;

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
                  new ChatMessage[]
                  {
                    new SystemChatMessage("You are a helpful assistant that talks like a pirate."),
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
    //GetChatStreamCompletionWithHistory
    public async Task GetChatStreamCompletionWithHistory(List<Models.ChatMessage> messages, Stream outputStream, string sessionId, ILogger logger)
    {
        try
        {
            ChatClient chatClient = _azureClient.GetChatClient("gpt-4o");

            var messagesToProcess = await CheckMessageLength(messages);

            var chatMessagesArray = messagesToProcess.Select<Models.ChatMessage, ChatMessage>(m =>
              {
                  return m.Role switch
                  {
                      "system" => new SystemChatMessage(m.Content),
                      "user" => new UserChatMessage(m.Content),
                      "assistant" => new AssistantChatMessage(m.Content),
                      _ => throw new InvalidOperationException($"Unknown role: {m.Role}")
                  };
              }).ToArray();

            AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreamingAsync(chatMessagesArray);

            var assistantMessage = new Models.ChatMessage { Role = "assistant", Content = string.Empty };

            await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
            {
                foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
                {
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(contentPart.Text);
                    await outputStream.WriteAsync(data, 0, data.Length);
                    await outputStream.FlushAsync();

                    assistantMessage.Content += contentPart.Text;
                }
            }

            _chatSessionService.AddMessage(sessionId, assistantMessage);

            logger.LogInformation("Streaming completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while streaming the chat response.");
            throw;
        }
    }

    private async Task<List<Models.ChatMessage>> CheckMessageLength(List<Models.ChatMessage> messages)
    {
        int tokenCount = EstimateTokenCount(messages);
        if (tokenCount + 1000 > 100000)
        {
            // Summarize older messages
            var historyToSummarize = messages.Take(messages.Count - 5).ToList(); // Keep the last 5 messages

            var prompt = string.Join("\n", historyToSummarize.Select(m => $"{m.Role}: {m.Content}"));
            var summary = await GetChatCompletion($"Summarize the following conversation: {prompt}");
            // Replace summarized messages with a single summary message
            messages = messages.Skip(messages.Count - 5).ToList(); // Keep the last 5 messages
            messages.Insert(0, new Models.ChatMessage
            {
                Role = "assistant",
                Content = summary
            });

            // Update session with summarized messages
            // session.Messages = messages;

            return messages;
        }
        return messages;
    }
    private int EstimateTokenCount(List<Models.ChatMessage> messages)
    {
        // A simple token estimation logic. Replace with actual token calculation if needed.
        return messages.Sum(m => m.Content.Length / 4); // Approx. 4 characters per token
    }

}