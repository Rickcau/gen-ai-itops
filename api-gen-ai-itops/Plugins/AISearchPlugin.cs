using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;
using Helper.AzureOpenAISearchHelper;
using Helper.AzureOpenAISearchConfiguration;
using Azure.Identity;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents;
using Runbook.Models;
using Microsoft.Extensions.Primitives;
using System.Text.Json;
using Azure.Core;

namespace Plugins
{
    public class AISearchPlugin
    {
        private readonly AISearchHelper _aiSearchHelper;
        private readonly Configuration _configuration;
        private readonly TokenCredential _credential;
        private readonly AzureOpenAIClient _azureOpenAIClient;
        private readonly SearchIndexClient _indexClient;
        private readonly SearchClient _searchClient;
        private readonly ILogger<AISearchPlugin> _logger;

        public AISearchPlugin(Configuration configuration, TokenCredential credential, ILogger<AISearchPlugin>? logger = null)
        {
            _credential = credential ?? throw new ArgumentNullException(nameof(credential));
            _configuration = configuration;
            _logger = logger ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AISearchPlugin>();

            // Create AISearchHelper with a logger
            var aiSearchHelperLogger = LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<AISearchHelper>();
            _aiSearchHelper = new AISearchHelper(aiSearchHelperLogger);

            // Initialize clients
            _azureOpenAIClient = _aiSearchHelper.InitializeOpenAIClient(configuration, _credential);
            _indexClient = _aiSearchHelper.InitializeSearchIndexClient(configuration, _credential);
            _searchClient = _indexClient.GetSearchClient(configuration.IndexName);
        }

        [KernelFunction, Description("Searches the Azure Search index for information about Runbooks that are available to perform operations with")]
        public async Task<string> SearchRunBookIndex(
            [Description("The search query that we are searching the Azure Search Index for information matching the search query.")]
            string searchQuery)
        {
            _logger.LogDebug("AISearchPlugin: Searching for runbooks matching query: {SearchQuery}", searchQuery);

            StringBuilder content = new StringBuilder();
            List<RunbookDetails> runBookDocumentList = await _aiSearchHelper.Search(_searchClient, searchQuery, semantic: true, hybrid: true);

            var options = new JsonSerializerOptions { WriteIndented = true };
            foreach (var item in runBookDocumentList)
            {
                var propertiesToInclude = new
                {
                    item.Name,
                    item.Description,
                    item.Parameters,
                    item.Score,
                    item.RerankerScore
                };
                content.Append(JsonSerializer.Serialize(propertiesToInclude, options));
            }

            return content.ToString();
        }
    }
}
