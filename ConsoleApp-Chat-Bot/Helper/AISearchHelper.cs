using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Azure;
using Microsoft.Extensions.Configuration;
using AzureOpenAISearchConfiguration;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents;
using System.Text.Json;
using Runbook.Models;
using OpenAI.Embeddings;
using Azure.Search.Documents.Models;
using Azure.Core;
using Microsoft.Extensions.Logging;

namespace AzureOpenAISearchHelper
{
    /// <summary>
    /// AzureOpenAISearchHelper : This class is a helper that allows index creation and parsing of the Runbooks.json file to add documents to the index.
    /// RDC: 12/24/2024 - Working well.
    /// </summary>
    public class AISearchHelper
    {
        private readonly ILogger<AISearchHelper> _logger;

        public AISearchHelper(ILogger<AISearchHelper>? logger = null)
        {
            _logger = logger ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AISearchHelper>();
        }

        public AzureOpenAIClient InitializeOpenAIClient(Configuration configuration, TokenCredential credential)
        {
            if (!string.IsNullOrEmpty(configuration.AzureOpenAIApiKey))
            {
                return new AzureOpenAIClient(new Uri(configuration.AzureOpenAIEndpoint!), new AzureKeyCredential(configuration.AzureOpenAIApiKey));
            }

            return new AzureOpenAIClient(new Uri(configuration.AzureOpenAIEndpoint!), credential);
        }

        public SearchIndexClient InitializeSearchIndexClient(Configuration configuration, TokenCredential credential)
        {
            if (!string.IsNullOrEmpty(configuration.SearchAdminKey))
            {
                return new SearchIndexClient(new Uri(configuration.SearchServiceEndpoint!), new AzureKeyCredential(configuration.SearchAdminKey));
            }

            return new SearchIndexClient(new Uri(configuration.SearchServiceEndpoint!), credential);
        }

        public async Task DeleteIndexAsync(Configuration configuration, SearchIndexClient indexClient)
        {
            try
            {
                _logger.LogInformation("Attempting to delete index: {IndexName}", configuration.IndexName);
                await indexClient.DeleteIndexAsync(configuration.IndexName);
                _logger.LogInformation("Successfully deleted index: {IndexName}", configuration.IndexName);
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status == 404)
                {
                    _logger.LogInformation("Index {IndexName} does not exist.", configuration.IndexName);
                }
                else
                {
                    _logger.LogError(ex, "Error deleting index: {Message}", ex.Message);
                    throw;
                }
            }
        }

        public async Task SetupIndexAsync(Configuration configuration, SearchIndexClient indexClient)
        {
            const string vectorSearchHnswProfile = "my-vector-profile";
            const string vectorSearchHnswConfig = "myHnsw";
            const string vectorSearchVectorizer = "myOpenAIVectorizer";
            const string semanticSearchConfig = "my-semantic-config";

            SearchIndex searchIndex = new(configuration.IndexName)
            {
                VectorSearch = new()
                {
                    Profiles =
                    {
                        new VectorSearchProfile(vectorSearchHnswProfile, vectorSearchHnswConfig)
                        {
                            VectorizerName = vectorSearchVectorizer
                        }
                    },
                    Algorithms =
                    {
                        new HnswAlgorithmConfiguration(vectorSearchHnswConfig)
                        {
                            Parameters = new HnswParameters
                            {
                                M = 4,
                                EfConstruction = 400,
                                EfSearch = 500,
                                Metric = "cosine"
                            }
                        }
                    },
                    Vectorizers =
                    {
                        new AzureOpenAIVectorizer(vectorSearchVectorizer)
                        {
                            Parameters = new AzureOpenAIVectorizerParameters
                            {
                                ResourceUri = new Uri(configuration.AzureOpenAIEndpoint !),
                                ModelName = configuration.AzureOpenAIEmbeddingModel,
                                DeploymentName = configuration.AzureOpenAIEmbeddingDeployment,
                                ApiKey = configuration.AzureOpenAIApiKey
                            }
                        }
                    }
                },
                SemanticSearch = new()
                {
                    Configurations =
                        {
                           new SemanticConfiguration(semanticSearchConfig, new()
                           {
                                TitleField = new SemanticField("name"),
                                ContentFields =
                                {
                                    new SemanticField("description")
                                },
                                KeywordsFields =
                                {
                                    new SemanticField("category"),
                                    new SemanticField("tags")
                                }
                           })

                    },
                },
                Fields =
                    {
                        new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true, IsFacetable = true },
                        new SearchableField("name") { IsFilterable = true, IsSortable = true },
                        new SearchableField("description") { IsFilterable = true },
                        // new SimpleField("parameters", SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsFilterable = false },
                        new SimpleField("parameters", SearchFieldDataType.String) { IsFilterable = false },
                        // new SearchableField("parameters") { IsFilterable = true },
                        new SearchableField("notes") { IsFilterable = true },
                        new SimpleField("tags", SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsFilterable = false},
                        new SimpleField("example", SearchFieldDataType.String) { IsFilterable = false },
                        new SearchableField("category") { IsFilterable = true },
                        new SimpleField("author", SearchFieldDataType.String) { IsFilterable = false },
                        new SimpleField("lastEdit", SearchFieldDataType.String) { IsFilterable = false },
                        new SimpleField("synopsis", SearchFieldDataType.String) { IsFilterable = false },
                        new SimpleField("version", SearchFieldDataType.String) { IsFilterable = false },
                        new SimpleField("dependencies", SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsFilterable = false },
                        new SearchField("descriptionVector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
                        {
                            IsSearchable = true,
                            VectorSearchDimensions = int.Parse(configuration.AzureOpenAIEmbeddingDimensions!),
                            VectorSearchProfileName = vectorSearchHnswProfile
                        }
                    }
            };

            await indexClient.CreateOrUpdateIndexAsync(searchIndex);
        }

        public async Task UploadSampleDocumentsAsync(Configuration configuration, SearchClient searchClient, string sampleDocumentsPath)
        {
            string sampleDocumentContent = File.ReadAllText(sampleDocumentsPath);
            var sampleDocuments = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(sampleDocumentContent);

            var options = new SearchIndexingBufferedSenderOptions<Dictionary<string, object>>
            {
                KeyFieldAccessor = (o) => o["id"].ToString()
            };
            using SearchIndexingBufferedSender<Dictionary<string, object>> bufferedSender = new(searchClient, options);
            await bufferedSender.UploadDocumentsAsync(sampleDocuments);
            await bufferedSender.FlushAsync();
        }

        public async Task GenerateAndSaveRunBookDocumentsAsync(Configuration configuration, AzureOpenAIClient azureOpenAIClient, SearchClient searchClient, string jsonFilePath)
        {
            var runbooks = LoadRunbooksFromJson(jsonFilePath);

            if (runbooks is null || runbooks.Count == 0)
            {
                throw new ArgumentNullException("Upload Runbooks: No data found in JSON.");
            }

            var test = configuration.AzureOpenAIEmbeddingDeployment;
            var embeddingClient = azureOpenAIClient.GetEmbeddingClient(configuration.AzureOpenAIEmbeddingDeployment);
            var embeddingOptions = new EmbeddingGenerationOptions { Dimensions = int.Parse(configuration.AzureOpenAIEmbeddingDimensions!) };
            
            // Generate embeddings for each Runbook
            foreach (var runbook in runbooks)
            {
                string textForEmbedding = $"Name: {runbook.Name ?? ""}, Description: {runbook.Description ?? ""}";

                if (runbook.ParametersJson != null)
                {
                    _logger.LogDebug("Parameters JSON: {ParametersJson}", runbook.ParametersJson);
                }

                runbook.Tags ??= new List<string>();
                runbook.Dependencies ??= new List<string>();

                OpenAIEmbedding embeddingDescription = await embeddingClient.GenerateEmbeddingAsync(textForEmbedding);
                runbook.DescriptionVector = embeddingDescription.ToFloats().ToArray().ToList();
            }

            // Serialize runbooks to JSON for verification
            string serializedRunbooks = JsonSerializer.Serialize(runbooks, new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new ParametersJsonConverter() }
            });

            _logger.LogDebug("Serialized Runbooks JSON: {SerializedRunbooks}", serializedRunbooks);

            // Upload or merge the documents
            var batch = IndexDocumentsBatch.Upload(runbooks);
            var result = await searchClient.IndexDocumentsAsync(batch);
            _logger.LogDebug("Index documents result: {Result}", result.ToString());
        }

        private List<RunbookData> LoadRunbooksFromJson(string jsonFilePath)
        {
            var jsonContent = File.ReadAllText(jsonFilePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                // Ensure that converters are discovered (if necessary)
                // Converters.Add(new ParametersJsonConverter()) // Not needed since applied via attribute
            };

            return JsonSerializer.Deserialize<List<RunbookData>>(jsonContent, options) ?? new List<RunbookData>();
        }

        public async Task<List<RunbookDetails>> Search(SearchClient searchClient, string query, int k = 3, int top = 10, string? filter = null, bool textOnly = false, bool exhaustive = false, bool hybrid = false, bool semantic = false, string debug = "disabled", double minRerankerScore = 2.0)
        {
            var searchOptions = new SearchOptions
            {
                Filter = filter,
                Size = top,
                Select = { "name", "id", "description", "parameters"},
                HighlightFields = { "name" },
                IncludeTotalCount = true
            };

            if (!textOnly)
            {
                searchOptions.VectorSearch = new()
                {
                    Queries = {
                        new VectorizableTextQuery(text: query)
                        {
                            KNearestNeighborsCount = k,
                            Fields = { "descriptionVector" },
                            Exhaustive = exhaustive
                        },
                    },

                };
            }

            if (semantic)
            {
                searchOptions.QueryType = SearchQueryType.Semantic;
                searchOptions.SemanticSearch = new SemanticSearchOptions
                {
                    SemanticConfigurationName = "my-semantic-config",
                    QueryCaption = new QueryCaption(QueryCaptionType.Extractive),
                    QueryAnswer = new QueryAnswer(QueryAnswerType.Extractive),
                };
            }

            string? queryText = (textOnly || hybrid || semantic) ? query : null;
            SearchResults<SearchDocument> response = await searchClient.SearchAsync<SearchDocument>(queryText, searchOptions);

            List<RunbookDetails> runbookDetailsList = new List<RunbookDetails>();

            if (response.SemanticSearch?.Answers?.Count > 0)
            {
                _logger.LogDebug("Query Answers:");
                foreach (QueryAnswerResult answer in response.SemanticSearch.Answers)
                {
                    _logger.LogDebug("Answer Highlights: {Highlights}", answer.Highlights);
                    _logger.LogDebug("Answer Text: {Text}", answer.Text);
                }
            }

            int count = 0;
            await foreach (SearchResult<SearchDocument> result in response.GetResultsAsync())
            {
                if (result.SemanticSearch?.RerankerScore >= minRerankerScore)
                {
                    count++;
                    RunbookDetails runBookDetails = new RunbookDetails();

                    runBookDetails.id = (string)result.Document["id"];
                    runBookDetails.Name = (string)result.Document["name"];
                    runBookDetails.Description = (string)result.Document["description"];
                    runBookDetails.Parameters = (string)result.Document["parameters"];
                    runBookDetails.Score = (double)result.Score!;
                    runBookDetails.RerankerScore = (double)result.SemanticSearch.RerankerScore;

                    runbookDetailsList.Add(runBookDetails);

                    _logger.LogDebug("Search Result - Name: {Name}, Score: {Score}, RerankerScore: {RerankerScore}", 
                        result.Document["name"],
                        result.Score,
                        result.SemanticSearch!.RerankerScore);

                    var truncatedDescription = result.Document["description"] != null && result?.Document?["description"]?.ToString()?.Length > 20
                        ? result.Document["description"]?.ToString()?.Substring(0, 20)
                        : result!.Document["description"];
                    _logger.LogDebug("Description: {Description}", truncatedDescription);

                    if (result.SemanticSearch?.Captions?.Count > 0)
                    {
                        QueryCaptionResult firstCaption = result.SemanticSearch.Captions[0];
                        _logger.LogDebug("First Caption - Highlights: {Highlights}, Text: {Text}", 
                            firstCaption.Highlights,
                            firstCaption.Text);
                    }
                }
            }
            _logger.LogDebug("Total Results: {TotalCount}", response.TotalCount);
            return runbookDetailsList;
        }

    }
}
