using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;
using AzureOpenAISearchHelper;
using AzureOpenAISearchConfiguration;
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

        public AISearchPlugin(Configuration configuration, TokenCredential credential)
        {
            //_aiSearchHelper = new AISearchHelper();
            _credential = credential;
            _configuration = configuration;

            _aiSearchHelper = new AISearchHelper();

            // Initialize clients
            _azureOpenAIClient = _aiSearchHelper.InitializeOpenAIClient(configuration, credential);
            _indexClient = _aiSearchHelper.InitializeSearchIndexClient(configuration, credential);
            _searchClient = _indexClient.GetSearchClient(configuration.IndexName);

        }


        [KernelFunction, Description("Searches the Azure Search index for information about Runbooks that are available to perform operations with")]
        public async Task<string> SearchRunBookIndex([Description("The search query that we are searching the Azure Search Index for information matching the search query.")]
            string searchQuery)
        {
            // RDC: We need to move this to ILogger
            Console.WriteLine($"Debug: AISearchPlugin: I can list VMs for you but I need to know the name of the resouce group... this is just for testing");
            Console.WriteLine($"Performing search for: {searchQuery}");

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


        //[KernelFunction, Description("Searches the Azure Search index for information about the Tesla based on the Tesla user manual.")]
        //public async Task<string> SearchManualsIndex(
        //   [Description("The search query that we are searching the Azure Search Tesla Manual Index for information matching the search query.")]
        //    string query,
        //   ILogger? logger = null)
        //{
        //    StringBuilder content = new StringBuilder();

        //    Console.WriteLine("Searching Manuals Index...\n");

        //    List<UserManualDetails> userManualDetailsList = await SemanticHybridSearch(query, logger);

        //    foreach (var item in userManualDetailsList)
        //    {
        //        //Console.WriteLine(item.Chunk);
        //        content.Append(item.Chunk);
        //    }

        //    return content.ToString();
        //}
    }
}
