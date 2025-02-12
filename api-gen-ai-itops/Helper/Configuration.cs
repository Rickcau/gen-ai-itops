using api_gen_ai_itops.Models;
using Microsoft.Extensions.Configuration;

namespace Helper.AzureOpenAISearchConfiguration
{
    /// <summary>
    /// Configuration for the demo app. Can be filled in from local.settings.json or environment variables
    /// </summary>
    public class Configuration
    {
        
        public CosmosDbOptions CosmosDb { get; set; } = new ();

        [ConfigurationKeyName("SecuritySettings:SystemWipeKey")]
        public string? SystemWipeKey { get; set; }

        // "AZURE_MANAGED_IDENTITY"
        [ConfigurationKeyName("AZURE_MANAGED_IDENTITY")]
        public string? AzureManagedIdentity { get; set; }

        [ConfigurationKeyName("AUTH_TYPE")]
        public string? AuthType { get; set; } = "ManagedIdentity"; // Options: ManagedIdentity, VisualStudio, AzureCLI

        /// <summary>
        /// Service endpoint for the search service
        /// e.g. "https://your-search-service.search.windows.net
        /// </summary>
        [ConfigurationKeyName("AZURE_SUBSCRIPTION_ID")]
        public string? AzureSubscriptionId { get; set; }

        [ConfigurationKeyName("AZURE_AUTOMATION_ACCOUNT_NAME")] 
        public string? AzureAutomationAccountName { get; set; }

        [ConfigurationKeyName("AZURE_AUTOMATION_RESOURCE_GROUP")]
        public string? AzureAutomationResourceGroup { get; set; }


        /// <summary>
        /// Service endpoint for the search service
        /// e.g. "https://your-search-service.search.windows.net
        /// </summary>
        [ConfigurationKeyName("AZURE_SEARCH_SERVICE_ENDPOINT")]
        public string? SearchServiceEndpoint { get; set; }

        /// <summary>
        /// Index name in the search service
        /// e.g. sample-index
        /// </summary>
        [ConfigurationKeyName("AZURE_SEARCH_INDEX_NAME")]
        public string? IndexName { get; set; }

        /// <summary>
        /// Index name in the search service
        /// e.g. sample-index
        /// </summary>
        [ConfigurationKeyName("AZURE_SEARCH_INDEX_VERSION")]
        public string? IndexVersion { get; set; }

        /// <summary>
        /// Admin API key for search service
        /// Optional, if not specified attempt to use DefaultAzureCredential
        /// </summary>
        [ConfigurationKeyName("AZURE_SEARCH_ADMIN_KEY")]
        public string? SearchAdminKey { get; set; }

        /// <summary>
        /// Search API key for search service, read-only no updates
        /// Optional, if not specified attempt to use DefaultAzureCredential
        /// </summary>
        [ConfigurationKeyName("AZURE_SEARCH_KEY")]
        public string? SearchKey { get; set; }

        /// <summary>
        /// Search API key for search service, read-only no updates
        /// Optional, if not specified attempt to use DefaultAzureCredential
        /// </summary>
        [ConfigurationKeyName("AZURE_SEARCH_API_VERSION")]
        public string? SearchApiVersion { get; set; }

        /// <summary>
        /// Azure Open AI key
        /// Optional, if not specified attempt to use DefaultAzureCredential
        /// </summary>
        [ConfigurationKeyName("AZURE_OPENAI_API_KEY")]
        public string? AzureOpenAIApiKey { get; set; }

        /// <summary>
        /// Endpoint for Azure OpenAI service
        /// </summary>
        [ConfigurationKeyName("AZURE_OPENAI_ENDPOINT")]
        public string? AzureOpenAIEndpoint { get; set; }

        /// <summary>
        /// EAzure OpenAI Model for Azure OpenAI service, in my case gpt-4o
        /// </summary>
        [ConfigurationKeyName("AZURE_OPENAI_DEPLOYMENT")]
        public string? AzureOpenAIDeployment { get; set; }

        /// <summary>
        /// EAzure OpenAI Model for Azure OpenAI service, in my case gpt-4o
        /// </summary>
        [ConfigurationKeyName("AZURE_OPENAI_API_VERSION")]
        public string? AzureOpenAIApiVersion { get; set; }

        /// <summary>
        /// Name of text embedding model deployment in Azure OpenAI service
        /// </summary>
        [ConfigurationKeyName("AZURE_OPENAI_EMBEDDING_DEPLOYMENT")]
        public string? AzureOpenAIEmbeddingDeployment { get; set; }

        /// <summary>
        /// Name of text embedding model in Azure OpenAI service
        /// </summary>
        [ConfigurationKeyName("AZURE_OPENAI_EMBEDDING_MODEL")]
        public string? AzureOpenAIEmbeddingModel { get; set; }

        /// <summary>
        /// Dimensions for embedding model
        /// </summary>
        [ConfigurationKeyName("AZURE_OPENAI_EMBEDDING_DIMENSIONS")]
        public string? AzureOpenAIEmbeddingDimensions { get; set; }

        /// <summary>
        /// Open Weather API end point
        /// e.g. e.g. https://api.openweathermap.org/data/2.5/weather?zip=
        /// </summary>
        [ConfigurationKeyName("WEATHER_ENDPOINT")]
        public string? WeatherEndpoint { get; set; }

        /// <summary>
        /// Open Weather API Key
        /// </summary>
        [ConfigurationKeyName("WEATHER_API_KEY")]
        public string? WeatherApiKey { get; set; }

        /// <summary>
        /// Service endpoint for the search service
        /// e.g. "https://your-search-service.search.windows.net
        /// </summary>
        [ConfigurationKeyName("APPLICATIONINSIGHTS_CONNECTION_STRING")]
        public string? AzureAppInsights { get; set; }

        /// <summary>
        /// Database connection string
        /// </summary>
        [ConfigurationKeyName("DatabaseConnection")]
        public string? DatabaseConnection { get; set; }

        /// <summary>
        /// Enable debug mode for detailed logging
        /// </summary>
        [ConfigurationKeyName("DEBUG_MODE")]
        public bool DebugMode { get; set; }

        /// <summary>
        /// Log level for detailed logging
        /// </summary>
        [ConfigurationKeyName("LOG_LEVEL")]
        public string? LogLevel { get; set; }

        [ConfigurationKeyName("GITHUB_TOKEN")]
        public string? GitHubToken { get; set; }

        [ConfigurationKeyName("GITHUB_OWNER")]
        public string? GitHubOwner { get; set; }

        [ConfigurationKeyName("GITHUB_REPOSITORY")]
        public string? GitHubRepository { get; set; }

        /// <summary>
        /// Validate the configuration and set applicable defaults if necessary
        /// </summary>
        /// <exception cref="ArgumentException">If any parameters are invalid</exception>
        public void Validate()
        {
            if (!Uri.TryCreate(SearchServiceEndpoint, UriKind.Absolute, out _))
            {
                throw new ArgumentException("Must specify search service endpoint", nameof(SearchServiceEndpoint));
            }

            if (string.IsNullOrEmpty(IndexName))
            {
                throw new ArgumentException("Must specify index name", nameof(IndexName));
            }

            if (!Uri.TryCreate(AzureOpenAIEndpoint, UriKind.Absolute, out _))
            {
                throw new ArgumentException("Must specify Azure OpenAI endpoint", nameof(AzureOpenAIEndpoint));
            }

            if (string.IsNullOrEmpty(AzureOpenAIEmbeddingDeployment))
            {
                AzureOpenAIEmbeddingDeployment = "text-embedding-ada-002";
            }

            if (string.IsNullOrEmpty(AzureOpenAIEmbeddingModel))
            {
                AzureOpenAIEmbeddingModel = "text-embedding-ada-002";
            }

            if (string.IsNullOrEmpty(AzureOpenAIEmbeddingDimensions))
            {
                AzureOpenAIEmbeddingDimensions = "1536";
            }

            if (string.IsNullOrEmpty(AzureSubscriptionId))
                throw new ArgumentException("Azure Subscription ID is not configured");

            if (string.IsNullOrEmpty(AzureAutomationAccountName))
                throw new ArgumentException("Azure Automation Account Name is not configured");

            if (string.IsNullOrEmpty(AzureAutomationResourceGroup))
                throw new ArgumentException("Azure Automation Resource Group is not configured");

            if (string.IsNullOrEmpty(GitHubToken))
                throw new ArgumentException("GitHub Token is not configured");

            if (string.IsNullOrEmpty(GitHubOwner))
                throw new ArgumentException("GitHub Owner is not configured");

            if (string.IsNullOrEmpty(GitHubRepository))
                throw new ArgumentException("GitHub Repository is not configured");
        }
    }
}
