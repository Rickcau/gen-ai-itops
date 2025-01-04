using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Octokit;
using Helper.AzureOpenAISearchConfiguration;

namespace Plugins
{
    public class GitHubWorkflowPlugin
    {
        private readonly GitHubClient _client;
        private readonly string _owner;
        private readonly string _repo;
        private readonly ILogger<GitHubWorkflowPlugin>? _logger;

        public GitHubWorkflowPlugin(Configuration configuration, ILogger<GitHubWorkflowPlugin>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentException.ThrowIfNullOrEmpty(configuration.GitHubToken, nameof(configuration.GitHubToken));
            ArgumentException.ThrowIfNullOrEmpty(configuration.GitHubOwner, nameof(configuration.GitHubOwner));
            ArgumentException.ThrowIfNullOrEmpty(configuration.GitHubRepository, nameof(configuration.GitHubRepository));

            _client = new GitHubClient(new ProductHeaderValue("gen-ai-itops"))
            {
                Credentials = new Credentials(configuration.GitHubToken)
            };
            _owner = configuration.GitHubOwner;
            _repo = configuration.GitHubRepository;
            _logger = logger;
        }

        [KernelFunction, Description("List all available workflows in the repository")]
        public async Task<string> ListWorkflows()
        {
            try
            {
                _logger?.LogInformation("Attempting to list workflows for {Owner}/{Repo}", _owner, _repo);
                _logger?.LogDebug("GitHub client configured with token starting with: {TokenPrefix}", 
                    _client.Credentials?.GetToken()?.Substring(0, 4) + "...");

                var workflows = await _client.Actions.Workflows.List(_owner, _repo);
                
                if (!workflows.Workflows.Any())
                {
                    _logger?.LogInformation("No workflows found in repository");
                    return "No workflows found in the repository.";
                }

                _logger?.LogInformation("Found {Count} workflows", workflows.Workflows.Count);

                var result = new StringBuilder();
                result.AppendLine($"Available workflows in {_owner}/{_repo}:");
                
                foreach (var workflow in workflows.Workflows)
                {
                    result.AppendLine($"- Name: {workflow.Name}");
                    result.AppendLine($"  ID: {workflow.Id}");
                    result.AppendLine($"  State: {workflow.State}");
                    result.AppendLine($"  Path: {workflow.Path}");
                    result.AppendLine();
                }

                return result.ToString().TrimEnd();
            }
            catch (AuthorizationException ex)
            {
                _logger?.LogError(ex, "GitHub authorization failed. Please check your token has correct permissions");
                throw new Exception("GitHub authorization failed. Please check your Personal Access Token has the required permissions (repo and workflow).", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error listing workflows");
                throw;
            }
        }

        [KernelFunction, Description("Trigger a workflow by its name or ID")]
        public async Task<string> TriggerWorkflow(string workflowNameOrId)
        {
            try
            {
                var workflows = await _client.Actions.Workflows.List(_owner, _repo);
                var workflow = workflows.Workflows.FirstOrDefault(w => 
                    w.Name.Equals(workflowNameOrId, StringComparison.OrdinalIgnoreCase) || 
                    w.Id.ToString() == workflowNameOrId);

                if (workflow == null)
                {
                    return $"Workflow '{workflowNameOrId}' not found.";
                }

                // Create a workflow dispatch with default branch
                var createDispatch = new CreateWorkflowDispatch("main");
                await _client.Actions.Workflows.CreateDispatch(_owner, _repo, workflow.Id, createDispatch);

                // Get the most recent run for this workflow
                var runs = await _client.Actions.Workflows.Runs.List(_owner, _repo);
                var latestRun = runs.WorkflowRuns
                    .Where(r => r.WorkflowId == workflow.Id)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefault();

                if (latestRun == null)
                {
                    return $"Workflow '{workflow.Name}' triggered, but no run information is available yet.";
                }

                return $"""
                    Workflow '{workflow.Name}' has been triggered successfully.
                    Run ID: {latestRun.Id}
                    Status: {latestRun.Status}
                    URL: {latestRun.HtmlUrl}
                    """;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error triggering workflow");
                throw;
            }
        }

        [KernelFunction, Description("Check the status of a workflow run")]
        public async Task<string> CheckWorkflowStatus(string runId)
        {
            try
            {
                if (!long.TryParse(runId, out var runIdLong))
                {
                    return "Invalid run ID. Please provide a valid numeric ID.";
                }

                var run = await _client.Actions.Workflows.Runs.Get(_owner, _repo, runIdLong);
                
                var result = new StringBuilder();
                result.AppendLine($"Status: {run.Status}");
                result.AppendLine($"Conclusion: {run.Conclusion ?? "in progress"}");
                result.AppendLine($"Started: {run.CreatedAt:u}");
                result.AppendLine($"Last Updated: {run.UpdatedAt:u}");
                result.AppendLine($"URL: {run.HtmlUrl}");
                result.AppendLine();

                // Get jobs for this specific run
                var jobsResponse = await _client.Connection.Get<WorkflowJobsResponse>(
                    new Uri($"repos/{_owner}/{_repo}/actions/runs/{runIdLong}/jobs"),
                    new Dictionary<string, string>());

                if (jobsResponse.Body.Jobs.Any())
                {
                    result.AppendLine("Jobs:");
                    foreach (var job in jobsResponse.Body.Jobs)
                    {
                        var status = job.Conclusion != null ? job.Conclusion.ToString() : job.Status.ToString();
                        result.AppendLine($"- {job.Name}: {status} ({job.StartedAt:HH:mm:ss} to {job.CompletedAt?.ToString("HH:mm:ss") ?? "ongoing"})");
                    }
                }

                return result.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking workflow status");
                throw;
            }
        }

        private class WorkflowJobsResponse
        {
            public int TotalCount { get; set; }
            public IReadOnlyList<WorkflowJob> Jobs { get; set; } = Array.Empty<WorkflowJob>();
        }
    }
} 