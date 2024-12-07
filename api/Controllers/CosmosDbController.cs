using GenAI.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenAI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CosmosDbController : ControllerBase
    {
        private readonly ICosmosMessageService _messageService;

        public CosmosDbController(ICosmosMessageService messageService)
        {
            _messageService = messageService;
        }

        // GET api/messages/{conversationId}
        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetConversationMessages(Guid conversationId)
        {
            var messages = await _messageService.GetMessagesByConversationAsync(conversationId);
            return Ok(messages);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetMessagesByUserId(Guid userId)
        {
            var messages = await _messageService.GetMessagesByUserIdAsync(userId);
            return Ok(messages);
        }

        // POST api/messages
        [HttpPost]
        public async Task<IActionResult> AddMessage([FromBody] Message message)
        {
            if (message == null || message.ConversationId == Guid.Empty || string.IsNullOrWhiteSpace(message.Content))
            {
                return BadRequest("Invalid message payload.");
            }

            // Add message
            var createdMessage = await _messageService.AddMessageAsync(message);
            return CreatedAtAction(nameof(GetConversationMessages), new { conversationId = createdMessage.ConversationId }, createdMessage);
        }
    }
}