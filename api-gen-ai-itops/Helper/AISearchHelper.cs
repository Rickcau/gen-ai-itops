using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Azure;
using Microsoft.Extensions.Configuration;
using Helper.AzureOpenAISearchConfiguration;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents;
using System.Text.Json;
using Runbook.Models;
using OpenAI.Embeddings;
using Azure.Search.Documents.Models;
using Azure.Core;
using Microsoft.Extensions.Logging;
using api_gen_ai_itops.Models;
using api_gen_ai_itops.Interfaces;

namespace Helper.AzureOpenAISearchHelper
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

        public async Task DeleteIndexV1Async(Configuration configuration, SearchIndexClient indexClient)
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

        // V1 is not really needed, so need to cl
        public async Task DeleteIndexAsync(string indexName, SearchIndexClient indexClient)
        {
          
                _logger.LogInformation("Attempting to delete index: {IndexName}", indexName);
                await indexClient.DeleteIndexAsync(indexName);
                _logger.LogInformation("Successfully deleted index: {IndexName}", indexName);
        }

        public async Task<IReadOnlyList<string>> GetIndexesAsync(SearchIndexClient indexClient)
        {
            try
            {
                _logger.LogInformation("Retrieving list of indexes");
                var indexes = await indexClient.GetIndexNamesAsync().ToListAsync();
                _logger.LogInformation("Successfully retrieved {Count} indexes", indexes.Count);
                return indexes;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Error retrieving indexes: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<SearchIndexDetails> GetIndexDetailsAsync(string indexName, SearchIndexClient indexClient)
        {
            try
            {
                _logger.LogInformation("Retrieving details for index: {IndexName}", indexName);
                SearchIndex index = await indexClient.GetIndexAsync(indexName);

                var details = new SearchIndexDetails
                {
                    Name = index.Name,
                    Fields = index.Fields.Select(f => new FieldInfo
                    {
                        Name = f.Name,
                        Type = f.Type.ToString(),
                        IsSearchable = f.IsSearchable ?? false,
                        IsFilterable = f.IsFilterable ?? false,
                        IsSortable = f.IsSortable ?? false,
                        IsFacetable = f.IsFacetable ?? false,
                        IsKey = f.IsKey ?? false
                    }).ToList(),
                    HasVectorSearch = index.VectorSearch != null,
                    HasSemanticSearch = index.SemanticSearch != null,
                    Vectorizers = index.VectorSearch?.Vectorizers?.Select(v => v.GetType().Name)?.ToList() ?? new List<string>(),
SemanticConfigurations = index.SemanticSearch?.Configurations?.Select(c => c.Name)?.ToList() ?? new List<string>()
                };

                _logger.LogInformation("Successfully retrieved index details for {IndexName}", indexName);
                return details;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Error retrieving index details: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IndexStatistics> GetIndexStatisticsAsync(string indexName, SearchIndexClient indexClient)
        {
            try
            {
                _logger.LogInformation("Retrieving statistics for index: {IndexName}", indexName);
                var stats = await indexClient.GetIndexStatisticsAsync(indexName);

                return new IndexStatistics
                {
                    DocumentCount = stats.Value.DocumentCount,
                    VectorIndexSize = stats.Value.VectorIndexSize,
                    StorageSizeInBytes = stats.Value.StorageSize
                };
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Error retrieving index statistics: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<List<SearchDocument>> ListDocumentsAsync(
                string indexName,
                SearchClient searchClient,
                SearchIndexClient indexClient,
                bool suppressVectorFields = true,
                int maxResults = 1000)
        {
            try
            {
                _logger.LogInformation("Retrieving documents from index: {IndexName}", indexName);

                var searchOptions = new SearchOptions
                {
                    Size = maxResults,
                    IncludeTotalCount = true
                };

                // If suppressVectorFields is true, get the index schema and exclude vector fields
                if (suppressVectorFields)
                {
                    SearchIndex index = await indexClient.GetIndexAsync(indexName);
                    var nonVectorFields = index.Fields
                        .Where(f => f.Type != SearchFieldDataType.Collection(SearchFieldDataType.Single))
                        .Select(f => f.Name);

                    // Add each field individually to the Select list
                    foreach (var field in nonVectorFields)
                    {
                        searchOptions.Select.Add(field);
                    }
                }

                var response = await searchClient.SearchAsync<SearchDocument>("*", searchOptions);
                var documents = new List<SearchDocument>();

                foreach (var result in response.Value.GetResults())
                {
                    documents.Add(result.Document);
                }

                _logger.LogInformation("Retrieved {Count} documents from index {IndexName}", documents.Count, indexName);
                return documents;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Error retrieving documents: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<List<Capability>> ListCapabilityDocumentsAsync(
    string indexName,
    SearchClient searchClient2,
    SearchIndexClient indexClient,
    bool suppressVectorFields = true,
    int maxResults = 1000)
        {
            try
            {
                var searchClient = indexClient.GetSearchClient(indexName);
                _logger.LogInformation("Retrieving capability documents from index: {IndexName}", indexName);

                var searchOptions = new SearchOptions
                {
                    Size = maxResults,
                    IncludeTotalCount = true
                };

                if (suppressVectorFields)
                {
                    SearchIndex index = await indexClient.GetIndexAsync(indexName);
                    var nonVectorFields = index.Fields
                        .Where(f => f.Type != SearchFieldDataType.Collection(SearchFieldDataType.Single))
                        .Select(f => f.Name);

                    foreach (var field in nonVectorFields)
                    {
                        searchOptions.Select.Add(field);
                    }
                }

                var response = await searchClient.SearchAsync<SearchDocument>("*", searchOptions);
                var capabilities = new List<Capability>();

                await foreach (var result in response.Value.GetResultsAsync())
                {
                    var doc = result.Document;
                    var parameters = new List<Parameter>();
                    var executionMethod = new ExecutionMethod();

                    // Deserialize parameters from single JSON string
                    if (doc.TryGetValue("parameters", out object? paramsObj))
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        string paramStr = paramsObj.ToString() ?? string.Empty;
                        parameters = JsonSerializer.Deserialize<List<Parameter>>(paramStr, options) ?? new List<Parameter>();
                    }

                    // Deserialize execution method
                    if (doc.TryGetValue("executionMethod", out object? execObj) &&
                        execObj is IDictionary<string, object> execDict)
                    {
                        executionMethod.Type = execDict.TryGetValue("type", out object? typeObj)
                            ? typeObj?.ToString() ?? "" : "";
                        executionMethod.Details = execDict.TryGetValue("details", out object? detailsObj)
                            ? detailsObj?.ToString() ?? "" : "";
                    }

                    var capability = new Capability
                    {
                        Id = doc.GetString("id") ?? "",
                        CapabilityType = doc.GetString("capabilityType") ?? "",
                        Name = doc.GetString("name") ?? "",
                        Description = doc.GetString("description") ?? "",
                        Tags = doc.TryGetValue("tags", out object? tagsObj) && tagsObj is IEnumerable<object> tagList
                                    ? tagList.Select(t => t.ToString() ?? "").ToList()
                                    : new List<string>(),
                        Parameters = parameters,
                        ExecutionMethod = executionMethod
                    };

                    capabilities.Add(capability);
                }

                _logger.LogInformation("Retrieved {Count} capabilities from index {IndexName}",
                    capabilities.Count, indexName);
                return capabilities;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Error retrieving capabilities: {Message}", ex.Message);
                throw;
            }
        }


        // Creates the Index for use with the data found in the CSV 
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

        // Creates the Capabilities Index
        public async Task SetupCapabilitiesIndexAsync(Configuration configuration, SearchIndexClient indexClient, string indexName)
        {
            const string vectorSearchHnswProfile = "my-vector-profile";
            const string vectorSearchHnswConfig = "myHnsw";
            const string vectorSearchVectorizer = "myOpenAIVectorizer";
            const string semanticSearchConfig = "my-semantic-config";

            SearchIndex searchIndex = new(indexName)
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
                                ResourceUri = new Uri(configuration.AzureOpenAIEndpoint!),
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
                                new SemanticField("capabilityType"),
                                new SemanticField("tags")
                            }
                        })
                    }
                },
                Fields =
                {
                    new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true },
                    new SimpleField("capabilityType", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                    new SearchableField("name") { IsFilterable = true, IsSortable = true },
                    new SearchableField("description") { IsFilterable = true },
                    new SimpleField("tags", SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsFilterable = true, IsFacetable = true },
                    new SimpleField("parameters", SearchFieldDataType.String)
                    {
                        IsFilterable = false
                    },
                    //new SimpleField("parameters", SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsFilterable = false },
                    new ComplexField("executionMethod")
                    {
                        Fields =
                        {
                            new SimpleField("type", SearchFieldDataType.String) { IsFilterable = true },
                            new SimpleField("details", SearchFieldDataType.String) { IsFilterable = true }
                        }
                    },
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

        public async Task GenerateCapabilityEmbeddingsForSearchAsync(
            Configuration configuration,
            AzureOpenAIClient azureOpenAIClient,
            ICosmosDbService cosmosDbService,
            SearchClient searchClient)
        {
            var capabilities = await cosmosDbService.GetCapabilitiesAsync();
            if (!capabilities.Any())
            {
                _logger.LogWarning("No capabilities found in Cosmos DB.");
                return;
            }

            var embeddingClient = azureOpenAIClient.GetEmbeddingClient(configuration.AzureOpenAIEmbeddingDeployment);

            foreach (var capability in capabilities)
            {
                try
                {
                    string textForEmbedding = $"Name: {capability.Name ?? ""}, Description: {capability.Description ?? ""}";
                    OpenAIEmbedding embeddingDescription = await embeddingClient.GenerateEmbeddingAsync(textForEmbedding);

                    //var searchDocument = new SearchDocument
                    //{
                    //    ["id"] = capability.Id,
                    //    ["capabilityType"] = capability.CapabilityType,
                    //    ["name"] = capability.Name,
                    //    ["description"] = capability.Description,
                    //    ["tags"] = capability.Tags,
                    //    ["parameters"] = capability.Parameters,
                    //    ["executionMethod"] = capability.ExecutionMethod,
                    //    ["descriptionVector"] = embeddingDescription.ToFloats().ToArray()
                    //};

                    var searchDocument = new SearchDocument
                    {
                        ["id"] = capability.Id,
                        ["capabilityType"] = capability.CapabilityType,
                        ["name"] = capability.Name,
                        ["description"] = capability.Description,
                        ["tags"] = capability.Tags,
                        ["parameters"] = JsonSerializer.Serialize(capability.Parameters), // Serialize to JSON string
                        ["executionMethod"] = capability.ExecutionMethod,
                        ["descriptionVector"] = embeddingDescription.ToFloats().ToArray()
                    };


                    await searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(new[] { searchDocument }));
                    _logger.LogInformation("Indexed capability {Id} with embedding", capability.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing capability {Id}: {Message}", capability.Id, ex.Message);
                }
            }
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

        public async Task<List<Capability>> SearchCapabilities(
            SearchClient searchClient,
            string query,
            int k = 3,
            int top = 10,
            string? filter = null,
            bool textOnly = false,
            bool hybrid = true,
            bool semantic = false,
            double minRerankerScore = 2.0)
        {
            var searchOptions = new SearchOptions
            {
                Filter = filter,
                Size = top,
                Select = { "id", "name", "description", "capabilityType", "tags", "parameters", "executionMethod" },
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
                    Fields = { "descriptionVector" }
                }
            }
                };
            }

            //if (semantic)
            //{
            //    searchOptions.QueryType = SearchQueryType.Semantic;
            //    searchOptions.SemanticSearch = new SemanticSearchOptions
            //    {
            //        SemanticConfigurationName = "my-semantic-config",
            //        QueryCaption = new QueryCaption(QueryCaptionType.Extractive)
            //    };
            //}

            if (hybrid || semantic)
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

            var capabilities = new List<Capability>();
            await foreach (var result in response.GetResultsAsync())
            {
                // Use RerankerScore if available; otherwise, fallback to Score for filtering
                double? relevanceScore = result.SemanticSearch?.RerankerScore ?? result.Score;

                double adjustedMinRerankerScore = textOnly ? 0.03 : minRerankerScore;

                //if (result.SemanticSearch?.RerankerScore >= minRerankerScore)
                //{
                if (relevanceScore >= adjustedMinRerankerScore)
                {
                    //Capability capability = new Capability
                    //{
                    //    Id = (string)result.Document["id"],
                    //    Name = (string)result.Document["name"],
                    //    Description = (string)result.Document["description"],
                    //    CapabilityType = (string)result.Document["capabilityType"],
                    //    Tags = ((string[])result.Document["tags"]).ToList(),
                    //    Parameters = result.Document["parameters"]?.ToString() is string paramStr ?
                    //        JsonSerializer.Deserialize<List<Parameter>>(paramStr) ?? new List<Parameter>() : new List<Parameter>(),
                    //    ExecutionMethod = result.Document["executionMethod"]?.ToString() is string execStr ?
                    //        JsonSerializer.Deserialize<ExecutionMethod>(execStr) ?? new ExecutionMethod() : new ExecutionMethod()
                    //};
                    Capability capability = new Capability
                    {
                        Id = (string)result.Document["id"],
                        Name = (string)result.Document["name"],
                        Description = (string)result.Document["description"],
                        CapabilityType = (string)result.Document["capabilityType"],
                        Tags = result.Document["tags"] is IEnumerable<object> tagObjects
                                    ? tagObjects.Select(tag => tag.ToString() ?? "").ToList()
                                    : new List<string>(),
                                                    Parameters = result.Document["parameters"]?.ToString() is string paramStr
                                    ? JsonSerializer.Deserialize<List<Parameter>>(paramStr) ?? new List<Parameter>()
                                    : new List<Parameter>(),
                                                    ExecutionMethod = result.Document["executionMethod"]?.ToString() is string execStr
                                    ? JsonSerializer.Deserialize<ExecutionMethod>(execStr) ?? new ExecutionMethod()
                                    : new ExecutionMethod()
                    };


                    capabilities.Add(capability);

                    _logger.LogDebug("Search Result - Name: {Name}, Score: {Score}, RerankerScore: {RerankerScore}",
                        capability.Name,
                        result.Score,
                        result.SemanticSearch?.RerankerScore);
                }
            }

            _logger.LogDebug("Total Results: {Count}", capabilities.Count);
            return capabilities;
        }

        public async Task UpsertCapabilityDocumentsAsync(string indexName, IEnumerable<Capability> capabilities,
                SearchClient searchClient, AzureOpenAIClient azureOpenAIClient, Configuration configuration)
        {
            try
            {
                _logger.LogInformation("Upserting capability documents to index: {IndexName}", indexName);

                var embeddingClient = azureOpenAIClient.GetEmbeddingClient(configuration.AzureOpenAIEmbeddingDeployment);
                var documents = new List<Dictionary<string, object>>();

                foreach (var capability in capabilities)
                {
                    if (string.IsNullOrEmpty(capability.Id) || string.IsNullOrEmpty(capability.CapabilityType))
                    {
                        throw new ArgumentException($"Capability must have both Id and CapabilityType");
                    }

                    // Generate embedding for the description
                    string textForEmbedding = $"Name: {capability.Name ?? ""}, Description: {capability.Description ?? ""}";
                    OpenAIEmbedding embeddingDescription = await embeddingClient.GenerateEmbeddingAsync(textForEmbedding);

                    // Convert parameters to string array
                    // var parameters = capability.Parameters?.Select(p => JsonSerializer.Serialize(p)).ToList() ?? new List<string>();

                    // After: Store entire list as single JSON string
                    var parameters = JsonSerializer.Serialize(capability.Parameters);

                    var document = new Dictionary<string, object>
                    {
                        { "id", capability.Id },
                        { "capabilityType", capability.CapabilityType },
                        { "name", capability.Name ?? "" },
                        { "description", capability.Description ?? "" },
                        { "tags", capability.Tags ?? new List<string>() },
                        { "parameters", parameters },
                        { "executionMethod", new Dictionary<string, object>
                            {
                                { "type", capability.ExecutionMethod?.Type ?? "" },
                                { "details", capability.ExecutionMethod?.Details ?? "" }
                            }
                        },
                        { "descriptionVector", embeddingDescription.ToFloats().ToArray().ToList()  }
                    };

                    documents.Add(document);
                }

                var batch = IndexDocumentsBatch.Upload(documents);
                await searchClient.IndexDocumentsAsync(batch);

                _logger.LogInformation("Successfully upserted {Count} capabilities to index {IndexName}",
                    capabilities.Count(), indexName);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Error upserting capabilities: {Message}", ex.Message);
                throw;
            }
        }
    }
}
