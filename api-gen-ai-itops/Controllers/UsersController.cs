using api_gen_ai_itops.Interfaces;
using api_gen_ai_itops.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Swashbuckle.AspNetCore.Annotations;

namespace api_gen_ai_itops.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly ICosmosDbService _cosmosDbService;

        public UsersController(
            ILogger<UsersController> logger,
            ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }

        [SwaggerOperation(
            Summary = "Gets all users",
            Description = "Returns a list of all User objects in the system."
        )]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            try
            {
                _logger.LogDebug("Retrieving all users");
                var users = await _cosmosDbService.GetUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, "Internal server error");
            }
        }

        [SwaggerOperation(
            Summary = "Gets a specific user",
            Description = "Returns a a specific User objects in the system."
        )]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            try
            {
                _logger.LogDebug("Retrieving user with id: {Id}", id);
                var user = await _cosmosDbService.GetUserAsync(id);
                if (user is null)
                {
                    _logger.LogWarning("User not found with id: {Id}", id);
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [SwaggerOperation(
            Summary = "Creates a user",
            Description = "Creates a user in the system."
        )]
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            _logger.LogDebug("Create User action");
            try
            {
                if (!UserRoles.ValidRoles.Contains(user.Role))
                {
                    return BadRequest($"Invalid role. Must be one of: {string.Join(", ", UserRoles.ValidRoles)}");
                }

                user.Id = user.UserInfo.Email;

                // Simple checks and set to defaults as needed before creating the user
                if (user.Tier != "pro" && user.Tier != "trial")
                {
                    user.Tier = "none";
                }
                if (user.Preferences.Theme != "light" || user.Preferences.Theme != "dark")
                {
                    user.Preferences.Theme = "light";
                }

                _logger.LogDebug("Creating new user with id: {Id}", user.Id);
                var created = await _cosmosDbService.CreateUserAsync(user);
                if (!created)
                {
                    return Conflict($"A user with ID {user.Id} already exists");
                }

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid user data provided");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {@User}", user);
                return StatusCode(500, "Internal server error");
            }
        }

        [SwaggerOperation(
            Summary = "Update specific user details",
            Description = "Update the properties of a specific user in the system."
        )]
        [HttpPut("{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> UpdateUser(string id, [FromBody] User user)
        {
            try
            {

                if (user.UserInfo?.Email == null)
                {
                    return BadRequest("UserInfo.Email cannot be null");
                }
                _logger.LogDebug("Updating user with email: {Email}", user.UserInfo.Email);

                user.Id = id;

                var updated = await _cosmosDbService.UpdateUserAsync(id, user);
                if (!updated)
                {
                    _logger.LogWarning("User not found with id: {Id}", id);
                    return NotFound();
                }

                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid user data provided");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [SwaggerOperation(
            Summary = "Deletes a user.",
            Description = "Deletes a specific user in the system"
        )]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                _logger.LogDebug("Deleting user with id: {Id}", id);

                var deleted = await _cosmosDbService.DeleteUserAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("User not found with id: {Id}", id);
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [SwaggerOperation(
            Summary = "Purges all user data",
            Description = "Deletes all chat sessions and messages associated with a specific user"
        )]
        [HttpDelete("{id}/history")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PurgeUserHistory(string id)
        {
            try
            {
                _logger.LogDebug("Purging chat history for user with id: {Id}", id);

                // First verify the user exists
                var user = await _cosmosDbService.GetUserAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User not found with id: {Id}", id);
                    return NotFound();
                }

                // Get all sessions for this user
                var userSessions = await _cosmosDbService.GetSessionsAsync(
                    "SELECT * FROM c WHERE c.type = @type AND c.userId = @userId",
                    id);

                // Delete each session and its messages
                foreach (var session in userSessions)
                {
                    await _cosmosDbService.DeleteSessionAndMessagesAsync(session.SessionId);
                }

                _logger.LogInformation("Successfully purged chat history for user: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purging chat history for user with id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [SwaggerOperation(
            Summary = "Gets the current authenticated user",
            Description = "Returns information about the currently authenticated user from Easy Auth."
        )]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<UserInfo> GetCurrentUser()
        {
            try
            {
                // Get user info from Easy Auth headers
                var userEmail = Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"].ToString();
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("No user email found in headers");
                    return Unauthorized("User not authenticated");
                }

                var userInfo = new UserInfo
                {
                    Email = userEmail,
                    FirstName = userEmail.Split('@').FirstOrDefault() ?? string.Empty
                };

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user info");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
