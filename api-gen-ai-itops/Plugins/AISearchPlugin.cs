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
using api_gen_ai_itops.Models;

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
        private readonly string _indexVersion;
        private readonly ILogger<AISearchPlugin> _logger;

        public AISearchPlugin(Configuration configuration, TokenCredential credential, ILogger<AISearchPlugin>? logger = null)
        {
            _credential = credential ?? throw new ArgumentNullException(nameof(credential));
            _configuration = configuration;
            _logger = logger ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AISearchPlugin>();
            _indexVersion = configuration.IndexVersion ?? "V1";

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

            if (_indexVersion =="V1")
            {
                _logger.LogDebug("aiSearchHelper.SearchV1 is being used!");
                // TDB need to fix this to use the V2 Index but for the demo we are fine.   
                List<RunbookDetails> runBookDocumentList = await _aiSearchHelper.SearchV1(_searchClient, searchQuery, semantic: true, hybrid: true);

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
            // If not V1 Index then use the V2 Index
            _logger.LogDebug("aiSearchHelper.SearchV2 is being used!");
            // TDB need to fix this to use the V2 Index but for the demo we are fine.   
            List<Capability> capablitiesList = await _aiSearchHelper.SearchCapabilities(_searchClient, searchQuery, semantic: true, hybrid: true);

            var optionsV2 = new JsonSerializerOptions { WriteIndented = true };
            foreach (var item in capablitiesList)
            {
                var propertiesToInclude = new
                {
                    item.Name,
                    item.Description,
                    item.Parameters,
                    item.CapabilityType,
                    item.ExecutionMethod
                };
                content.Append(JsonSerializer.Serialize(propertiesToInclude, optionsV2));
            }

            return content.ToString();



            // below is working
            //_logger.LogDebug("aiSearchHelper.SearchV1 is being used!");
            //// TDB need to fix this to use the V2 Index but for the demo we are fine.   
            //List<RunbookDetails> runBookDocumentList = await _aiSearchHelper.SearchV1(_searchClient, searchQuery, semantic: true, hybrid: true);

            //var options = new JsonSerializerOptions { WriteIndented = true };
            //foreach (var item in runBookDocumentList)
            //{
            //    var propertiesToInclude = new
            //    {
            //        item.Name,
            //        item.Description,
            //        item.Parameters,
            //        item.Score,
            //        item.RerankerScore
            //    };
            //    content.Append(JsonSerializer.Serialize(propertiesToInclude, options));
            //}

            return content.ToString();
        }
    }
}
