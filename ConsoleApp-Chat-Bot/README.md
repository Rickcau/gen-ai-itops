# IT Operations Chat Bot

A conversational AI assistant that helps users interact with Azure Automation runbooks and perform IT operations tasks.

## Features

- Natural language interaction with IT operations
- Integration with Azure Automation runbooks
- Semantic search capabilities for finding relevant operations
- Dynamic job status checking
- Configurable logging levels
- Azure Application Insights telemetry integration

## Components

### Core Components

- **Assistant**: The main orchestrator that evaluates user requests and routes them appropriately
- **IT Specialist**: Handles the execution of IT operations and provides status updates
- **Azure Automation Integration**: Executes and monitors runbook jobs
- **AI Search**: Finds relevant runbooks based on user queries

### Key Files

- `Program.cs`: Main application logic and chat handling
- `Configuration.cs`: Application configuration management
- `AzureAutomationClient.cs`: Azure Automation interaction
- `AISearchHelper.cs`: Azure Cognitive Search integration
- `RunbookPlugin.cs`: Runbook execution and status checking
- `AISearchPlugin.cs`: Semantic search functionality
- `EchoPlugin.cs`: Debug logging support

## Configuration

The application uses a `local.settings.json` file for configuration. Here are all the required settings:

```json
{
  "AZURE_SUBSCRIPTION_ID": "Your Azure subscription ID",
  "AZURE_AUTOMATION_ACCOUNT_NAME": "Your Automation account name",
  "AZURE_AUTOMATION_RESOURCE_GROUP": "Your resource group name",
  "AZURE_SEARCH_SERVICE_ENDPOINT": "Your Azure Cognitive Search endpoint",
  "AZURE_SEARCH_ADMIN_KEY": "Your search service admin key",
  "AZURE_SEARCH_KEY": "Your search service query key",
  "AZURE_SEARCH_INDEX_NAME": "Your search index name",
  "AZURE_SEARCH_API_VERSION": "Search API version (e.g., 2020-06-30)",
  "AZURE_OPENAI_ENDPOINT": "Your Azure OpenAI endpoint",
  "AZURE_OPENAI_API_KEY": "Your Azure OpenAI API key",
  "AZURE_OPENAI_DEPLOYMENT": "Your OpenAI model deployment name",
  "AZURE_OPENAI_API_VERSION": "OpenAI API version (e.g., 2024-08-01-preview)",
  "AZURE_OPENAI_EMBEDDING_DEPLOYMENT": "Your embedding model deployment name",
  "AZURE_OPENAI_EMBEDDING_MODEL": "Your embedding model name (e.g., text-embedding-ada-002)",
  "AZURE_OPENAI_EMBEDDING_DIMENSIONS": "Embedding dimensions (e.g., 1536)",
  "APPLICATIONINSIGHTS_CONNECTION_STRING": "Your Application Insights connection string",
  "LOG_LEVEL": "Logging level (DEBUG, INFORMATION, WARNING, ERROR)"
}
```

## Usage

1. Configure all the required settings in `local.settings.json`
2. Run the application
3. Interact with the assistant using natural language
4. Type 'exit' to quit

### Example Interactions

```
User > List all VMs
Assistant: I am forwarding your request to the IT Specialist.
IT Specialist: I'll help you list the virtual machines...

User > Check job status
IT Specialist: The job status is: Completed
Output: [VM inventory details...]
```

## Logging

- Set `LOG_LEVEL` to control logging verbosity (DEBUG, INFORMATION, WARNING, ERROR)
- Logs are sent to Application Insights using the provided connection string
- Debug mode provides additional console output

## Error Handling

The application includes robust error handling for:
- Azure service throttling
- Authentication issues
- Network connectivity problems
- Invalid requests
- Service availability issues

## Dependencies

- .NET 8.0
- Azure.Identity
- Azure.AI.OpenAI
- Azure.Search.Documents
- Microsoft.SemanticKernel
- OpenTelemetry for logging

## Security

- Uses Azure AD authentication
- Supports managed identities
- Configurable access control through Azure RBAC 