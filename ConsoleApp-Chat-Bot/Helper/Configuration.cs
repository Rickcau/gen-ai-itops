using Microsoft.Extensions.Configuration;

namespace AzureOpenAISearchConfiguration
{
    /// <summary>
    /// Configuration for the demo app. Can be filled in from local.settings.json or environment variables
    /// </summary>
    public class Configuration
    {

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
        /// Service endpoint for the search service
        /// e.g. "https://your-search-service.search.windows.net
        /// </summary>
        [ConfigurationKeyName("APPLICATIONINSIGHTS_CONNECTION_STRING")]
        public string? AzureAppInsights { get; set; }

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
        }
    }
}
