using api_gen_ai_itops.Interfaces;
using api_gen_ai_itops.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Net.Mime;
using Swashbuckle.AspNetCore.Annotations;
using Helper.AzureOpenAISearchConfiguration;

namespace api_gen_ai_itops.Controllers
{
    [ApiController]
    [Route("sessions")]
    public class SessionsController : ControllerBase
    {
        private readonly ILogger<SessionsController> _logger;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly Configuration _configuration;

        public SessionsController(
            ILogger<SessionsController> logger,
            ICosmosDbService cosmosDbService,
            Configuration configuration)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
            _configuration = configuration;
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

        /// <summary>
        /// Deletes all sessions and messages from the system.
        /// This is a protected operation that requires a valid system wipe key.
        /// </summary>
        /// <param name="systemWipeKey">The security key required to authorize the system wipe operation</param>
        /// <remarks>
        /// This operation will permanently delete all chat sessions and their associated messages.
        /// It requires a valid system wipe key passed in the X-System-Wipe-Key header.
        /// </remarks>
        /// <returns>
        /// - 204 NoContent: If the deletion was successful
        /// - 401 Unauthorized: If the system wipe key is invalid
        /// - 500 InternalServerError: If there's an error during the operation
        /// </returns>
        /// <response code="204">All sessions and messages successfully deleted</response>
        /// <response code="401">Invalid or missing system wipe key</response>
        /// <response code="500">Internal server error occurred during the operation</response>
        [SwaggerOperation(
            Summary = "Delete all sessions and messages",
            Description = "Deletes all sessions and their associated messages from the system. Requires system wipe authorization key."
        )]
        [HttpDelete("system-wipe")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAllSessions([FromHeader(Name = "X-System-Wipe-Key")] string systemWipeKey)
        {
            try
            {
                var configuredKey = _configuration.SystemWipeKey;

                if (string.IsNullOrEmpty(configuredKey))
                {
                    _logger.LogError("System wipe key not configured");
                    return StatusCode(500, "System wipe functionality not properly configured");
                }

                if (!configuredKey.Equals(systemWipeKey, StringComparison.Ordinal))
                {
                    _logger.LogWarning("Unauthorized system wipe attempt with incorrect key");
                    return Unauthorized("Invalid system wipe key");
                }

                _logger.LogWarning("Authorized system wipe initiated - deleting all sessions and messages");

                await _cosmosDbService.DeleteAllSessionsAndMessagesAsync();

                _logger.LogWarning("System wipe completed successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during system wipe operation");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
