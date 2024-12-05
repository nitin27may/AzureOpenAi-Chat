using System.Collections.Generic;
using GenAI.Api.Models;
using OpenAI.Chat;
namespace GenAI.Api.Services;

public interface IChatSessionService
{
    /// <summary>
    /// Retrieves an existing chat session or creates a new one if it doesn't exist.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <returns>The chat session object.</returns>
    ChatSession GetOrCreateSession(string sessionId);

    /// <summary>
    /// Adds a message to the chat session history.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <param name="message">The message to add to the session.</param>
    void AddMessage(string sessionId, Models.ChatMessage message);

    /// <summary>
    /// Retrieves the relevant chat history for a session, including the current user prompt.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <param name="currentPrompt">The current prompt to be added to the history.</param>
    /// <returns>A list of chat messages relevant to the current context.</returns>
    Task<List<Models.ChatMessage>> GetRelevantHistory(string sessionId, string currentPrompt);
}
