using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using api_gen_ai_itops.Prompts;
using api_gen_ai_itops.Models;
using Helper.ApprovalTermStrategy;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Linq;
using Plugins;

namespace Helper.AgentContainer
{
    /// <summary>
    /// Manages a group chat between an Assistant and a Specialist agent for handling IT operations.
    /// The Assistant agent evaluates requests and routes them to the Specialist, which handles runbook operations.
    /// </summary>
    /// <example>
    /// Usage:
    /// <code>
    /// var container = new AgentContainer(chatCompletionService, kernel);
    /// var groupChat = await container.CreateAgentGroupChatAsync();
    /// var chatHistory = await container.ExecuteGroupChatAsync(groupChat, "List all VMs");
    /// </code>
    /// </example>

    public class AgentContainer
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly Kernel _kernel;
        private readonly AISearchPlugin _aiSearchPlugin;
        private readonly RunbookPlugin _runbookPlugin;
        private const string AssistantAgentName = "Assistant";
        private const string SpecialistAgentName = "Specialist";

        /// <summary>
        /// Initializes a new instance of the AgentContainer class.
        /// </summary>
        /// <param name="chatCompletionService">The chat completion service for agent communication.</param>
        /// <param name="kernel">The semantic kernel instance for agent operations.</param>
        /// <param name="aiSearchPlugin">The AI search plugin for the specialist agent.</param>
        /// <param name="runbookPlugin">The runbook plugin for the specialist agent.</param>
        public AgentContainer(
            IChatCompletionService chatCompletionService, 
            Kernel kernel,
            AISearchPlugin aiSearchPlugin,
            RunbookPlugin runbookPlugin)
        {
            _chatCompletionService = chatCompletionService;
            _kernel = kernel;
            _aiSearchPlugin = aiSearchPlugin;
            _runbookPlugin = runbookPlugin;
        }

        /// <summary>
        /// Creates and configures a group chat with Assistant and Specialist agents.
        /// </summary>
        /// <returns>A configured AgentGroupChat ready for user interaction.</returns>
        public AgentGroupChat CreateAgentGroupChat()
        {
            // Create the assistant agent
            var assistantAgent = CreateAssistantAgent();

            // Create the specialist agent
            var specialistAgent = CreateSpecialistAgent();

            // Create the group chat with both agents
            var groupChat = new AgentGroupChat(assistantAgent, specialistAgent)
            {
                ExecutionSettings = new()
                {
                    // TerminationStrategy is used to determine when the Assistant Agent says "DONE!"
                    TerminationStrategy = new ApprovalTerminationStrategy()
                    {
                        // Only the Assistant Agent may consider this done
                        Agents = [assistantAgent],
                        // Increase maximum iterations to allow for longer conversations
                        MaximumIterations = 2,
                    },
                    // Here a KernelFunctionSelectionStrategy selects agents based on a prompt function
                    SelectionStrategy = new KernelFunctionSelectionStrategy(CreateSelectionFunction(), _kernel.Clone())
                    {
                        ResultParser = (result) =>
                        {
                            var nextAgent = result.GetValue<string>()?.Trim();
                            var history = result.ToString() ?? string.Empty;
                            var hasBeenRouted = history.Contains("ROUTE_TO_SPECIALIST");
                            var operationComplete = history.Contains("OPERATION_COMPLETE");

                            // If already routed and trying to route again, force to specialist agent
                            if (hasBeenRouted && !operationComplete && nextAgent == AssistantAgentName)
                            {
                                return SpecialistAgentName;
                            }

                            return nextAgent ?? AssistantAgentName;
                        },
                        AgentsVariableName = "agents",
                        HistoryVariableName = "chatHistory",
                    },
                }
            };

            return groupChat;
        }

        private KernelFunction CreateSelectionFunction()
        {
            return KernelFunctionFactory.CreateFromPrompt(
                $$$"""
                Your job is to determine which participant takes the next turn in a conversation.
                Return only the name: "{{{AssistantAgentName}}}" or "{{{SpecialistAgentName}}}".
                        
                Rules (in strict priority order):
                1. If this is a NEW user request (not a response to a status check), return "{{{AssistantAgentName}}}"
                2. If the last message was a status check response from {{{SpecialistAgentName}}}, return "{{{AssistantAgentName}}}" to ask if there's anything else
                3. If the user has responded "yes" to a status check and it hasn't been processed yet, return "{{{SpecialistAgentName}}}"
                4. After seeing "ROUTE_TO_SPECIALIST", return "{{{SpecialistAgentName}}}" until seeing "OPERATION_COMPLETE"
                5. After "OPERATION_COMPLETE", return "{{{AssistantAgentName}}}" for wrap-up
                
                Check carefully:
                - Is this a status check response? ({{{SpecialistAgentName}}} just reported a status)
                - Is this a "yes" to check status? (User responding to "Would you like to check status?")
                - Has this request already been routed once?

                History:
                {{$chatHistory}}

                Return only the exact agent name, no explanation.
                """);
        }

        private ChatCompletionAgent CreateAssistantAgent()
        {
            return new ChatCompletionAgent
            {
                Name = AssistantAgentName,
                Instructions = """
                    You are an Assistant Agent that evaluates user requests.
                    
                    When handling a NEW request related to IT Operations:
                    1. State "I am forwarding the request to the IT Specialist for handling."
                    2. Include the request details
                    3. End with "ROUTE_TO_SPECIALIST"
                    
                    When receiving control after a status check:
                    1. Ask "Is there anything else I can help you with?"
                    2. End with "DONE!"
                    
                    For non-IT queries:
                    1. Explain that you only help with IT Operations
                    2. End with "DONE!"
                    
                    Important:
                    - Never forward a request that has already been routed
                    - After a status check, only ask if there's anything else
                    """,
                Kernel = _kernel.Clone(),
                Arguments = new KernelArguments(
                    new OpenAIPromptExecutionSettings()
                    {
                        ServiceId = "azure-openai",
                    })
            };
        }

        private ChatCompletionAgent CreateSpecialistAgent()
        {
            var agent = new ChatCompletionAgent
            {
                Name = SpecialistAgentName,
                Instructions = """
                    You are an Specialist Agent specialized in IT Operations. Your tasks include:
                    1. Search for the IT Operations available by calling the AISearchPlugin 
                    2. Processing IT operation requests by Calling the RunbookPlugin (if not available, don't try to call it) 
                    3. Executing necessary system commands
                    4. Providing detailed status updates
                    
                    For each request:
                    1. If the request is to check a job status:
                       - First, look in the recent chat history for any mentioned Job IDs
                       - If found in history, use that Job ID automatically
                       - If not found, ask the user for the Job ID
                       - Call CheckJobStatus with only the GUID portion
                       - Present the status and output in a clear format
                       - End with "OPERATION_COMPLETE"
                    2. For other requests:
                       - Invoke the AISearchPlugin to find the operations available
                       - Based on the operations found, ask for additional details if needed
                       - Execute the appropriate runbook
                       - After providing the job ID, ask if the user would like to check the status
                       - End with "OPERATION_COMPLETE"

                    Important: 
                    - Always scan the chat history for context, especially Job IDs
                    - When checking job status, only pass the GUID portion of the job ID
                    - You must ALWAYS end your response with "OPERATION_COMPLETE"
                    - Never end your response without "OPERATION_COMPLETE"
                    """,
                Kernel = _kernel.Clone(),
                Arguments = new KernelArguments(
                    new OpenAIPromptExecutionSettings
                    {
                        ServiceId = "azure-openai",
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    }
                )
            };

            // Add plugins to the specialist agent
            agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(_aiSearchPlugin));
            agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(_runbookPlugin));

            return agent;
        }

        /// <summary>
        /// Executes a user request in the group chat and returns the formatted responses.
        /// </summary>
        /// <param name="groupChat">The configured group chat instance.</param>
        /// <param name="userInput">The user's request or message.</param>
        /// <returns>A ChatProviderResponse containing filtered agent responses.</returns>
        public async Task<ChatProviderResponse> ExecuteGroupChatAsync(AgentGroupChat groupChat, string userInput)
        {
            var response = new ChatProviderResponse();
            groupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userInput));
            
            var responses = await groupChat.InvokeAsync().ToListAsync();
            response.ChatResponse = string.Join("\n", responses.Select(r => $"{r.AuthorName}: {r.Content}"));

            foreach (var agentResponse in responses)
            {
                // Filter out control messages
                var displayContent = agentResponse.Content!
                    .Replace("ROUTE_TO_SPECIALIST", "")
                    .Replace("OPERATION_COMPLETE", "")
                    .Replace("DONE!", "")
                    .Trim();

                if (!string.IsNullOrWhiteSpace(displayContent))
                {
                    switch (agentResponse.AuthorName)
                    {
                        case "Assistant":
                            response.AssistantResponse = displayContent;
                            break;
                        case "Specialist":
                            response.SpecialistResponse = displayContent;
                            break;
                    }
                }
            }
            
            return response;
        }

        /// <summary>
        /// Gets the Assistant agent for evaluating and routing requests.
        /// </summary>
        /// <returns>A configured ChatCompletionAgent for the Assistant role.</returns>
        public ChatCompletionAgent GetAssistantAgent()
        {
            return CreateAssistantAgent();
        }

        /// <summary>
        /// Gets the Specialist agent for handling IT operations.
        /// </summary>
        /// <returns>A configured ChatCompletionAgent for the Specialist role.</returns>
        public ChatCompletionAgent GetSpecialistAgent()
        {
            return CreateSpecialistAgent();
        }

        /// <summary>
        /// Processes a chat request through the agent system, handling the interaction between Assistant and Specialist agents.
        /// </summary>
        /// <param name="userInput">The user's current input message.</param>
        /// <param name="existingChatHistory">The existing chat history from previous interactions.</param>
        /// <returns>A ChatProviderResponse containing the assistant and specialist responses.</returns>
        public async Task<ChatProviderResponse> ProcessChatRequestAsync(string userInput, ChatHistory existingChatHistory)
        {
            var response = new ChatProviderResponse();
            var chatHistory = new ChatHistory();

            // Add the last 5 messages from existing history for context
            var recentHistory = existingChatHistory.TakeLast(5).ToList();
            foreach (var historicalMessage in recentHistory)
            {
                chatHistory.Add(new ChatMessageContent(
                    historicalMessage.Role,
                    historicalMessage.Content,
                    historicalMessage.AuthorName
                ));
            }

            // Add the current user input
            chatHistory.AddUserMessage(userInput);

            var assistantAgent = GetAssistantAgent();
            var specialistAgent = GetSpecialistAgent();

            // First, let the Assistant process the request
            var assistantResponses = new List<string>();
            bool isRouteToSpecialist = false;

            await foreach (ChatMessageContent assistantResponse in assistantAgent.InvokeAsync(chatHistory))
            {
                var originalContent = assistantResponse.Content!.Trim();
                isRouteToSpecialist = originalContent.EndsWith("ROUTE_TO_SPECIALIST");

                var displayContent = originalContent
                    .Replace("ROUTE_TO_SPECIALIST", "")
                    .Replace("DONE!", "")
                    .Trim();

                if (!string.IsNullOrWhiteSpace(displayContent))
                {
                    assistantResponses.Add(displayContent);
                    chatHistory.AddAssistantMessage(displayContent);
                }
            }

            response.AssistantResponse = string.Join("\n", assistantResponses);

            // If the Assistant routed to Specialist, process with Specialist
            if (isRouteToSpecialist)
            {
                var specialistResponses = new List<string>();
                await foreach (ChatMessageContent specialistResponse in specialistAgent.InvokeAsync(chatHistory))
                {
                    var originalContent = specialistResponse.Content!.Trim();
                    var displayContent = originalContent
                        .Replace("OPERATION_COMPLETE", "")
                        .Trim();

                    if (!string.IsNullOrWhiteSpace(displayContent))
                    {
                        specialistResponses.Add(displayContent);
                        chatHistory.AddAssistantMessage(displayContent);
                    }
                }

                response.SpecialistResponse = string.Join("\n", specialistResponses);
            }

            response.ChatResponse = string.Join("\n", assistantResponses.Concat(new[] { response.SpecialistResponse }).Where(r => !string.IsNullOrWhiteSpace(r)));
            return response;
        }
    }
} 