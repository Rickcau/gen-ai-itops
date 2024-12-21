using api_gen_ai_itops.Interfaces;
using api_gen_ai_itops.Models;
using api_gen_ai_itops.Plugins;
using api_gen_ai_itops.Prompts;
using api_gen_ai_itops.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Net.Mime;

namespace api_gen_ai_itops.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chat;
        private readonly IChatHistoryManager _chatHistoryManager;
        private readonly IAzureDbService _azureDbService;
        private readonly string _connectionString;
        private readonly string _databaseDescription;
        private readonly string _tables;

        public ChatController(
            ILogger<ChatController> logger,
            IConfiguration configuration,
            Kernel kernel,
            IChatCompletionService chat,
            IChatHistoryManager chathistorymanager,
            IAzureDbService azuredbservice)
        {
            _kernel = kernel;
            _chat = chat;
            _chatHistoryManager = chathistorymanager;
            _logger = logger;
            _azureDbService = azuredbservice;

            _connectionString = configuration.GetValue<string>("DatabaseConnection") ?? throw new ArgumentNullException("DatabaseConnection");
            _databaseDescription = configuration.GetValue<string>("DatabaseDescription") ?? throw new ArgumentNullException("DatabaseDescription");
            _tables = configuration.GetValue<string>("Tables") ?? throw new ArgumentNullException("Tables");
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] ChatProviderRequest chatRequest)
        {
            var response = new ChatProviderResponse();

            try
            {
                if (string.IsNullOrEmpty(chatRequest.SessionId))
                {
                    // needed for new chats
                    chatRequest.SessionId = Guid.NewGuid().ToString();
                }

                if (string.IsNullOrEmpty(chatRequest.Prompt))
                {
                    _logger.LogWarning("Chat request is missing prompt.");
                    return new BadRequestResult();
                }

                var sessionId = chatRequest.SessionId;
                var chatHistory = _chatHistoryManager.GetOrCreateChatHistory(sessionId);

                // example sql for calculating median                
                var missingPersonsMedianAgeExample = $$$"""### SQL Median Age Example: Below is an example of how to build a SQL Statement to return the median age for missing persons. ### ::: Example SQL ::: WITH AgeRanking AS (     SELECT Age,            PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY Age) OVER () AS MedianAge    FROM MissingPersons    WHERE Age IS NOT NULL) SELECT DISTINCT MedianAgeFROM AgeRanking; """;

                _kernel.ImportPluginFromObject(new DBQueryPlugin(_azureDbService));

               //  var jsonSchema = await GetDatabaseSchemaAsync();

                //chatHistory.AddUserMessage(missingPersonsMedianAgeExample);
               // chatHistory.AddUserMessage(NLPSqlPluginPrompts.GetNLPToSQLSystemPrompt(jsonSchema));
                chatHistory.AddUserMessage(chatRequest.Prompt);

                ChatMessageContent? result = null;

                result = await _chat.GetChatMessageContentAsync(
                      chatHistory,
                      executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.8, TopP = 0.0, ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions },
                      kernel: _kernel);

                response.ChatResponse = result.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, "Internal server error.");
            }

            return new OkObjectResult(response);
        }

        [HttpPatch("patch-missing-person")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Patch([FromBody] MissingPersonFoundRequest request)
        {
            try
            {
                _logger.LogInformation("Incoming request: {MissingPersonFoundRequest}", request);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var person = await _azureDbService.GetMissingPerson(request.Name.Trim().ToLower(), request.Age, request.DateReported);

                if (person == null)
                {
                    _logger.LogInformation($"Pissing person not found in the database. Name:{request.Name}");
                    return NotFound();
                }

                var updated = await _azureDbService.UpdateMissingPerson(person.Id, request.DateFound);

                _logger.LogInformation($"Updated missing person. Id:{person.Id} Name:{person.Name}");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching missing person.");
                return StatusCode(500, "Internal server error.");
            }
        }

        //private async Task<string> GetDatabaseSchemaAsync()
        //{
        //    var sqlHarness = new SqlSchemaProviderHarness(_connectionString, _databaseDescription);
        //    var jsonSchema = string.Empty;
        //    var tableNames = _tables.Split("|");
        //    jsonSchema = await sqlHarness.ReverseEngineerSchemaJSONAsync(tableNames);

        //    return jsonSchema;
        //}
    }
}
