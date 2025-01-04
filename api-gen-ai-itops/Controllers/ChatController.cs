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

namespace api_gen_ai_itops.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IChatCompletionService _chat;
        private readonly Kernel _kernel;
        private readonly Configuration _configuration;
        private readonly IChatHistoryManager _chatHistoryManager;
        private readonly AISearchPlugin _aiSearchPlugin;
        private readonly RunbookPlugin _runbookPlugin;

        public ChatController(
            ILogger<ChatController> logger,
            ILoggerFactory loggerFactory,
            IChatCompletionService chat,
            Kernel kernel,
            Configuration configuration,
            IChatHistoryManager chatHistoryManager,
            AISearchPlugin aiSearchPlugin,
            RunbookPlugin runbookPlugin)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _chat = chat;
            _kernel = kernel;
            _configuration = configuration;
            _chatHistoryManager = chatHistoryManager;
            _aiSearchPlugin = aiSearchPlugin;
            _runbookPlugin = runbookPlugin;
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] ChatProviderRequest chatRequest)
        {
            _logger.LogDebug("Starting Chat with debug mode enabled");
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

                var sessionId = chatRequest.SessionId;
                var chatHistory = _chatHistoryManager.GetOrCreateChatHistory(sessionId);

                // Create agent container with all necessary dependencies
                var agentContainer = new AgentContainer(_chat, _kernel, _aiSearchPlugin, _runbookPlugin);

                // Process the chat request with existing history
                var response = await agentContainer.ProcessChatRequestAsync(chatRequest.Prompt, chatHistory);

                // Update the session chat history with the new messages
                chatHistory.AddUserMessage(chatRequest.Prompt);
                if (!string.IsNullOrWhiteSpace(response.AssistantResponse))
                {
                    chatHistory.AddAssistantMessage(response.AssistantResponse);
                }
                if (!string.IsNullOrWhiteSpace(response.SpecialistResponse))
                {
                    chatHistory.AddAssistantMessage(response.SpecialistResponse);
                }

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
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
