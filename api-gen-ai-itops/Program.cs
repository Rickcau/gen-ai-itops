using api_gen_ai_itops.Interfaces;
using api_gen_ai_itops.Middleware;
using api_gen_ai_itops.Prompts;
using api_gen_ai_itops.Services;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Helper.AzureOpenAISearchConfiguration;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Http;
using Azure.Identity;
using Azure.Core;
using Plugins;
using Microsoft.Extensions.DependencyInjection;
using api_gen_ai_itops.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Cors;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true); 
// RDC Old var configuration = builder.Configuration;
// RDC Old var apiDeploymentName = configuration.GetValue<string>("AzureOpenAiDeploymentName") ?? throw new ArgumentException("The AzureOpenAiDeploymentName is not configured or is empty.");
// RDC Old var apiEndpoint = configuration.GetValue<string>("AzureOpenAiEndpoint") ?? throw new ArgumentException("The AzureOpenAiEndpoint is not configured or is empty.");
// RDC Old var apiKey = configuration.GetValue<string>("AzureOpenAiKey") ?? throw new ArgumentException("The AzureOpenAiKey is not configured or is empty.");
// RDC Old var connectionString = configuration.GetValue<string>("DatabaseConnection") ?? throw new ArgumentException("The DatabaseConnection is not configured or is empty.");

// Configure and validate settings
var config = new Configuration();
builder.Configuration.Bind(config);
config.Validate();

// Add debug logging to see what's being loaded
Console.WriteLine($"CosmosDb Settings: {JsonSerializer.Serialize(config.CosmosDb, new JsonSerializerOptions { WriteIndented = true })}");

// Configure Azure credential based on environment
TokenCredential azureCredential;
if (builder.Environment.IsDevelopment())
{
    var accountToUse = builder.Configuration["Azure:AccountToUse"];
    if (string.IsNullOrEmpty(accountToUse))
        throw new ArgumentException("Azure:AccountToUse is not configured for development");
    
    azureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        TenantId = builder.Configuration["Azure:TenantId"],
        SharedTokenCacheUsername = accountToUse,
        // Exclude other credential types to ensure we only use CLI credentials
        ExcludeEnvironmentCredential = true,
        ExcludeManagedIdentityCredential = true,
        ExcludeVisualStudioCredential = true,
        ExcludeVisualStudioCodeCredential = true,
        ExcludeInteractiveBrowserCredential = true
    });

    // azureCredential = new InteractiveBrowserCredential();
}
else
{
    // Use Managed Identity in production
    if (string.IsNullOrEmpty(config.AzureManagedIdentity))
        throw new ArgumentException("AzureManagedIdentity is not configured");
    
    azureCredential = new ManagedIdentityCredential(config.AzureManagedIdentity);
}

// Add CosmosDB configuration
builder.Services.Configure<CosmosDbOptions>(options =>
{
    options.AccountUri = config.CosmosDb.AccountUri;
    options.TenantId = config.CosmosDb.TenantId;
    options.DatabaseName = config.CosmosDb.DatabaseName;
    options.ChatHistoryContainerName = config.CosmosDb.ChatHistoryContainerName;
    options.UsersContainerName = config.CosmosDb.UsersContainerName;
    options.CapabilitiesContainerName = config.CosmosDb.CapabilitiesContainerName;
});

// Register CosmosDbService as singleton
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();

// Register the credential and plugins as singletons
builder.Services.AddSingleton<TokenCredential>(azureCredential);
builder.Services.AddSingleton<AISearchPlugin>(sp => 
    new AISearchPlugin(
        sp.GetRequiredService<Configuration>(), 
        azureCredential, 
        sp.GetRequiredService<ILogger<AISearchPlugin>>()
    ));
builder.Services.AddSingleton<RunbookPlugin>(sp => 
    new RunbookPlugin(
        sp.GetRequiredService<Configuration>(), 
        azureCredential, 
        sp.GetRequiredService<ILogger<RunbookPlugin>>()
    ));
builder.Services.AddSingleton<GitHubWorkflowPlugin>(sp => 
    new GitHubWorkflowPlugin(
        sp.GetRequiredService<Configuration>(),
        sp.GetRequiredService<ILogger<GitHubWorkflowPlugin>>()
    ));
builder.Services.AddSingleton<WeatherPlugin>(sp =>
    new WeatherPlugin(
        sp.GetRequiredService<Configuration>(),
        sp.GetRequiredService<ILogger<WeatherPlugin>>()
    ));

// Add services to the container.
// RDC Old builder.Services.AddApplicationInsightsTelemetry();
// RDC Old builder.Logging.AddConsole();
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = config.AzureAppInsights;
});
builder.Services.AddLogging(logging =>
{
    if (config.DebugMode)
    {
        logging.SetMinimumLevel(LogLevel.Debug);
    }
});

// Configure global HTTP client defaults
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler().Configure(options =>
    {
        options.Retry.MaxRetryAttempts = 1;
        options.Retry.Delay = TimeSpan.FromSeconds(2);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
    });
});

// Register Configuration as a singleton
builder.Services.AddSingleton(config);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyApp",
        policy =>
        {
            // RDC Old policy.WithOrigins("http://localhost:3000", "https://localhost:3443", "https://localhost:3001")  // Remove the API URL and trailing slash
            policy.WithOrigins("http://localhost:3000", "https://localhost:3443", "https://localhost:3001")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// RDC Old builder.Services.AddTransient<IAzureDbService>(s => new AzureDbService(connectionString));
builder.Services.AddTransient<IAzureDbService>(s => 
    new AzureDbService(config.DatabaseConnection ?? throw new ArgumentException("DatabaseConnection is not configured")));

builder.Services.AddTransient<Kernel>(s =>
{
    var builder = Kernel.CreateBuilder();
    // RDC Old builder.AddAzureOpenAIChatCompletion(apiDeploymentName, apiEndpoint, apiKey);
    builder.AddAzureOpenAIChatCompletion(
        config.AzureOpenAIDeployment ?? throw new ArgumentException("AzureOpenAIDeployment is not configured"),
        config.AzureOpenAIEndpoint ?? throw new ArgumentException("AzureOpenAIEndpoint is not configured"),
        config.AzureOpenAIApiKey ?? throw new ArgumentException("AzureOpenAIApiKey is not configured"),
        serviceId: "azure-openai");

    return builder.Build();
});

builder.Services.AddSingleton<IChatCompletionService>(sp =>
                     sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());

builder.Services.AddScoped<ChatHistory>(s =>
{
    var chathistory = new ChatHistory();
    return chathistory;
});

// We can remove the ChatHistory Manager once we have the Cosmos persisting the chathistory.

builder.Services.AddSingleton<api_gen_ai_itops.Services.IChatHistoryManager, ChatHistoryManager>();

builder.Services.AddHostedService<ChatHistoryCleanupService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    // Configure API key for Swagger
    c.AddSecurityDefinition("api-key", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter your API key",
        Name = "api-key",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "api-key"
                    }
                },
                new string[] {}
            }
        });
    c.EnableAnnotations();
});

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowMyApp");
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();