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
using AzureAutomationLibrary;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Azure.Core;

namespace Plugins
{
    public class RunbookPlugin
    {
        private readonly Configuration _configuration;
        private readonly TokenCredential _credential;
        private readonly AzureAutomationClient _azureAutomationClient;

        public RunbookPlugin(Configuration configuration, TokenCredential credential)
        {
            //_aiSearchHelper = new AISearchHelper();
            _credential = credential;
            _configuration = configuration;

            // Step 3: Initialize AzureAutomationClient with the credential
            _azureAutomationClient = new AzureAutomationClient(configuration.AzureSubscriptionId!, configuration.AzureAutomationResourceGroup!, configuration.AzureAutomationAccountName!, _credential);        

        }


        [KernelFunction, Description("Execute the Runbook in Azure Automation")]
        public async Task<string> ExecuteRunBook(
            [Description("The name of the runbook to execute")]
            string runbookName,
            [Description("Parameters to pass to the Runbook.")]
            string parameters = ""
            )
        {
            StringBuilder content = new StringBuilder();
            try
            {
                // RDC: We need to move this to ILogger
                Console.WriteLine($"Debug: ExecuteRunBook: {runbookName!}");
                Console.WriteLine($"Parameters: {(string.IsNullOrEmpty(parameters) ? "No parameters" : parameters)}");

                var (jobId, resultrunbookName) = await _azureAutomationClient.StartRunbookAsync(runbookName);

                content.Append($" JobId: {jobId}, RunBook Name: {resultrunbookName}");
                Console.WriteLine($"Runbook Plugin: Execute Runbook: JobId: {jobId}, RunBook Name: {resultrunbookName}");
            }
            catch (Azure.RequestFailedException ex)
            {
                content.Append($"I am sorry but you do not have permissions. Authorization Error: {ex.Message}. Status: {ex.Status}, Error Code: {ex.ErrorCode}");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)  // Catch other exceptions
            {
                content.Append($"Error executing runbook: {ex.Message}");
                Console.WriteLine(ex.Message);
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
