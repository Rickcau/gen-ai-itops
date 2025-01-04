using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Automation;
using Azure.ResourceManager.Automation.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;

namespace Helper.AzureAutomationLibrary
{
    internal class RunbookConfig
    {
        public string? SubscriptionId { get; set; }
        public string? ResourceGroupName { get; set; }
        public string? AutomationAccountName { get; set; }
        public string? RunbookName { get; set; }
    }

    public class AzureAutomationClient
    {
        private readonly string _subscriptionId;
        private readonly string _resourceGroupName;
        private readonly string _automationAccountName;
        private readonly ArmClient _armClient;
        private readonly ILogger<AzureAutomationClient> _logger;

        public AzureAutomationClient(string subscriptionId, string resourceGroupName, string automationAccountName, TokenCredential credential, ILogger<AzureAutomationClient>? logger = null)
        {
            if (credential == null)
                throw new ArgumentNullException(nameof(credential), "The provided credential cannot be null.");

            _subscriptionId = subscriptionId;
            _resourceGroupName = resourceGroupName;
            _automationAccountName = automationAccountName;
            _logger = logger ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AzureAutomationClient>();

            _armClient = new ArmClient(credential);
            _logger.LogInformation("Initialized ArmClient with subscription: {SubscriptionId}, ResourceGroup: {ResourceGroup}", subscriptionId, resourceGroupName);
        }

        public async Task<List<string>> ListRunbooksAsync()
        {
            try
            {
                _logger.LogInformation("Starting ListRunbooksAsync for subscription: {SubscriptionId}", _subscriptionId);
                var runbookNames = new List<string>();
                var subscriptionResource = _armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{_subscriptionId}"));
                _logger.LogInformation("Got subscription resource, attempting to get resource group: {ResourceGroup}", _resourceGroupName);
                
                ResourceGroupResource resourceGroup = await subscriptionResource.GetResourceGroupAsync(_resourceGroupName);
                _logger.LogInformation("Successfully got resource group");
                var automationAccount = await resourceGroup.GetAutomationAccountAsync(_automationAccountName);
                var runbooks = automationAccount.Value.GetAutomationRunbooks();

                await foreach (var runbook in runbooks.GetAllAsync())
                {
                    runbookNames.Add(runbook.Data.Name);
                }
                return runbookNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while listing runbooks");
                throw;
            }
        }

        public async Task<(string JobId, string RunbookName)> StartRunbookAsync(string runbookName, string parameters = "")
        {
            if (string.IsNullOrEmpty(runbookName))
                throw new ArgumentException("Runbook name cannot be null or empty", nameof(runbookName));
            // RDC: Added below for testing, this need to be dealt with
            try
            {
                var subscriptionResource = _armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{_subscriptionId}"));
                ResourceGroupResource resourceGroup = await subscriptionResource.GetResourceGroupAsync(_resourceGroupName);
                var automationAccount = (await resourceGroup.GetAutomationAccountAsync(_automationAccountName)).Value;
                var runbook = automationAccount.GetAutomationRunbook(runbookName);

                var jobContent = new AutomationJobCreateOrUpdateContent { RunbookName = runbookName };

                // Parse and add parameters if provided
                if (!string.IsNullOrEmpty(parameters))
                {
                    try
                    {
                        var parametersDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(parameters);
                        if (parametersDictionary != null)
                        {
                            foreach (var param in parametersDictionary)
                            {
                                jobContent.Parameters.Add(param.Key, param.Value);
                                _logger.LogDebug("Added parameter {Key}: {Value}", param.Key, param.Value);
                            }
                        }
                    }
                    catch (System.Text.Json.JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to parse parameters JSON string: {Parameters}", parameters);
                        throw new ArgumentException("Parameters must be a valid JSON string in the format {\"key\":\"value\"}", nameof(parameters), ex);
                    }
                }

                var jobCollection = automationAccount.GetAutomationJobs();
                var job = await jobCollection.CreateOrUpdateAsync(WaitUntil.Completed, Guid.NewGuid().ToString(), jobContent);

                var jobId = job.Value.Data.JobId.ToString() ?? throw new InvalidOperationException("Job ID cannot be null");
                _logger.LogDebug("Started runbook job. JobId: {JobId}, RunbookName: {RunbookName}", jobId, runbookName);

                return (jobId, runbookName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ("12324", "test");
        }

        public async Task<AutomationJobResource> GetJobStatusByIdAsync(string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
                throw new ArgumentException("Job ID cannot be null or empty.", nameof(jobId));

            try
            {
                // Extract the job name from the full jobId
                var jobName = new ResourceIdentifier(jobId).Name;

                // Retrieve the Automation Account
                var subscriptionResource = _armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{_subscriptionId}"));
                ResourceGroupResource resourceGroup = await subscriptionResource.GetResourceGroupAsync(_resourceGroupName);
                var automationAccount = (await resourceGroup.GetAutomationAccountAsync(_automationAccountName)).Value;

                // Get the job by its name
                var job = await automationAccount.GetAutomationJobAsync(jobName);
                return job.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the job with ID '{JobId}'", jobId);
                throw;
            }
        }

        public async Task<string> GetJobOutputByIdAsync(string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
                throw new ArgumentException("Job ID cannot be null or empty.", nameof(jobId));

            try
            {
                // Extract the job name from the full jobId
                var jobName = new ResourceIdentifier(jobId).Name;

                // Retrieve the Automation Account
                var subscriptionResource = _armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{_subscriptionId}"));
                ResourceGroupResource resourceGroup = await subscriptionResource.GetResourceGroupAsync(_resourceGroupName);
                var automationAccount = (await resourceGroup.GetAutomationAccountAsync(_automationAccountName)).Value;

                // Get the job by its name
                var job = await automationAccount.GetAutomationJobAsync(jobName);
                var jobValue = job.Value;

                // Ensure the job has completed before fetching output
                if (jobValue.Data.Status != AutomationJobStatus.Completed)
                {
                    throw new InvalidOperationException($"Job '{jobId}' has not completed yet. Current status: {jobValue.Data.Status}");
                }

                // Fetch the job output
                var output = await jobValue.GetOutputAsync();
                return output ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the output of job with ID '{JobId}'", jobId);
                throw;
            }
        }

        private async Task MonitorJobStatusAsync(AutomationJobResource job)
        {
            bool completed = false;
            while (!completed)
            {
                job = await job.GetAsync(); // Refresh job state
                _logger.LogDebug("Job Status: {Status} at {Time}", job.Data.Status, DateTimeOffset.Now);

                completed = job.Data.Status == AutomationJobStatus.Completed ||
                    job.Data.Status == AutomationJobStatus.Failed ||
                    job.Data.Status == AutomationJobStatus.Suspended ||
                    job.Data.Status == AutomationJobStatus.Stopped;

                if (!completed)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
        }
    }
}

