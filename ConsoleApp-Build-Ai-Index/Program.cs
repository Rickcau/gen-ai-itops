using System;
using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Embeddings;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using AzureOpenAISearchConfiguration;
using AzureOpenAISearchHelper;
using Microsoft.Extensions.Configuration;
using Runbook.Models;

//// RDC: 12/24/2024 - Working well.
// Description:
// Azure OpenAI Search Helper

// Usage:
//   ConsoleApp-Build-Ai-Index [options]

// Options:
//  --create           Create the search index
//  --load             Generate and load documents into the index
//  --search <search>  Perform a search with the specified query
//  --delete           Delete the search index
//  --version          Show version information
//  -?, -h, --help     Show help and usage information

// Define command-line options
var createOption = new Option<bool>(
    "--create",
    "Create the search index");

var loadOption = new Option<bool>(
    "--load",
    "Generate and load documents into the index");

var searchOption = new Option<string>(
    "--search",
    "Perform a search with the specified query");

var deleteOption = new Option<bool>(
    "--delete",
    "Delete the search index");

// Create root command and add options
var rootCommand = new RootCommand("Azure OpenAI Search Helper");
rootCommand.AddOption(createOption);
rootCommand.AddOption(loadOption);
rootCommand.AddOption(searchOption);
rootCommand.AddOption(deleteOption);


rootCommand.SetHandler(
    async (bool create, bool load, string searchQuery, bool delete) =>
    {
        try
        {
            // Create the Configuration Object and AISearchHelper

            var configuration = new Configuration();
            var aiSearchHelper = new AISearchHelper();
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json")
                .Build()
                .Bind(configuration);

            configuration.Validate();

            // Initialize clients
            var defaultCredential = new DefaultAzureCredential();
            var azureOpenAIClient = aiSearchHelper.InitializeOpenAIClient(configuration, defaultCredential);
            var indexClient = aiSearchHelper.InitializeSearchIndexClient(configuration, defaultCredential);
            var searchClient = indexClient.GetSearchClient(configuration.IndexName);

            // Handle delete index
            if (delete)
            {
                Console.WriteLine("Deleting index...");
                await aiSearchHelper.DeleteIndexAsync(configuration, indexClient);
            }

            // Handle create index
            if (create)
            {
                Console.WriteLine("Creating index...");
                await aiSearchHelper.SetupIndexAsync(configuration, indexClient);

                // If we're also loading documents, add a delay
                if (load)
                {
                    Console.WriteLine("\nWaiting for index to be ready...");
                    var startTime = DateTime.Now;
                    while ((DateTime.Now - startTime).TotalSeconds < 5)
                    {
                        Console.Write(".");
                        Thread.Sleep(200);
                    }
                    Console.WriteLine(); // Add newline at the end
                }
            }

            // Handle document loading
            if (load)
            {
                Console.WriteLine("Generating and loading documents...");
                await aiSearchHelper.GenerateAndSaveRunBookDocumentsAsync(
                    configuration,
                    azureOpenAIClient,
                    searchClient,
                    $@"C:\temp\runbooks.json");
            }

           

            // Handle search
            if (!string.IsNullOrEmpty(searchQuery))
            {
                Console.WriteLine($"Performing search for: {searchQuery}");
                List<RunbookDetails> runBookDocuments = await aiSearchHelper.Search(searchClient, searchQuery, semantic: true, hybrid: true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.ToString());
        }
    },
createOption, loadOption, searchOption, deleteOption);

// Run the command
await rootCommand.InvokeAsync(args);