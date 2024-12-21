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

namespace AzureAutomationLibrary
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

        public AzureAutomationClient(string subscriptionId, string resourceGroupName, string automationAccountName, TokenCredential credential)
        {
            if (credential == null)
                throw new ArgumentNullException(nameof(credential), "The provided credential cannot be null.");

            _subscriptionId = subscriptionId ?? throw new ArgumentNullException(nameof(subscriptionId));
            _resourceGroupName = resourceGroupName ?? throw new ArgumentNullException(nameof(resourceGroupName));
            _automationAccountName = automationAccountName ?? throw new ArgumentNullException(nameof(automationAccountName));
            _armClient = new ArmClient(credential);
        }

        public async Task<IEnumerable<string>> ListRunbooksAsync()
        {
            try
            {
                // Attempt to retrieve the subscription resource
                var subscriptionResource = _armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{_subscriptionId}"));
                ResourceGroupResource resourceGroup = await subscriptionResource.GetResourceGroupAsync(_resourceGroupName);
                AutomationAccountResource automationAccount = await resourceGroup.GetAutomationAccountAsync(_automationAccountName);
                
                var runbooks = automationAccount.GetAutomationRunbooks();
                var runbookNames = new List<string>();

                await foreach (var runbook in runbooks.GetAllAsync())
                {
                    runbookNames.Add(runbook.Data.Name);
                }
                return runbookNames;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred while listing runbooks.");
                Console.WriteLine($"Details: {ex.Message}");
                throw; // Optionally rethrow the exception to propagate it
            }
        }


        public async Task<(string JobId, string RunbookName)> StartRunbookAsync(string runbookName)
        {
            if (string.IsNullOrEmpty(runbookName))
                throw new ArgumentException("Runbook name cannot be null or empty", nameof(runbookName));

            var subscriptionResource = _armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{_subscriptionId}"));
            ResourceGroupResource resourceGroup = await subscriptionResource.GetResourceGroupAsync(_resourceGroupName);
            AutomationAccountResource automationAccount = await resourceGroup.GetAutomationAccountAsync(_automationAccountName);

            var runbooks = automationAccount.GetAutomationRunbooks();
            AutomationRunbookResource? matchingRunbook = null;

            await foreach (var rb in runbooks.GetAllAsync())
            {
                if (rb.Data.Name.Equals(runbookName, StringComparison.OrdinalIgnoreCase))
                {
                    matchingRunbook = rb;
                    break;
                }
            }

            if (matchingRunbook == null)
                throw new Exception($"Runbook '{runbookName}' not found in the list of available runbooks!");

            var jobName = Guid.NewGuid().ToString();
            var jobContent = new AutomationJobCreateOrUpdateContent { RunbookName = matchingRunbook.Data.Name };
            var jobCollection = automationAccount.GetAutomationJobs();

            ArmOperation<AutomationJobResource> createJobOperation = await jobCollection.CreateOrUpdateAsync(
                WaitUntil.Completed,
                jobName,
                jobContent);

            AutomationJobResource job = createJobOperation.Value;
            var jobId = job.Data!.Id;

            if (string.IsNullOrEmpty(job.Data!.Id)) 
                throw new InvalidOperationException("Job ID cannot be null or empty");

            return (JobId: job.Data!.Id.ToString(), RunbookName: runbookName!);
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
                AutomationAccountResource automationAccount = await resourceGroup.GetAutomationAccountAsync(_automationAccountName);

                // Get the job by its name
                var job = await automationAccount.GetAutomationJobAsync(jobName);
                return job.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the job with ID '{jobId}'.");
                Console.WriteLine($"Details: {ex.Message}");
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
                AutomationAccountResource automationAccount = await resourceGroup.GetAutomationAccountAsync(_automationAccountName);

                // Get the job by its name
                var job = await automationAccount.GetAutomationJobAsync(jobName);

                // Ensure the job has completed before fetching output
                if (job.Value.Data.Status != AutomationJobStatus.Completed)
                {
                    throw new InvalidOperationException($"Job '{jobId}' has not completed yet. Current status: {job.Value.Data.Status}");
                }

                // Fetch the job output
                var output = await job.Value.GetOutputAsync();
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving the output of job with ID '{jobId}'.");
                Console.WriteLine($"Details: {ex.Message}");
                throw;
            }
        }



        private static async Task MonitorJobStatusAsync(AutomationJobResource job)
        {
            bool completed = false;
            while (!completed)
            {
                job = await job.GetAsync(); // Refresh job state
                Console.WriteLine($"Job Status: {job.Data.Status} at {DateTimeOffset.Now:HH:mm:ss}");

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
