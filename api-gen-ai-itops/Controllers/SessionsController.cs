using api_gen_ai_itops.Interfaces;
using api_gen_ai_itops.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Net.Mime;
using Swashbuckle.AspNetCore.Annotations;

namespace api_gen_ai_itops.Controllers
{
    [ApiController]
    [Route("sessions")]
    public class SessionsController : ControllerBase
    {
        private readonly ILogger<SessionsController> _logger;
        private readonly ICosmosDbService _cosmosDbService;

        public SessionsController(
            ILogger<SessionsController> logger,
            ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }


        [SwaggerOperation(
            Summary = "Get all sessions for userId",
            Description = "Returns all the sessions for a userId."
        )]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Session>>> GetAllSessions([FromQuery] string? userId = null)
        {
            try
            {
                _logger.LogDebug("Retrieving chat sessions" + (!string.IsNullOrEmpty(userId) ? $" for user: {userId}" : ""));
                string? query = null;
                if (!string.IsNullOrEmpty(userId))
                {
                    query = "SELECT * FROM c WHERE c.type = 'session' AND c.userId = @userId";
                }
                var sessions = await _cosmosDbService.GetSessionsAsync(query,userId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chat sessions");
                return StatusCode(500, "Internal server error");
            }
        }


        [SwaggerOperation(
            Summary = "Get session details using sessionId",
            Description = "Returns session details using sessionId."
        )]
        [HttpGet("{sessionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Session>> GetSession(string sessionId)
        {
            try
            {
                _logger.LogDebug("Retrieving session: {SessionId}", sessionId);
                var session = await _cosmosDbService.GetSessionAsync(sessionId);
                return Ok(session);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is Microsoft.Azure.Cosmos.CosmosException)
            {
                _logger.LogWarning(ex, "Session not found: {SessionId}", sessionId);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session: {SessionId}", sessionId);
                return StatusCode(500, "Internal server error");
            }
        }


        [SwaggerOperation(
            Summary = "Get all messages using sessionId",
            Description = "Returns all messages using sessionId"
        )]
        [HttpGet("{sessionId}/messages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Message>>> GetSessionMessages(string sessionId)
        {
            try
            {
                _logger.LogDebug("Retrieving messages for session: {SessionId}", sessionId);

                if (!await _cosmosDbService.SessionExists(sessionId))
                {
                    return NotFound();
                }

                var messages = await _cosmosDbService.GetSessionMessagesAsync(sessionId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for session: {SessionId}", sessionId);
                return StatusCode(500, "Internal server error");
            }
        }

        [SwaggerOperation(
            Summary = "Create a new session.",
            Description = "Creates a new sesion in the system"
        )]
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Session>> CreateSession([FromBody] Session session)
        {
            try
            {
                _logger.LogDebug("Creating new chat session");

                if (string.IsNullOrEmpty(session.SessionId))
                {
                    session.SessionId = Guid.NewGuid().ToString();
                }

                var createdSession = await _cosmosDbService.InsertSessionAsync(session);
                return CreatedAtAction(nameof(GetSession), new { sessionId = createdSession.SessionId }, createdSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat session");
                return StatusCode(500, "Internal server error");
            }
        }

        [SwaggerOperation(
            Summary = "Add a message to a session",
            Description = "Creates a new message for a specific sessionId."
        )]
        [HttpPost("{sessionId}/messages")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Message), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Message>> AddMessage(string sessionId, [FromBody] Message message)
        {
            try
            {
                _logger.LogDebug("Adding message to session: {SessionId}", sessionId);

                if (!await _cosmosDbService.SessionExists(sessionId))
                {
                    return NotFound();
                }

                message.SessionId = sessionId;
                var createdMessage = await _cosmosDbService.InsertMessageAsync(message);
                return CreatedAtAction(nameof(GetSessionMessages), new { sessionId }, createdMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding message to session: {SessionId}", sessionId);
                return StatusCode(500, "Internal server error");
            }
        }

        [SwaggerOperation(
            Summary = "Update a session details using a sessionId",
            Description = "Updates a session using a sessionId"
        )]
        [HttpPut("{sessionId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Session>> UpdateSession(string sessionId, [FromBody] Session session)
        {
            try
            {
                _logger.LogDebug("Updating session: {SessionId}", sessionId);

                if (sessionId != session.SessionId)
                {
                    return BadRequest("Session ID mismatch");
                }

                var updatedSession = await _cosmosDbService.UpdateSessionAsync(session);
                return Ok(updatedSession);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is Microsoft.Azure.Cosmos.CosmosException)
            {
                _logger.LogWarning(ex, "Session not found: {SessionId}", sessionId);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session: {SessionId}", sessionId);
                return StatusCode(500, "Internal server error");
            }
        }

        [SwaggerOperation(
            Summary = "Deletes a session using a sessionId and messages",
            Description = "Deletes a session and all its associated messages using transactional batch operations"
        )]
        [HttpDelete("{sessionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSession(string sessionId)
        {
            try
            {
                _logger.LogDebug("Deleting session and messages: {SessionId}", sessionId);

                if (!await _cosmosDbService.SessionExists(sessionId))
                {
                    return NotFound();
                }

                await _cosmosDbService.DeleteSessionAndMessagesAsync(sessionId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session: {SessionId}", sessionId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
