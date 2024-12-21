using Azure;
using Azure.ResourceManager.Automation;
using Azure.ResourceManager.Automation.Models;

namespace RunbookMonitor;

public record RunbookConfig
{
    public string? SubscriptionId { get; init; }
    public string? ResourceGroupName { get; init; }
    public string? AutomationAccountName { get; init; }
    public string? RunbookName { get; init; }
}

public static class RunbookHelper
{
    public static async Task MonitorJobStatusAsync(AutomationJobResource job)
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

    public static async Task GetJobOutputAsync(AutomationJobResource job)
    {
        Console.WriteLine("\nJob Output:");
        Console.WriteLine("====================");

        Response<string> output = await job.GetOutputAsync();
        Console.WriteLine(output.Value);
    }
}
