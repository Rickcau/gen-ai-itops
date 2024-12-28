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

var configuration = new Configuration();
new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables()
    .AddJsonFile("local.settings.json")
    .Build()
    .Bind(configuration);

configuration.Validate();

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
           // options.AddConsoleExporter();
            // Format log messages. This is default to false.
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
        });
        builder.SetMinimumLevel(LogLevel.Debug);
    });
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
runbookAgent.Kernel.Plugins.Add(echoplugin);
// runbookAgent.Kernel.Plugins.Add(aisearchplugin); <- this is not needed as the kernel.ImportPluginFromObject() adds the plugin to the Plugin collection; 
// KernelPlugin aisearchplugin = KernelPluginFactory.CreateFromType<AISearchPlugin>();

KernelPlugin aisearchplugin = kernel.ImportPluginFromObject(new AISearchPlugin(configuration,credential));
KernelPlugin runbookplugin = kernel.ImportPluginFromObject(new RunbookPlugin(configuration, credential));


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


