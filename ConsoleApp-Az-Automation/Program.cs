using System;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Automation;
using Azure.ResourceManager.Automation.Models;
using System.Configuration;
using Azure.ResourceManager.Resources;
using RunbookMonitor;
using AzureAutomationLibrary;
// RDC: This code is working fine as of 12/19/2024.
var config = new RunbookConfig
{
    SubscriptionId = ConfigurationManager.AppSettings["AzureSubscriptionId"]
        ?? throw new ConfigurationErrorsException("AzureSubscriptionId not found in config"),
    ResourceGroupName = ConfigurationManager.AppSettings["AzureAutomationResourceGroupName"]
        ?? throw new ConfigurationErrorsException("AzureAutomationResourceGroupName not found in config"),
    AutomationAccountName = ConfigurationManager.AppSettings["AzureAutomationAccountName"]
        ?? throw new ConfigurationErrorsException("AzureAutomationAccountName not found in config"),
    RunbookName = ConfigurationManager.AppSettings["AzureRunbookName"]
        ?? throw new ConfigurationErrorsException("AzureRunbookName not found in config")
};

try
{
    // Step 1: Prompt the user to authenticate
    Console.WriteLine("A browser window will open for authentication. Please select your account...");
    var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
    {
        TokenCachePersistenceOptions = new TokenCachePersistenceOptions()
    });

    // Validate the credential by acquiring a token (one-time prompt)
    await credential.GetTokenAsync(new TokenRequestContext(new[] { "https://management.azure.com/.default" }));
    Console.WriteLine("Authentication successful!");

    // Step 2: Configuration (replace with your ConfigurationManager or JSON if needed)
    var subscriptionId = config.SubscriptionId; // Replace with actual subscription ID
    var resourceGroupName = config.ResourceGroupName;
    var automationAccountName = config.AutomationAccountName;

    // Step 3: Initialize AzureAutomationClient with the credential
    var automationClient = new AzureAutomationClient(subscriptionId, resourceGroupName, automationAccountName, credential);

    // Step 4: List available runbooks
    Console.WriteLine("\nListing available runbooks...");
    var runbooks = await automationClient.ListRunbooksAsync();
    foreach (var runbook in runbooks)
    {
        Console.WriteLine($"- {runbook}");
    }

    // Step 5: Start a specific runbook
    var runbookToStart = "test-vm-permissions"; // Replace with the runbook name
    var (jobId, runbookName) = await automationClient.StartRunbookAsync(runbookToStart);
    Console.WriteLine($"\nStarted runbook '{runbookName}'");

    // Step 6: Monitor job status
    Console.WriteLine("\nRetrieving job status...");
    bool completed = false;
    AutomationJobResource? job = null;
    while (!completed)
    {
        job = await automationClient.GetJobStatusByIdAsync(jobId);
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

    // Fetch job output if completed
    if (job!.Data.Status == AutomationJobStatus.Completed)
    {
        var output = await automationClient.GetJobOutputByIdAsync(jobId);
        Console.WriteLine("Job Output:");
        Console.WriteLine(output);
    }
    else
    {
        Console.WriteLine("Job has not completed yet. Please try again later.");
    }



    var jobResource = await automationClient.GetJobStatusByIdAsync(jobId);
    Console.WriteLine($"Runbook: '{runbookName}' status: {jobResource.Data.Status}");
    
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
}