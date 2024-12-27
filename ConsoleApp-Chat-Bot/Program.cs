//using System;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using Azure.Identity;
//using Azure.AI.OpenAI;
////using OpenAI;
////using OpenAI.Embeddings;
//using Azure.Search.Documents;
//using Azure.Search.Documents.Indexes;
//using Azure.Search.Documents.Indexes.Models;
//using Azure.Search.Documents.Models;
//using AzureOpenAISearchConfiguration;
//using AzureOpenAISearchHelper;
//using Microsoft.Extensions.Configuration;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.Agents;
//using Microsoft.SemanticKernel.Agents.Chat;
//using Microsoft.SemanticKernel.Agents.OpenAI;
//using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
//using Microsoft.SemanticKernel.Connectors.OpenAI;
//using Microsoft.SemanticKernel.ChatCompletion;
//using ConsoleApp_Chat_Bot.Prompts;
//using ConsoleApp_Chat_Bot.Helper;

//// dotnet add package Microsoft.SemanticKernel.Agents.Core --version 1.32.0-alpha
//// dotnet add package Microsoft.SemanticKernel.Agents.OpenAI --version 1.32.0-alpha
//// dotnet add package Microsoft.SemanticKernel.Agents.Abstractions --version 1.32.0-alpha

//// Create the Configuration Object and AISearchHelper
//var configuration = new Configuration();
//var aiSearchHelper = new AISearchHelper();
//new ConfigurationBuilder()
//    .SetBasePath(Directory.GetCurrentDirectory())
//    .AddEnvironmentVariables()
//    .AddJsonFile("local.settings.json")
//    .Build()
//    .Bind(configuration);

//configuration.Validate();


//// Initialize a Kernel with a chat-completion service
//IKernelBuilder builder = Kernel.CreateBuilder();

////builder.AddAzureOpenAIChatCompletion(
////     "gpt-35-turbo",                      // Azure OpenAI Deployment Name
////     "https://contoso.openai.azure.com/", // Azure OpenAI Endpoint
////     "...your Azure OpenAI Key...");      // Azure OpenAI Key

//builder.AddAzureOpenAIChatCompletion(configuration.AzureOpenAIDeployment!, configuration.AzureOpenAIEndpoint!, configuration.AzureOpenAIApiKey!, serviceId: "service-1");

//Kernel kernel = builder.Build();

//ChatCompletionAgent orchestratorAgent =
//    new()
//    {
//        Name = "OrchestratorAgent",
//        Instructions = CorePrompts.OrchestratorAgentInstructions,
//        Kernel = kernel,
//        Arguments = // Specify the service-identifier via the KernelArguments
//          new KernelArguments(
//            new OpenAIPromptExecutionSettings()
//            {
//                ServiceId = "service-1" // The target service-identifier.
//            }),
//    };

//ChatCompletionAgent runbookAgent =
//    new()
//    {
//        Name = "RunbookAgent",
//        Instructions = "<agent instructions>",
//        Kernel = kernel,
//        Arguments = // Specify the service-identifier via the KernelArguments
//          new KernelArguments(
//            new OpenAIPromptExecutionSettings()
//            {
//                ServiceId = "service-1", // The target service-identifier.
//                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
//            }),
//    };

//// Next we need to define the plugins available to the agent.
//// runbookAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromType<AISearchPlugin>());
//// runbookAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromType<AISearchPlugin>());

//// Create a ChatHistory object to maintain the conversation state.
//ChatHistory chathistory = [];

//// Add a user message to the conversation
//chathistory.Add(new ChatMessageContent(AuthorRole.User, "Is the Sky Blue?"));

//string OrchestratorAgentName = "OrchestratorAgent";
//string RunbookAgentName = "RunbookAgent";

//KernelFunction selectionFunction =
//    KernelFunctionFactory.CreateFromPrompt(
//        $$$"""
//        Your job is to determine which participant takes the next turn in a conversation according to the action of the most recent participant.
//        State only the name of the participant to take the next turn.

//        Choose only from these participants:
//        - {{{OrchestratorAgentName}}}
//        - {{{RunbookAgentName}}}

//        Always follow these rules when selecting the next participant:
//        - After user input, it is {{{OrchestratorAgentName}}}'s turn.
//        - After {{{OrchestratorAgentName}}} replies, it is {{{RunbookAgentName}}}'s turn.
//        - After {{{RunbookAgentName}}} replies, it is {{{OrchestratorAgentName}}}'s turn.

//        History:
//        {{$chathistory}}
//        """);


//AgentGroupChat groupchat =
//    new(orchestratorAgent, runbookAgent)
//    {
//        ExecutionSettings =
//            new()
//            {
//                // TerminationStrategy is used to termine whern the Orchestration Agent says "DONE!"
//                TerminationStrategy =
//                    new ApprovalTerminationStrategy()
//                    {
//                        // Only the Orchestrator Agent may consider this done
//                        Agents = [orchestratorAgent],
//                        // Limit total number of turns
//                        MaximumIterations = 10,
//                    },
//                // Here a KernelFunctionSelectionStrategy selects agents based on a prompt function
//                SelectionStrategy =
//                    new KernelFunctionSelectionStrategy(selectionFunction, CreateKernelWithChatCompletion())
//                    {
//                        // Returns the entire result value as a string.
//                        ResultParser = (result) => result.GetValue<string>() ?? OrchestratorAgentName,
//                        // The prompt variable name for the agents argument.
//                        AgentsVariableName = "agents",
//                        // The prompt variable name for the history argument.
//                        HistoryVariableName = "chathistory",
//                    },
//            }
//    };


//// invoke the chat and display messages
//string agentsTask =
//    """
//    Can you tell me why the sky is blue?
//    """;

//groupchat.AddChatMessage(new ChatMessageContent(AuthorRole.User, agentsTask));
//Console.WriteLine($"# {AuthorRole.User}: '{agentsTask}\n");

//await foreach (var content in groupchat.InvokeAsync())
//{
//    // SetConsoleForegroundColor(content.AuthorName);

//    Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'\n");

//    /*if (content.Content.Contains("anything else", StringComparison.OrdinalIgnoreCase))
//    {
//        string userInput = Console.ReadLine();

//        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userInput));
//    }*/
//}
//// Generate the agent response(s)

//Kernel CreateKernelWithChatCompletion()
//        {
//            // Create the Kernel
//            Kernel kernel = Kernel.CreateBuilder()
//                .AddAzureOpenAIChatCompletion(configuration.AzureOpenAIDeployment!, configuration.AzureOpenAIEndpoint!, configuration.AzureOpenAIApiKey!, serviceId: "service-1")
//                .Build();

//return kernel;
//        }

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ConsoleApp_Chat_Bot.Helper;
using AzureOpenAISearchConfiguration;
using Plugins;
using System;
using System.Data;

var configuration = new Configuration();
new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables()
    .AddJsonFile("local.settings.json")
    .Build()
    .Bind(configuration);

configuration.Validate();

// Initialize kernel with chat completion service
Kernel kernel = CreateKernelWithChatCompletion();

//var kernel = Kernel.CreateBuilder()
//    .AddAzureOpenAIChatCompletion(
//        configuration.AzureOpenAIDeployment!,
//        configuration.AzureOpenAIEndpoint!,
//        configuration.AzureOpenAIApiKey!,
//        serviceId: "azure-openai"
//    )
//    .Build();

// Create Orchestrator Agent
ChatCompletionAgent orchestratorAgent = new ChatCompletionAgent
{
    Name = "OrchestratorAgent",
    Instructions = """
        You are an Orchestrator Agent that evaluates user requests.
        If the request is related to IT Operations (like server management, VMs, user access, system updates),
        route it to the Runbook Agent and continue the conversation.
        
        When routing to Runbook Agent:
        1. State you are forwarding the request
        2. Include the exact request details
        3. End with "ROUTE_TO_RUNBOOK"
        
        For non-IT queries:
        1. Provide a direct response, but only if the quesiton is related to IT Operations.  If the question is not IT Operations related simply respond letting the user you only help with IT Operations.
        2. End with "DONE!"
        """,
    Kernel = kernel,
    Arguments = // Specify the service-identifier via the KernelArguments
          new KernelArguments(
            new OpenAIPromptExecutionSettings()
            {
                ServiceId = "azure-openai", // The target service-identifier.
                // ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            }),

    // Rest of the configuration remains the same
};

// Update RunbookAgent instructions
ChatCompletionAgent runbookAgent = new ChatCompletionAgent
{
    Name = "RunbookAgent",
    Instructions = """
        You are a Runbook Agent specialized in IT Operations. Your tasks include:
        1. Always Echo the repsonse back to the use by calling the EchoPlugin
        3. Search for the IT Operations availabel by calling the AISearchPlugin 
        4. Processing IT operation requests by Calling the RunbookPlugin (if not available, don't try to call it) 
        4. Executing necessary system commands
        5. Providing detailed status updates
        
        For each request:
        1. Invoke the AISearchPlugin to find the operations available for the request
        2. Based on the operations that were found to best align with the user request, as for additional details if needed.
        3. End EVERY response with "OPERATION_COMPLETE"

        Important: You must end EVERY response with "OPERATION_COMPLETE", even if you cannot fulfill the request
        """,
    Kernel = kernel,
    Arguments = new KernelArguments(
        new OpenAIPromptExecutionSettings
        {
            ServiceId = "azure-openai",
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        }
    )
};

KernelPlugin echoplugin = KernelPluginFactory.CreateFromType<EchoPlugin>();
KernelPlugin aisearchplugin = KernelPluginFactory.CreateFromType<AISearchPlugin>();
runbookAgent.Kernel.Plugins.Add(echoplugin);
runbookAgent.Kernel.Plugins.Add(aisearchplugin);

string OrchestratorAgentName = "OrchestratorAgent";
string RunbookAgentName = "RunbookAgent";

// Update selection function to handle RunbookAgent transitions
var selectionFunction = KernelFunctionFactory.CreateFromPrompt(
    $$$"""
    Your job is to determine which participant takes the next turn in a conversation based on the most recent message.
    Return only the name of the next participant.
            
    Participants:
    - {{{OrchestratorAgentName}}}
    - {{{RunbookAgentName}}}
            
    Selection rules:
    - After user input: {{{OrchestratorAgentName}}}
    - After "ROUTE_TO_RUNBOOK": {{{RunbookAgentName}}}
    - After "OPERATION_COMPLETE": {{{OrchestratorAgentName}}}
    - After "DONE!": end conversation
    
    Note: If{{{RunbookAgentName}}} is mentioned but doesn't end with "OPERATION_COMPLETE", continue with RunbookAgent

    History:
    {{$chatHistory}}
    """);

// Configure group chat
// Initialize chat history
var chatHistory = new ChatHistory();


AgentGroupChat groupchat =
    new(orchestratorAgent, runbookAgent)
    {
        ExecutionSettings =
            new()
            {
                // TerminationStrategy is used to termine whern the Orchestration Agent says "DONE!"
                TerminationStrategy =
                    new ApprovalTerminationStrategy()
                    {
                        // Only the Orchestrator Agent may consider this done
                        Agents = [orchestratorAgent],
                        // Limit total number of turns
                        MaximumIterations = 3,
                    },
                // Here a KernelFunctionSelectionStrategy selects agents based on a prompt function
                SelectionStrategy =
                    new KernelFunctionSelectionStrategy(selectionFunction, CreateKernelWithChatCompletion())
                    {
                        // Returns the entire result value as a string.
                        ResultParser = (result) => result.GetValue<string>() ?? "OrchestratorName",
                        // The prompt variable name for the agents argument.
                        AgentsVariableName = "agents",
                        // The prompt variable name for the history argument.
                        HistoryVariableName = "chatHistory",
                    },
            }
    };

    Console.WriteLine("IT Operations Assistant (type 'exit' to quit)");
    Console.WriteLine("----------------------------------------");
bool isComplete = false;

do
{
    Console.WriteLine($"\n[IS COMPLETED: {groupchat.IsComplete}]");
    Console.Write("\nYour request: ");
    string userInput = Console.ReadLine() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(userInput))
    {
        continue;
    }
    if (userInput.Trim().Equals("EXIT", StringComparison.OrdinalIgnoreCase))
    {
        isComplete = true;
        break;
    }
    
    Console.WriteLine();
    groupchat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userInput));
    // Add user message to ChatHistory
    chatHistory.AddMessage(AuthorRole.User, userInput);
    await foreach (var response in groupchat.InvokeAsync())
    {
        Console.WriteLine($"\n{response.AuthorName}: {response.Content}\n");
        // Add agent response to ChatHistory
        //var chatMessageContent = new ChatMessageContent(
        //    Role = AuthorRole.Assistant,
        //    Content = response.Content!,
        //    AuthorName = response.AuthorName,
        //    );
        chatHistory.Add(new ChatMessageContent
        {
            Role = AuthorRole.Assistant,
            Content = response.Content!,
            AuthorName = response.AuthorName
        });
        //chatHistory.Add(AuthorRole.Assistant, response.Content!, authorName: response.AuthorName);

    }
    Console.WriteLine($"\n[IS COMPLETED: {groupchat.IsComplete}]");
    if (groupchat.IsComplete)
    {
        isComplete = true;
        break;
    }

} while (!isComplete);

foreach (var message in chatHistory)
{
    Console.WriteLine($"Author Name: {message.AuthorName}");
    Console.WriteLine($"Role: {message.Role}");
    Console.WriteLine($"Content: {message.Content}");
    Console.WriteLine("-------------");
}



//while (true)
//{
//    Console.Write("\nYour request: ");
//    string userInput = Console.ReadLine() ?? string.Empty;

//    if (userInput.ToLower() == "exit") break;

//    groupchat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userInput));

//    await foreach (var response in groupchat.InvokeAsync())
//    {
//        Console.WriteLine($"\n{response.AuthorName}: {response.Content}\n");
//    }
//    Console.WriteLine($"\n[IS COMPLETED: {groupchat.IsComplete}]");
//}

Kernel CreateKernelWithChatCompletion()
{
    // Create the Kernel
    Kernel kernel = Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(configuration.AzureOpenAIDeployment!, configuration.AzureOpenAIEndpoint!, configuration.AzureOpenAIApiKey!, serviceId: "azure-openai")
        .Build();

    return kernel;
    //        }
}


