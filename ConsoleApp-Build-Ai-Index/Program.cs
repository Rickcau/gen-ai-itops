//using System;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using Azure;
//using Azure.Identity;
//using Azure.AI.OpenAI;  // for EmbeddingsOptions
//using Azure.Search.Documents;
//using Azure.Search.Documents.Indexes;
//using Azure.Search.Documents.Indexes.Models;
//using Azure.Search.Documents.Models;
//using OpenAI;
//using OpenAI.Embeddings;
//using AzureOpenAISearchConfiguration;
//using Microsoft.Extensions.Configuration;
//using AzureOpenAISearchHelper;
//using System.Runtime.CompilerServices;
//using System.CommandLine;

//// RDC: 12/24/2024 - Working well.
//// If you plan to run this from the UI via Start Debug, you will need to create a debug provide and make sure to pass
//// in the two arguments.
////
//// Usage: appname <buildindex> <search>
//// Example: appname true false   <- this will result in only the index being built


//// First lets check to make sure some arguments were passed in before we do anything else
//if (args.Length != 2)
//{
//    Console.WriteLine("Error: Please provide exactly two arguments: buildindex and search.");
//    Console.WriteLine("Usage: appname <buildindex> <search>");
//    Console.WriteLine("Example: appname true false");
//    return;
//}

//// Create the Configuration Object - a helper class to help us with the environment variables needed.
//// Create our aiSearchHelper object which will be used to create the index and perform searchers
//var configuration = new Configuration();
//var aiSearchHelper = new AISearchHelper();
//new ConfigurationBuilder()
//              .SetBasePath(Directory.GetCurrentDirectory())
//              .AddEnvironmentVariables()
//              .AddJsonFile("local.settings.json")
//              .Build()
//              .Bind(configuration);

//configuration.Validate();

//// Let's aquire credentials using DefaultAzureCredential() then lets create instances of everything we will need.
//var defaultCredential = new DefaultAzureCredential();
//var azureOpenAIClient = aiSearchHelper.InitializeOpenAIClient(configuration, defaultCredential);
//var indexClient = aiSearchHelper.InitializeSearchIndexClient(configuration, defaultCredential);
//var searchClient = indexClient.GetSearchClient(configuration.IndexName);
//// await aiSearchHelper.SetupIndexAsync(configuration, indexClient);




//// Parse the arguments
//string buildIndexArg = args[0].ToLower();
//string searchArg = args[1].ToLower();

//Console.WriteLine($"Build Index = {buildIndexArg}");
//Console.WriteLine($"Search = {searchArg}");

//if (Boolean.Parse(buildIndexArg) && !Boolean.Parse(searchArg))
//{
//    Console.WriteLine("Building the index only...");
//    try
//    {
//        await aiSearchHelper.SetupIndexAsync(configuration, indexClient);
//        await aiSearchHelper.GenerateAndSaveRunBookDocumentsAsync(configuration, azureOpenAIClient, searchClient, $@"C:\temp\runbooks.json");

//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine(ex.ToString());
//    }
//}
//else if (Boolean.Parse(searchArg) && !Boolean.Parse(buildIndexArg))
//{
//    Console.WriteLine("Performing a Search only...");
//    try
//    {
//        await aiSearchHelper.Search(searchClient, "Start a VM for me", semantic: true, hybrid: true);

//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine(ex.ToString());
//    }
//}
//else if (Boolean.Parse(searchArg) && Boolean.Parse(buildIndexArg))
//{
//    Console.WriteLine("Building Index then... Performing a Search...");
//    try
//    {
//        await aiSearchHelper.SetupIndexAsync(configuration, indexClient);
//        await aiSearchHelper.GenerateAndSaveRunBookDocumentsAsync(configuration, azureOpenAIClient, searchClient, $@"C:\temp\runbooks.json");
//        Console.WriteLine("\nLet's give the index time to finish it's background work.");
//        var startTime = DateTime.Now;
//        while ((DateTime.Now - startTime).TotalSeconds < 5)
//        {
//            Console.Write(".");
//            Thread.Sleep(200); // Add small delay between dots to avoid flooding
//        }
//        Console.WriteLine(); // Add newline at the end
//        await aiSearchHelper.Search(searchClient, "Start a VM for me", semantic: true, hybrid: true);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine(ex.ToString());
//    }
//}



////try
////{
////    await aiSearchHelper.GenerateAndSaveRunBookDocumentsAsync(configuration, azureOpenAIClient, searchClient, $@"C:\temp\runbooks.json");
////    Task.Delay(5000).Wait();
////    await aiSearchHelper.Search(searchClient, "Start a VM for me", semantic: true, hybrid: true);
////}
////catch (Exception ex)
////{
////    Console.WriteLine(ex.ToString());
////}

////async Task CreateIndexAsync(AISearchHelper aisearchhelper)
////{
////    await aisearchhelper.SetupIndexAsync(configuration, indexClient);
////}
////async Task BuildIndex()
////{

////}



// Description:
// Azure OpenAI Search Helper

// Usage:
//   app[options]

// Options:
//  --create         Create the search index
//  --loaddocument  Generate and load documents into the index
//  --search        Perform a search with the specified query
//  --help          Show help and usage information
//  --version       Show version information

using System.CommandLine;
using System.Text.Json;
using Azure.Identity;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using AzureOpenAISearchConfiguration;
using AzureOpenAISearchHelper;
using Microsoft.Extensions.Configuration;
// ... (keep other using statements)

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
                await aiSearchHelper.Search(searchClient, searchQuery, semantic: true, hybrid: true);
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