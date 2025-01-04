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
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;
using Azure.Core;
using Microsoft.Extensions.Logging.Console;
using Azure;
using System.Net;

var configuration = new Configuration();
new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables()
    .AddJsonFile("local.settings.json")
    .Build()
    .Bind(configuration);

configuration.Validate();

var isDebugMode = configuration.DebugMode;

// This section of Code is only here to allow the testing of AppInsights Logging.
// I only use this if the OpenTelemetry code is having issues and I am not seeing things in App Insights.
// 

#region Testing AppInsights
var checkAppInsights = false;

if (checkAppInsights)
{
    // When this code executes you should see a TestActivity logged to AppInsights
    // This will verify that you logging to AppInsights is working
    string AppInsightsConnection= configuration.AzureAppInsights ?? "";
    using var tracerProvider = Sdk.CreateTracerProviderBuilder()
           .AddSource("DemoSource")
           .AddAzureMonitorTraceExporter(options =>
           {
               options.ConnectionString = AppInsightsConnection;
           })
           .Build();

    // Manually create a trace
    var activitySource = new ActivitySource("DemoSource");
    using (var activity = activitySource.StartActivity("TestActivity"))
    {
        activity?.SetTag("demo", "test");
    }
}
#endregion End Testing AppInsights

// Create an ActivitySource that matches your .AddSource name:
//var manualSource = new ActivitySource("TelemetryMyExample");



#region Enable OpenTelemetry for Semantic Kernel
// If you want to see all the calls that are happening with LLM, this is the best way to get that type of telemetry.
// Likely you could leverage something like AppContext to enable this or just use an environment setting
// 
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);
    string AppInsightsConnectionString = configuration.AzureAppInsights ?? "";  // Uncomment this if you plan top use OpenTelemetry
    // Using resource builder to add service name to all telemetry items
    var resourceBuilder = ResourceBuilder
        .CreateDefault()
        .AddService("TelemetryMyExample");
    // Create the OpenTelemetry TracerProvider and MeterProvider
    using var traceProvider = Sdk.CreateTracerProviderBuilder()
        .SetResourceBuilder(resourceBuilder)
        .AddSource("Microsoft.SemanticKernel*")
        .AddSource("TelemetryMyExample")
        // .AddConsoleExporter()
        .AddAzureMonitorTraceExporter(options => options.ConnectionString = AppInsightsConnectionString)
        .Build();

    using var meterProvider = Sdk.CreateMeterProviderBuilder()
        .SetResourceBuilder(resourceBuilder)
        .AddMeter("Microsoft.SemanticKernel*")
        //.AddConsoleExporter()
        .AddAzureMonitorMetricExporter(options => options.ConnectionString = AppInsightsConnectionString)
        .Build();

    // Create the OpenTelemetry LoggerFactory
    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        // Add OpenTelemetry as a logging provider
        builder.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
            options.AddAzureMonitorLogExporter(options => options.ConnectionString = AppInsightsConnectionString);
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
        });

        // Get log level from configuration, default to Information if not specified
        var logLevel = configuration.LogLevel?.ToUpper() switch
        {
            "DEBUG" => LogLevel.Debug,
            "TRACE" => LogLevel.Trace,
            "INFORMATION" => LogLevel.Information,
            "WARNING" => LogLevel.Warning,
            "ERROR" => LogLevel.Error,
            "CRITICAL" => LogLevel.Critical,
            _ => LogLevel.Information // Default to Information if not specified
        };

        // Add console logging if debug mode is enabled or log level is specified
        if (isDebugMode || configuration.LogLevel != null)
        {
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "[HH:mm:ss] ";
                options.SingleLine = true;
                options.UseUtcTimestamp = true;
                options.ColorBehavior = LoggerColorBehavior.Disabled;
            });
        }

        builder.SetMinimumLevel(logLevel);
    });

    // Create a logger for agent communication
    var agentLogger = loggerFactory.CreateLogger("AgentCommunication");
# endregion End of Enable OpenTelemetry for Semantic Kernel
// Initialize kernel with chat completion service
Kernel kernel = CreateKernelWithChatCompletion();

// Aquire Crednetials
Console.WriteLine("A browser window will open for authentication. Please select your account...");
var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
{
    TokenCachePersistenceOptions = new TokenCachePersistenceOptions()
});

// Validate the credential by acquiring a token (one-time prompt)
await credential.GetTokenAsync(new TokenRequestContext(new[] { "https://management.azure.com/.default" }));
Console.WriteLine("Authentication successful!");

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
    Name = "assistant",
    Instructions = """
        You are an Orchestrator Agent that evaluates user requests.
        
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
    Kernel = kernel,
    Arguments = new KernelArguments(
        new OpenAIPromptExecutionSettings()
        {
            ServiceId = "azure-openai",
        })
};

// Update RunbookAgent instructions
ChatCompletionAgent runbookAgent = new ChatCompletionAgent
{
    Name = "specialist",
    Instructions = """
        You are a Runbook Agent specialized in IT Operations. Your tasks include:
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
    Kernel = kernel,
    Arguments = new KernelArguments(
        new OpenAIPromptExecutionSettings
        {
            ServiceId = "azure-openai",
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        }
    )
};

// Create loggers for each plugin
var aiSearchLogger = loggerFactory.CreateLogger<AISearchPlugin>();
var runbookLogger = loggerFactory.CreateLogger<RunbookPlugin>();
var echoLogger = loggerFactory.CreateLogger<EchoPlugin>();

// Create plugins with their loggers
KernelPlugin aisearchplugin = kernel.ImportPluginFromObject(new AISearchPlugin(configuration, credential, aiSearchLogger));
KernelPlugin runbookplugin = kernel.ImportPluginFromObject(new RunbookPlugin(configuration, credential, runbookLogger));

// Only register the EchoPlugin if debug logging is enabled
if (echoLogger.IsEnabled(LogLevel.Debug))
{
    var echoPlugin = new EchoPlugin(echoLogger);
    kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(echoPlugin));
    agentLogger.LogDebug("EchoPlugin registered due to debug logging being enabled");
}

string OrchestratorAgentName = "assistant";
string RunbookAgentName = "specialist";

var selectionFunction2 = KernelFunctionFactory.CreateFromPrompt(
    $$$"""
    Your job is to determine which participant takes the next turn in a conversation.
    Return only the name: "{{{OrchestratorAgentName}}}" or "{{{RunbookAgentName}}}".
            
    Rules (in order of priority):
    1. For NEW user requests, return "{{{OrchestratorAgentName}}}"
    2. After seeing "ROUTE_TO_SPECIALIST", keep returning "{{{RunbookAgentName}}}" until seeing "OPERATION_COMPLETE"
    3. After seeing "OPERATION_COMPLETE", return "{{{OrchestratorAgentName}}}"
    4. For status check responses (yes/no), return "{{{RunbookAgentName}}}"
    5. If unsure, return "{{{OrchestratorAgentName}}}"

    Remember: 
    - After a request is routed once, DO NOT route it again
    - Keep track if a request has already been routed
    - Stay with {{{RunbookAgentName}}} until the operation is complete

    History:
    {{$chatHistory}}

    Return only the name, no explanation.
    """);

var selectionFunction = KernelFunctionFactory.CreateFromPrompt(
    $$$"""
    Your job is to determine which participant takes the next turn in a conversation.
    Return only the name: "{{{OrchestratorAgentName}}}" or "{{{RunbookAgentName}}}".
            
    Rules (in strict priority order):
    1. If this is a NEW user request (not a response to a status check), return "{{{OrchestratorAgentName}}}"
    2. If the last message was a status check response from {{{RunbookAgentName}}}, return "{{{OrchestratorAgentName}}}" to ask if there's anything else
    3. If the user has responded "yes" to a status check and it hasn't been processed yet, return "{{{RunbookAgentName}}}"
    4. After seeing "ROUTE_TO_SPECIALIST", return "{{{RunbookAgentName}}}" until seeing "OPERATION_COMPLETE"
    5. After "OPERATION_COMPLETE", return "{{{OrchestratorAgentName}}}" for wrap-up
    
    Check carefully:
    - Is this a status check response? ({{{RunbookAgentName}}} just reported a status)
    - Is this a "yes" to check status? (User responding to "Would you like to check status?")
    - Has this request already been routed once?

    History:
    {{$chatHistory}}

    Return only the exact agent name, no explanation.
    """);

// Update selection function to better handle job status checks and chat history
var selectionFunction1 = KernelFunctionFactory.CreateFromPrompt(
    $$$"""
    Your job is to determine which participant takes the next turn in a conversation based on the most recent message and chat history.
    Return only the name of the next participant: either "{{{OrchestratorAgentName}}}" or "{{{RunbookAgentName}}}".
            
    Selection rules:
    1. Initial user input or new requests always go to "{{{OrchestratorAgentName}}}"
    2. After "{{{OrchestratorAgentName}}}" sends "ROUTE_TO_SPECIALIST", next message goes to "{{{RunbookAgentName}}}"
    3. After "{{{RunbookAgentName}}}" completes an operation ("OPERATION_COMPLETE"), next message goes to "{{{OrchestratorAgentName}}}"
    4. For job status checks:
       - If user asks about status or says "yes" to a status check, route to "{{{RunbookAgentName}}}"
       - After status is reported, return to "{{{OrchestratorAgentName}}}"
    
    Important:
    - NEVER return "user" as the next participant
    - Only one agent should respond at a time
    - After "{{{RunbookAgentName}}}" reports a status, control returns to "{{{OrchestratorAgentName}}}"
    - Always return EXACTLY one of these two values: "{{{OrchestratorAgentName}}}" or "{{{RunbookAgentName}}}"

    History:
    {{$chatHistory}}

    Based on this history and the rules above, who should respond next?
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
                        // Increase maximum iterations to allow for longer conversations
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
                        HistoryVariableName = "chatHistory2",
                    },
            }
    };

    Console.WriteLine("IT Operations Assistant (type 'exit' to quit)");
    Console.WriteLine("----------------------------------------");
bool isComplete = false;

agentLogger.LogDebug("Starting chat with debug mode enabled");
agentLogger.LogDebug("Orchestrator Agent Instructions: {Instructions}", orchestratorAgent.Instructions);
agentLogger.LogDebug("Runbook Agent Instructions: {Instructions}", runbookAgent.Instructions);

do
{
    var currentGroupChat = new AgentGroupChat(orchestratorAgent, runbookAgent);
    try
    {
        Console.Write("\nUser > ");
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
        
        agentLogger.LogDebug("User Input: {Input}", userInput);
        Console.WriteLine();

        // Configure the new group chat
        /*
        currentGroupChat.ExecutionSettings = new()
        {
            TerminationStrategy = new ApprovalTerminationStrategy()
            {
                Agents = [orchestratorAgent],
                MaximumIterations = 3, // Allow for Orchestrator -> Runbook Agent handoff
            },
            SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, CreateKernelWithChatCompletion())
            {
                ResultParser = (result) => result.GetValue<string>() ?? "OrchestratorName",
                AgentsVariableName = "agents",
                HistoryVariableName = "chatHistory",
            },
        };
        */

        // Inside your main loop, before processing messages:
        var hasBeenRouted = chatHistory.Any(m => m.Content?.Contains("ROUTE_TO_SPECIALIST") == true);
        var operationComplete = chatHistory.Any(m => m.Content?.Contains("OPERATION_COMPLETE") == true);

        currentGroupChat.ExecutionSettings = new()
        {
            TerminationStrategy = new ApprovalTerminationStrategy()
            {
                Agents = [orchestratorAgent],
                MaximumIterations = 2, // Keep at 2 for basic handoff
            },
            SelectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, CreateKernelWithChatCompletion())
            {
                ResultParser = (result) =>
                {
                    var nextAgent = result.GetValue<string>()?.Trim();
                    agentLogger.LogDebug("Selection - HasBeenRouted: {HasBeenRouted}, OperationComplete: {OperationComplete}", 
                        hasBeenRouted, operationComplete);

                    // If already routed and trying to route again, force to runbook agent
                    if (hasBeenRouted && !operationComplete && nextAgent == OrchestratorAgentName)
                    {
                        agentLogger.LogDebug("Forcing continuation with Runbook Agent");
                        return RunbookAgentName;
                    }

                    return nextAgent ?? OrchestratorAgentName;
                },
                AgentsVariableName = "agents",
                HistoryVariableName = "chatHistory"
            }
        };

        // Get the last 5 messages from chat history for context
        var recentHistory = chatHistory.TakeLast(5).ToList();
        agentLogger.LogDebug("Including {Count} messages from recent history", recentHistory.Count);
        
        // Add recent history to the new group chat
        foreach (var historicalMessage in recentHistory)
        {
            currentGroupChat.AddChatMessage(new ChatMessageContent(
                historicalMessage.Role,
                historicalMessage.Content,
                historicalMessage.AuthorName
            ));
            agentLogger.LogDebug("Context from history - {Author}: {Content}", 
                historicalMessage.AuthorName, 
                historicalMessage.Content);
        }

        // Add the current user's message
        currentGroupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userInput));
        chatHistory.AddMessage(AuthorRole.User, userInput);

        try
        {
            await foreach (var response in currentGroupChat.InvokeAsync())
            {
                agentLogger.LogDebug("Agent Response - {AuthorName}: {Content}", 
                    response.AuthorName, 
                    response.Content);

                // Filter out the control messages from user display
                var displayContent = response.Content!
                    .Replace("ROUTE_TO_SPECIALIST", "")
                    .Replace("OPERATION_COMPLETE", "")
                    .Replace("DONE!", "")
                    .Trim();

                // Only display if there's actual content after filtering
                if (!string.IsNullOrWhiteSpace(displayContent))
                {
                    var displayName = response.AuthorName switch
                    {
                        "assistant" => "Assistant",
                        "specialist" => "IT Specialist",
                        _ => response.AuthorName
                    };

                    Console.WriteLine($"\n{displayName}: {displayContent}\n");
                }

                // Add response to chat history for context in future interactions
                chatHistory.Add(new ChatMessageContent
                {
                    Role = AuthorRole.Assistant,
                    Content = response.Content!,
                    AuthorName = response.AuthorName
                });
            }
        }
        catch (HttpOperationException ex)
        {
            // Check if this is a throttling or service availability issue
            var statusCode = (int)(ex.StatusCode ?? 0);  // Cast to int
            if (statusCode == 429 || statusCode == 503)
            {
                agentLogger.LogWarning(ex, "Service is currently throttled or unavailable (Status: {StatusCode}). Waiting before retry...", statusCode);
                Console.WriteLine("\nThe service is temporarily busy. I'll wait a moment and try again...");
                
                // Wait for a short period before retrying
                await Task.Delay(TimeSpan.FromSeconds(2));
                continue;
            }
            
            // Check if this is an Azure OpenAI specific error
            if (ex.Message.Contains("model_error") || ex.Message.Contains("invalid_request_error"))
            {
                agentLogger.LogError(ex, "Azure OpenAI error: {Message}", ex.Message);
                Console.WriteLine("\nI had trouble processing that request. Could you try rephrasing it?");
                continue;
            }

            // Handle other HTTP-related errors
            agentLogger.LogError(ex, "HTTP error during chat completion: {StatusCode} - {Message}", 
                statusCode, ex.Message);
            Console.WriteLine("\nI encountered an error communicating with the service. Let me try again.");
            continue;
        }
        catch (RequestFailedException azEx)
        {
            if (azEx.Status == 429 || azEx.Status == 503)
            {
                agentLogger.LogWarning(azEx, "Azure service is throttled (Status: {Status}). Waiting before retry...", azEx.Status);
                Console.WriteLine("\nThe Azure service is temporarily busy. I'll wait a moment and try again...");
                
                // Wait for a short period before retrying
                await Task.Delay(TimeSpan.FromSeconds(2));
                continue;
            }
            
            agentLogger.LogError(azEx, "Azure request failed: {Status} - {Message}", azEx.Status, azEx.Message);
            Console.WriteLine("\nI encountered an error with the Azure service. Let me try again.");
            continue;
        }
        catch (KernelException kex) when (kex.Message.Contains("Strategy unable to select next agent"))
        {
            agentLogger.LogError(kex, "Agent selection error: {Message}", kex.Message);
            Console.WriteLine("\nKernel Error: I need to reset our conversation. Please try your request again.");
            
            // Reset the group chat
            currentGroupChat = new AgentGroupChat(orchestratorAgent, runbookAgent);
            continue;
        }
        catch (KernelException kex)
        {
            agentLogger.LogError(kex, "Other kernel error: {Message}", kex.Message);
            Console.WriteLine("\nI encountered an issue. Let me try to handle your request again.");
            continue;
        }
        catch (Exception ex)
        {
            agentLogger.LogError(ex, "Unexpected error during chat completion: {ExceptionType} - {Message}", 
                ex.GetType().Name, ex.Message);
            Console.WriteLine("\nI apologize, but something unexpected happened. Please try your request again.");
            continue;
        }
    }
    catch (Exception ex)
    {
        agentLogger.LogError(ex, "Critical error in main chat loop");
        Console.WriteLine("\nI apologize, but I encountered an unexpected error. Please try again.");
        continue;
    }
} while (!isComplete);

// Log chat history for debugging
agentLogger.LogDebug("Chat History:");
foreach (var message in chatHistory)
{
    agentLogger.LogDebug("Message: Author={AuthorName}, Role={Role}, Content={Content}", 
        message.AuthorName,
        message.Role,
        message.Content);
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
    // Very important to enable logging with the loggerFactory otherwise the telemetry will not be logged
    // AppInsights the line of code that most folks miss is: builder.Services.AddSingleton(loggerFactory); which requires the use of dependency injection
    var builder = Kernel.CreateBuilder();
    builder.Services.AddSingleton(loggerFactory);
    builder.AddAzureOpenAIChatCompletion(
        configuration.AzureOpenAIDeployment!,
        configuration.AzureOpenAIEndpoint!,
        configuration.AzureOpenAIApiKey!,
        serviceId: "azure-openai"
    );
    return builder.Build();
    //        }
}


