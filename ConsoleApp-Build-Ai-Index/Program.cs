using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.Identity;
using Azure.AI.OpenAI;  // for EmbeddingsOptions
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using OpenAI;
using OpenAI.Embeddings;
using AzureOpenAISearchConfiguration;
using Microsoft.Extensions.Configuration;
using AzureOpenAISearchHelper;

// using Azure.Search.Documents.Models;

// IMPORTANT:
//  - If your "AzureOpenAIClient" class or "EmbeddingsOptions" class
//    come from a custom or different namespace, be sure to adjust
//    these using statements accordingly.


// Create the Configuration Object - a helper class to help us with the environment variables needed.
var configuration = new Configuration();
var aiSearchHelper = new AISearchHelper();
new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddEnvironmentVariables()
              .AddJsonFile("local.settings.json")
              .Build()
              .Bind(configuration);

configuration.Validate();

var defaultCredential = new DefaultAzureCredential();
var azureOpenAIClient = aiSearchHelper.InitializeOpenAIClient(configuration, defaultCredential);
var indexClient = aiSearchHelper.InitializeSearchIndexClient(configuration, defaultCredential);
var searchClient = indexClient.GetSearchClient(configuration.IndexName);
await aiSearchHelper.SetupIndexAsync(configuration,indexClient);
try
{
    await aiSearchHelper.GenerateAndSaveRunBookDocumentsAsync(configuration, azureOpenAIClient, searchClient, $@"C:\temp\runbooks.json");
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}



// var azureOpenAIClient = InitializeOpenAIClient(configuration, defaultCredential);
//var indexClient = InitializeSearchIndexClient(configuration, defaultCredential);
//var searchClient = indexClient.GetSearchClient(configuration.IndexName);

// --- Constants / Configuration ---
//var openAiEndpoint = "https://YourOpenAIEndpoint.openai.azure.com/";
//var openAiKey = "YourOpenAIKey";
//var openAiDeploymentOrModel = "text-embedding-ada-002";

//var searchServiceEndpoint = "https://YourSearchServiceName.search.windows.net";
//var searchApiKey = "YourSearchApiKey";
//var indexName = "runbooks-index";

//// Path to your JSON file
//var jsonFilePath = "runbooks.json";

//// 1) Load data from JSON
//Console.WriteLine("Loading runbooks from JSON...");
//var runbooks = await LoadRunbooksFromJsonAsync(jsonFilePath);
//if (runbooks is null || runbooks.Count == 0)
//{
//    Console.WriteLine("No data found in JSON.");
//    return;
//}

// 2) Create the AzureOpenAIClient
//    Note: This constructor signature must match your custom client.
//var openAiClient = new AzureOpenAIClient(
//    new Uri(openAiEndpoint), 
//    new AzureKeyCredential(openAiKey)
//);

//// 3) Generate embeddings for each runbook
//Console.WriteLine("Generating embeddings using AzureOpenAIClient...");
//foreach (var runbook in runbooks)
//{
//    string textForEmbedding = $"Name: {runbook.Name ?? ""}, Description: {runbook.Description ?? ""}";

//    if (string.IsNullOrWhiteSpace(textForEmbedding))
//    {
//        runbook.DescriptionVector = Array.Empty<float>();
//        continue;
//    }

  

//    // Call your custom method to get embeddings
//    // If your AzureOpenAIClient uses a different signature, adjust accordingly.
//    var embeddingClient = openAiClient.GetEmbeddingClient(openAiDeploymentOrModel);

//    var embeddingResponse = await embeddingClient.GenerateEmbeddingAsync(textForEmbedding);
//    IReadOnlyList<float> embeddingVector = (IReadOnlyList<float>)embeddingResponse.Value;


//    // Store the embedding in DescriptionVector
//    runbook.DescriptionVector = embeddingVector.ToArray();
//}

//// 4) Create or update the Azure Cognitive Search index
//Console.WriteLine($"Creating or updating index '{indexName}'...");
//await CreateOrUpdateIndexAsync(indexName, searchServiceEndpoint, searchApiKey);

//// 5) Upload the runbooks to the index
//Console.WriteLine("Uploading runbooks to the index...");
//await UploadRunbooksAsync(runbooks, indexName, searchServiceEndpoint, searchApiKey);

//Console.WriteLine("Done! Press any key to exit.");
//Console.ReadKey();

//static AzureOpenAIClient InitializeOpenAIClient(Configuration configuration, DefaultAzureCredential defaultCredential)
//{
//    if (!string.IsNullOrEmpty(configuration.AzureOpenAIApiKey))
//    {
//        return new AzureOpenAIClient(new Uri(configuration.AzureOpenAIEndpoint!), new AzureKeyCredential(configuration.AzureOpenAIApiKey));
//    }

//    return new AzureOpenAIClient(new Uri(configuration.AzureOpenAIEndpoint!), defaultCredential);
//}

//static SearchIndexClient InitializeSearchIndexClient(Configuration configuration, DefaultAzureCredential defaultCredential)
//{
//    if (!string.IsNullOrEmpty(configuration.SearchAdminKey))
//    {
//        return new SearchIndexClient(new Uri(configuration.SearchServiceEndpoint!), new AzureKeyCredential(configuration.SearchAdminKey));
//    }

//    return new SearchIndexClient(new Uri(configuration.SearchServiceEndpoint!), defaultCredential);
//}

//static async Task SetupIndexAsync(Configuration configuration, SearchIndexClient indexClient)
//{
//    const string vectorSearchHnswProfile = "my-vector-profile";
//    const string vectorSearchHnswConfig = "myHnsw";
//    const string vectorSearchVectorizer = "myOpenAIVectorizer";
//    const string semanticSearchConfig = "my-semantic-config";

//    SearchIndex searchIndex = new(configuration.IndexName)
//    {
//        VectorSearch = new()
//        {
//            Profiles =
//                    {
//                        new VectorSearchProfile(vectorSearchHnswProfile, vectorSearchHnswConfig)
//                        {
//                            VectorizerName = vectorSearchVectorizer
//                        }
//                    },
//            Algorithms =
//                    {
//                        new HnswAlgorithmConfiguration(vectorSearchHnswConfig)
//                        {
//                            Parameters = new HnswParameters
//                            {
//                                M = 4,
//                                EfConstruction = 400,
//                                EfSearch = 500,
//                                Metric = "cosine"
//                            }
//                        }
//                    },
//            Vectorizers =
//                    {
//                        new AzureOpenAIVectorizer(vectorSearchVectorizer)
//                        {
//                            Parameters = new AzureOpenAIVectorizerParameters
//                            {
//                                ResourceUri = new Uri(configuration.AzureOpenAIEndpoint !),
//                                ModelName = configuration.AzureOpenAIEmbeddingModel,
//                                DeploymentName = configuration.AzureOpenAIEmbeddingDeployment
//                            }
//                        }
//                    }
//        },
//        SemanticSearch = new()
//        {
//            Configurations =
//                        {
//                           new SemanticConfiguration(semanticSearchConfig, new()
//                           {
//                                TitleField = new SemanticField("name"),
//                                ContentFields =
//                                {
//                                    new SemanticField("description")
//                                },
//                                KeywordsFields =
//                                {
//                                    new SemanticField("category"),
//                                    new SemanticField("tags")
//                                }
//                           })

//                    },
//        },
//        Fields =
//            {
//                new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true, IsFacetable = true },
//                new SearchableField("name") { IsFilterable = true, IsSortable = true },
//                new SearchableField("description") { IsFilterable = true },
//                new SimpleField("parameters", SearchFieldDataType.String) { IsFilterable = false },
//                new SearchableField("notes") { IsFilterable = true },
//                new SimpleField("tags", SearchFieldDataType.String) { IsFilterable = false },
//                new SearchableField("category") { IsFilterable = true },
//                new SimpleField("author", SearchFieldDataType.String) { IsFilterable = false },
//                new SimpleField("lastEdit", SearchFieldDataType.String) { IsFilterable = false },
//                new SimpleField("synopsis", SearchFieldDataType.String) { IsFilterable = false },
//                new SimpleField("version", SearchFieldDataType.String) { IsFilterable = false },
//                new SimpleField("dependencies", SearchFieldDataType.String) { IsFilterable = false },
//                new SearchField("descriptionVector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
//                {
//                    IsSearchable = true,
//                    VectorSearchDimensions = int.Parse(configuration.AzureOpenAIEmbeddingDimensions!),
//                    VectorSearchProfileName = vectorSearchHnswProfile
//                }
//            }
//    };

//    await indexClient.CreateOrUpdateIndexAsync(searchIndex);
//}



// -------------------- Local Functions --------------------
//async Task<List<RunbookData>> LoadRunbooksFromJsonAsync(string path)
//{
//    if (!File.Exists(path))
//    {
//        Console.WriteLine($"File not found: {path}");
//        return new List<RunbookData>();
//    }

//    string jsonContent = await File.ReadAllTextAsync(path);
//    return JsonSerializer.Deserialize<List<RunbookData>>(jsonContent, new JsonSerializerOptions
//    {
//        PropertyNameCaseInsensitive = true
//    }) ?? new List<RunbookData>();
//}

//async Task CreateOrUpdateIndexAsync(string idxName, string searchEndpoint, string apiKey)
//{
//    const string vectorSearchHnswProfile = "myHnswProfile";
//    const string vectorSearchHnswConfig = "myHnsw";
//    const string vectorSearchVectorizer = "myOpenAIVectorizer";
//    const string semanticSearchConfig = "my-semantic-config";

//    var indexClient = new SearchIndexClient(new Uri(searchEndpoint), new AzureKeyCredential(apiKey));

//    var fields = new List<SearchField>
//    {
//        // 'name' field - unique identifier
//        new SimpleField("id", SearchFieldDataType.String)
//        {
//            IsKey = true,
//            IsFilterable = true
//        },
//        new SearchField("name", SearchFieldDataType.String)
//        {
//            IsFilterable = true,
//            IsSortable = true,
//        },
//        new SearchField("description", SearchFieldDataType.String)
//        {
//            IsFilterable = true
//        },
//        new SimpleField("parameters", SearchFieldDataType.String)
//        {
//            IsFilterable = false
//        },
//        new SearchField("notes", SearchFieldDataType.String)
//        {
//            IsFilterable = false
//        },
//        new SimpleField("tags", SearchFieldDataType.String)
//        {
//            IsFilterable = true ,
//        },
//        new SearchField("category", SearchFieldDataType.String)
//        {
//            IsFilterable = true
//        },
//        new SimpleField("author", SearchFieldDataType.String)
//        {
//            IsFilterable = false ,
//        },
//        new SimpleField("lastEdit", SearchFieldDataType.String)
//        {
//            IsFilterable = false ,
//        },
//        new SimpleField("synipsis", SearchFieldDataType.String)
//        {
//            IsFilterable = false ,
//        },
//        new SimpleField("version", SearchFieldDataType.String)
//        {
//            IsFilterable = false ,
//        },
//        new SimpleField("dependencies", SearchFieldDataType.String)
//        {
//            IsFilterable = false ,
//        },
//        // Vector field 'descriptionVector'
//        new SearchField("descriptionVector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
//        {
//            IsFilterable = false,
//            IsSortable = false,
//            IsFacetable = false,
//            IsSearchable = false,

//            // Vector-specific properties
//            VectorSearchDimensions = 1536,              // Must match your embedding model (e.g., text-embedding-ada-002)
//            VectorSearchProfileName = "myHnswProfile"    // Must match your vector search profile name
//        }
//    };

    // Define Vector Search Configuration
    //var vectorSearch = new VectorSearch
    //{
    //    Algorithms = new List<HnswAlgorithmConfiguration>
    //    {
    //        new HnswAlgorithmConfiguration
    //        {
    //            Name = "my-vector-config",
    //            Kind = "hnsw",
    //            Parameters = new Dictionary<string, object>
    //            {
    //                { "m", 4 },
    //                { "efConstruction", 400 },
    //                { "efSearch", 500 },
    //                { "metric", "cosine" }
    //            }
    //        }
    //    }
    //};


    //var vectorSearch = new VectorSearch
    //{
    //    Algorithms =
    //            {
    //                new HnswAlgorithmConfiguration("myHnsw")
    //                {
    //                    name
    //                    // HNSW Parameters
    //                    m = 4,
    //                    EfConstruction = 400,
    //                    EfSearch = 500,
    //                    SimilarityMetric = "cosine" // Options: "cosine", "dotProduct", "euclidean"
    //                }
    //            },
    //    Profiles =
    //            {
    //                new VectorSearchProfile("myHnswProfile")
    //                {
    //                    AlgorithmConfiguration = "myHnsw"
    //                }
    //            }
    //};

    // Define Semantic Configuration
    //var semanticConfig = new SemanticConfiguration("default",
    //    new SemanticPrioritizedFields
    //    {
    //        TitleField = new SemanticField("vendorName"),
    //        KeywordsFields = { new SemanticField("contractTitle"), new SemanticField("clientName") },
    //        ContentFields = { new SemanticField("content") }
    //    }
    //);

    //var semanticSearch = new SemanticSearch
    //{
    //    Configurations = { semanticConfig }
    //};

    //// Create the SearchIndex with fields, vector search, and semantic search
    //var indexDefinition = new SearchIndex(indexName, fields)
    //{
    //    VectorSearch = vectorSearch,
    //    SemanticSearch = semanticSearch
    //};
    // Vector field 'descriptionVector'
    //new SearchField("descriptionVector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
    //{
    //    IsFilterable = false,
    //    IsSortable = false,
    //    IsFacetable = false,
    //    IsSearchable = false,

    //    // Vector-specific properties
    //    VectorSearchDimensions = 1536,              // Must match your embedding model (e.g., text-embedding-ada-002)
    //    VectorSearchProfileName = "myHnswProfile"    // Must match your vector search profile name
    //}


    //// Use reflection-based FieldBuilder to generate fields for RunbookData
    //var fields = new FieldBuilder().Build(typeof(RunbookData));

    //// Remove any existing auto-detected field for "descriptionVector"
    //fields.RemoveAll(f => f.Name.Equals("descriptionVector", StringComparison.OrdinalIgnoreCase));

    //// Create a vector field for the embeddings
    //var vectorField = new SearchField(
    //    name: "descriptionVector",
    //    type: SearchFieldDataType.Collection(SearchFieldDataType.Single))
    //    {
    //        // Optional typical field settings
    //        IsFilterable = false,
    //        IsSortable = false,
    //        IsFacetable = false,
    //        IsSearchable = false,

    //        // Vector-specific properties
    //        VectorSearchDimensions = 1536,              // Must match your embedding model
    //        VectorSearchProfileName = "myHnswProfile" // Must match your configured vector search profile
    //    };

    //fields.Add(vectorField);

    //var semanticConfig = new SemanticConfiguration("default",
    //            new SemanticPrioritizedFields
    //            {
    //                TitleField = new SemanticField("descriptionVector"),
    //                KeywordsFields = { "contractTitle", "clientName" },
    //                ContentFields = { "content" }
    //            }
    //        );

    //var semanticSearch = new SemanticSearch
    //{
    //    Configurations = { semanticConfig }
    //};

    //// Create the search index definition
    //var definition = new SearchIndex(idxName, fields)
    //{
    //    // Add vector search configuration
    //    VectorSearch = new VectorSearch,
    //    SemanticSearch = semanticSearch
    //};

    //// If you want 'name' as primary key (assuming it's unique)
    //definition.Fields["name"].IsKey = true;

    // Create or update the index
    //await indexClient.CreateOrUpdateIndexAsync(definition);
//}



//async Task UploadRunbooksAsync(
//    List<RunbookData> runbooks,
//    string idxName,
//    string searchEndpoint,
//    string apiKey)
//{
//    var searchClient = new SearchClient(new Uri(searchEndpoint), idxName, new AzureKeyCredential(apiKey));

//    // Upload or merge the documents
//    var batch = IndexDocumentsBatch.Upload(runbooks);
//    await searchClient.IndexDocumentsAsync(batch);
//}

// -------------------- Data Models --------------------
//public class RunbookData
//{
//    [JsonPropertyName("tags")]
//    public List<string>? Tags { get; set; }

//    [JsonPropertyName("example")]
//    public string? Example { get; set; }

//    [JsonPropertyName("parameters")]
//    public List<RunbookParameter>? Parameters { get; set; }

//    [JsonPropertyName("category")]
//    public string? Category { get; set; }

//    [JsonPropertyName("author")]
//    public string? Author { get; set; }

//    [JsonPropertyName("description")]
//    public string? Description { get; set; }

//    [JsonPropertyName("synopsis")]
//    public string? Synopsis { get; set; }

//    [JsonPropertyName("version")]
//    public string? Version { get; set; }

//    [JsonPropertyName("name")]
//    public string? Name { get; set; }

//    [JsonPropertyName("notes")]
//    public string? Notes { get; set; }

//    [JsonPropertyName("lastEdit")]
//    public string? LastEdit { get; set; }

//    [JsonPropertyName("dependencies")]
//    public List<string>? Dependencies { get; set; }

//    // Vector field for embeddings
//    public IList<float>? DescriptionVector { get; set; }
//}

//public class RunbookParameter
//{
//    [JsonPropertyName("required")]
//    public bool Required { get; set; }

//    [JsonPropertyName("type")]
//    public string? Type { get; set; }

//    [JsonPropertyName("description")]
//    public string? Description { get; set; }

//    [JsonPropertyName("default")]
//    public string? Default { get; set; }

//    [JsonPropertyName("name")]
//    public string? Name { get; set; }
//}
