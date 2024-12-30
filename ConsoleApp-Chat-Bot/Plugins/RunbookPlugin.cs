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
using Azure.Core;

namespace Plugins
{
    public class RunbookPlugin
    {
        private readonly Configuration _configuration;
        private readonly TokenCredential _credential;
        private readonly AzureAutomationClient _azureAutomationClient;
        private readonly ILogger<RunbookPlugin> _logger;

        public RunbookPlugin(Configuration configuration, TokenCredential credential, ILogger<RunbookPlugin>? logger = null)
        {
            _credential = credential;
            _configuration = configuration;
            _logger = logger ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<RunbookPlugin>();

            // Create a logger for AzureAutomationClient
            var automationLogger = LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<AzureAutomationClient>();

            // Initialize AzureAutomationClient with the credential and logger
            _azureAutomationClient = new AzureAutomationClient(
                configuration.AzureSubscriptionId!, 
                configuration.AzureAutomationResourceGroup!, 
                configuration.AzureAutomationAccountName!, 
                _credential,
                automationLogger);        
        }

        [KernelFunction, Description("Execute the Runbook in Azure Automation")]
        public async Task<string> ExecuteRunBook(
            [Description("The name of the runbook to execute")]
            string runbookName,
            [Description("Parameters to pass to the Runbook in JSON format, e.g. {\"VMName\":\"myvm\",\"ResourceGroupName\":\"mygroup\"}")]
            string parameters = ""
            )
        {
            StringBuilder content = new StringBuilder();
            try
            {
                _logger.LogDebug("Executing runbook: {RunbookName}", runbookName);
                _logger.LogDebug("Parameters: {Parameters}", string.IsNullOrEmpty(parameters) ? "No parameters" : parameters);

                var (jobId, resultrunbookName) = await _azureAutomationClient.StartRunbookAsync(runbookName, parameters);

                var resultMessage = $"The runbook \"{resultrunbookName}\" has been successfully executed. You can track its progress with Job ID: `{jobId}`.";
                content.AppendLine(resultMessage);
                content.AppendLine("\nIf you would like to check the status of this job, please let me know.");
                _logger.LogDebug("Runbook execution started: {ResultMessage}", resultMessage);
            }
            catch (Azure.RequestFailedException ex)
            {
                var errorMessage = $"Authorization Error: {ex.Message}. Status: {ex.Status}, Error Code: {ex.ErrorCode}";
                content.Append($"I am sorry but you do not have permissions. {errorMessage}");
                _logger.LogError(ex, "Authorization error executing runbook: {ErrorMessage}", errorMessage);
            }
            catch (ArgumentException ex)
            {
                var errorMessage = $"Parameter Error: {ex.Message}";
                content.Append(errorMessage);
                _logger.LogError(ex, "Parameter error executing runbook: {ErrorMessage}", errorMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error executing runbook: {ex.Message}";
                content.Append(errorMessage);
                _logger.LogError(ex, "Unexpected error executing runbook: {ErrorMessage}", errorMessage);
            }
            return content.ToString();
        }

        [KernelFunction, Description("Check the status of a runbook job")]
        public async Task<string> CheckJobStatus(
            [Description("The job ID of the runbook execution to check")]
            string jobId)
        {
            StringBuilder content = new StringBuilder();
            try
            {
                _logger.LogDebug("Checking status for job: {JobId}", jobId);

                // Construct the full resource ID for the job
                var fullJobId = $"/subscriptions/{_configuration.AzureSubscriptionId}/resourceGroups/{_configuration.AzureAutomationResourceGroup}/providers/Microsoft.Automation/automationAccounts/{_configuration.AzureAutomationAccountName}/jobs/{jobId}";
                _logger.LogDebug("Full job resource ID: {FullJobId}", fullJobId);

                var job = await _azureAutomationClient.GetJobStatusByIdAsync(fullJobId);
                var status = job.Data.Status.ToString();
                
                content.AppendLine($"Status: {status}");
                
                if (job.Data.Status == Azure.ResourceManager.Automation.Models.AutomationJobStatus.Completed)
                {
                    try
                    {
                        var output = await _azureAutomationClient.GetJobOutputByIdAsync(fullJobId);
                        if (!string.IsNullOrEmpty(output))
                        {
                            content.AppendLine("\nOutput:");
                            content.AppendLine(output);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error retrieving job output");
                        content.AppendLine("\nNote: Could not retrieve job output.");
                    }
                }
                else if (job.Data.Status == Azure.ResourceManager.Automation.Models.AutomationJobStatus.Failed)
                {
                    content.AppendLine("\nThe job has failed. Please check the Azure portal for more details.");
                }
                else if (job.Data.Status == Azure.ResourceManager.Automation.Models.AutomationJobStatus.Running)
                {
                    content.AppendLine("\nThe job is still running. Please check back later for the results.");
                }
                
                _logger.LogDebug("Retrieved job status: {Status}", status);
            }
            catch (Azure.RequestFailedException ex)
            {
                var errorMessage = $"Authorization Error: {ex.Message}. Status: {ex.Status}, Error Code: {ex.ErrorCode}";
                content.AppendLine($"I am sorry but you do not have permissions. {errorMessage}");
                _logger.LogError(ex, "Authorization error checking job status: {ErrorMessage}", errorMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error checking job status: {ex.Message}";
                content.AppendLine(errorMessage);
                _logger.LogError(ex, "Unexpected error checking job status: {ErrorMessage}", errorMessage);
            }
            return content.ToString();
        }
    }
}
