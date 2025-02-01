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
using Helper.AzureOpenAISearchConfiguration;
using Plugins;
using Helper.ApprovalTermStrategy;
using Helper.AgentContainer;
using Microsoft.SemanticKernel.Agents;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Azure.Identity;
using System.Net;
using Azure.Core;
using Microsoft.SemanticKernel.Agents.Chat;
using System.ComponentModel;
using System.Xml.Linq;
using System;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.AspNetCore.Identity.UI.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace api_gen_ai_itops.Controllers
{
    [ApiController]
    [Route("chat")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IChatCompletionService _chat;
        private readonly Kernel _kernel;
        private readonly Configuration _configuration;
        private readonly IChatHistoryManager _chatHistoryManager;
        private readonly ChatHistory _chatHistory;
        private readonly AISearchPlugin _aiSearchPlugin;
        private readonly RunbookPlugin _runbookPlugin;
        private readonly GitHubWorkflowPlugin _gitHubWorkflowPlugin;
        private readonly WeatherPlugin _weatherPlugin;
        private ICosmosDbService _azureCosmosDbService;

        public ChatController(
            ILogger<ChatController> logger,
            ILoggerFactory loggerFactory,
            IChatCompletionService chat,
            Kernel kernel,
            ChatHistory chatHistory,
            Configuration configuration,
            IChatHistoryManager chatHistoryManager,
            AISearchPlugin aiSearchPlugin,
            RunbookPlugin runbookPlugin,
            GitHubWorkflowPlugin gitHubWorkflowPlugin,
            WeatherPlugin weatherPlugin,
            ICosmosDbService azureCosmosDbService)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _chat = chat;
            _kernel = kernel;
            _chatHistory = chatHistory;
            _configuration = configuration;
            _chatHistoryManager = chatHistoryManager;
            _aiSearchPlugin = aiSearchPlugin;
            _runbookPlugin = runbookPlugin;
            _gitHubWorkflowPlugin = gitHubWorkflowPlugin;
            _weatherPlugin = weatherPlugin;
            _azureCosmosDbService = azureCosmosDbService;
        }

        [SwaggerOperation(
            Summary = "Send a user prompt to get a response from the Agents",
            Description = "Returns a response from the Agents for the user prompt"
        )]
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] ChatProviderRequest chatRequest)
        {
            // Request body example:
            /*
                {
                    "userId": "stevesmith@contoso.com",
                    "sessionId": "12345678",
                    "tenantId": "00001",
                    "chatName": "New Chat",
                    "prompt": "Hello, What can you do for me?"
                }
            */
            _logger.LogDebug("Starting Chat with debug mode enabled");
            _chatHistory.Clear(); // let's make sure we have a clear chathisotry with each request

            try
            {
                if (string.IsNullOrEmpty(chatRequest.SessionId))
                {
                    chatRequest.SessionId = Guid.NewGuid().ToString();
                }

                if (string.IsNullOrEmpty(chatRequest.Prompt))
                {
                    _logger.LogWarning("Chat request is missing prompt.");
                    return new BadRequestResult();
                }

                // insert session if it doesn't already exist
                bool sessionExists = await _azureCosmosDbService.SessionExists(chatRequest.SessionId);
                if (!sessionExists)
                {
                    Session session = new Session
                    {
                        Id = Guid.NewGuid().ToString(),
                        SessionId = chatRequest.SessionId,
                        UserId = chatRequest.UserId,
                        Name = chatRequest.ChatName,
                        Type = "session",
                        Timestamp = DateTime.UtcNow
                    };

                    await _azureCosmosDbService.InsertSessionAsync(session);
                }

                // Add all chat history from the Session stored in CosmosDB to the Scoped ChatHistory so SK has context of this session's conversation
                List<Message> messages = await _azureCosmosDbService.GetSessionMessagesAsync(chatRequest.SessionId);
                foreach (Message msg in messages)
                {
                    _chatHistory.AddUserMessage(msg.Prompt);
                }

                // add the current user prompt to the Scoped ChatHistory
                _chatHistory.AddUserMessage(chatRequest.Prompt);
                // add the new user message to Cosmos
                await CreateNewMessage("message", "user", chatRequest.SessionId, chatRequest.Prompt);

                // we should be able to remove this
                //var sessionId = chatRequest.SessionId;
                //// var chatHistory = _chatHistoryManager.GetOrCreateChatHistory(sessionId);
                //_chatHistory.AddUserMessage(chatRequest.Prompt); // add user message to chatHistory


                // Create agent container with all necessary dependencies
                var agentContainer = new AgentContainer(_chat, _kernel, _aiSearchPlugin, _runbookPlugin, _gitHubWorkflowPlugin, _weatherPlugin);

                // Process the chat request with existing history
                var response = await agentContainer.ProcessChatRequestAsync(chatRequest.Prompt, _chatHistory);

                // Update the session chat history with the new messages
                
                if (!string.IsNullOrWhiteSpace(response.AssistantResponse))
                {
                    _chatHistory.AddAssistantMessage(response.AssistantResponse);
                    // add the new user message to Cosmos
                    await CreateNewMessage("message", "Assistant", chatRequest.SessionId, response.AssistantResponse);
                }
                if (!string.IsNullOrWhiteSpace(response.SpecialistResponse))
                {
                    _chatHistory.AddAssistantMessage(response.SpecialistResponse);
                    await CreateNewMessage("message", "Specialist", chatRequest.SessionId, response.SpecialistResponse);
                }

                // RDC : Debugging List Workflow issue.  the ProcessChatRequestAsync is handling the request properly, it's something on the UI side that 
                // is not handling this.

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, "Internal server error.");
            }
        }

        private async Task CreateNewMessage(string type, string sender, string sessionid, string prompt)
        {
            /******** Create message for this session in cosmos DB ********/
            var message = new Message()
            {
                Id = Guid.NewGuid().ToString(),

                Type = type,
                Sender = sender,
                SessionId = sessionid,
                TimeStamp = DateTime.UtcNow,
                Prompt = prompt,
            };

            // Insert user prompt
            await _azureCosmosDbService.InsertMessageAsync(message);

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
