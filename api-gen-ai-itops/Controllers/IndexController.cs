using Azure.Search.Documents.Indexes;
using Microsoft.AspNetCore.Mvc;
using Helper.AzureOpenAISearchHelper;
using Helper.AzureOpenAISearchConfiguration;
using System.Net.Mime;
using Swashbuckle.AspNetCore.Annotations;
using Azure.Core;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using api_gen_ai_itops.Models;
using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Search.Documents.Models;
using Octokit;
using System.Runtime.InteropServices;
using api_gen_ai_itops.Interfaces;

namespace api_gen_ai_itops.Controllers
{
    [ApiController]
    [Route("indexes")]
    public class IndexesController : ControllerBase
    {
        private readonly ILogger<IndexesController> _logger;
        private readonly AISearchHelper _aiSearchHelper;
        private readonly Configuration _configuration;
        private readonly SearchIndexClient _indexClient;
        private readonly TokenCredential _credential;
        private readonly AzureOpenAIClient _azureOpenAIClient;
        private readonly string _indexVersion;
        private readonly Azure.Search.Documents.SearchClient _searchClient;

        public IndexesController(ILogger<IndexesController> logger, Configuration configuration, TokenCredential credential)
        {
            _logger = logger;
            _credential = credential ?? throw new ArgumentNullException(nameof(credential));
            _configuration = configuration;
            _aiSearchHelper = new AISearchHelper();
            // Initialize clients
            _azureOpenAIClient = _aiSearchHelper.InitializeOpenAIClient(configuration, _credential);
            _indexClient = _aiSearchHelper.InitializeSearchIndexClient(configuration, _credential);
            _searchClient = _indexClient.GetSearchClient(configuration.IndexName);
            _indexVersion = configuration.IndexVersion ?? "V1";
        }

        // GET: api/indexes
        [SwaggerOperation(
           Summary = "Gets all Indexes",
           Description = "Returns a list of AI Search Indexes"
        )]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetIndexes()
        {
            try
            {
                // TODO: Implement logic to list all indexes
                // This could use SearchIndexClient to list available indexes
                var list = await _aiSearchHelper.GetIndexesAsync(_indexClient);
                await Task.Delay(100);
                return Ok(list); // Placeholder
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [SwaggerOperation(
            Summary = "Create a new capabilities index",
            Description = "Creates a new Azure AI Search index with capabilities schema")]
        [HttpPost("{indexName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateIndex(string indexName)
        {
            try
            {
                await _aiSearchHelper.SetupCapabilitiesIndexAsync(_configuration, _indexClient, indexName);
                return CreatedAtAction(nameof(GetIndexDetails), new { indexName }, indexName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("embeddings")]
        [SwaggerOperation(
                Summary = "Generate embeddings for capabilities",
                Description = "Generates and stores embeddings for all capabilities in the search index")]
                    [ProducesResponseType(StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateCapabilityEmbeddings(
                [FromServices] ICosmosDbService cosmosDbService,
                [FromQuery][Required] string indexName)
        {
            try
            {
                var searchClient = _indexClient.GetSearchClient(indexName);
                await _aiSearchHelper.GenerateCapabilityEmbeddingsForSearchAsync(
                    _configuration,
                    _azureOpenAIClient,
                    cosmosDbService,
                    searchClient);

                return Ok("Successfully generated and stored embeddings for capabilities");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        /// <summary>
        /// This is the function / operation that is used to search for capabilities
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="request"></param>
        /// <returns>List&lt;Capability&gt;</returns>
        [HttpPost("capabilities/search")]
        [SwaggerOperation(
            Summary = "Search capabilities index",
            Description = "Performs hybrid search against capabilities index including vector, text, and semantic search")]
        [ProducesResponseType(typeof(List<Capability>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchCapabilities(
            [FromQuery][Required] string indexName,
            [FromBody] SearchCapabilityRequest request)
        {
            try
            {
                // Need to check which veriosn of the Index is being used.

                if (_indexVersion == "V1") {
                    _logger.LogInformation("V1 Index is being used!!!");
                    var searchClientV1 = _indexClient.GetSearchClient(indexName);
                    var resultsV1 = await _aiSearchHelper.SearchV1(
                        searchClientV1,
                        request.Query,
                        request.K,
                        request.Top,
                        request.Filter,
                        request.TextOnly,
                        request.Hybrid,
                        request.Semantic);
                    return Ok(resultsV1);
                }
                // if it's not a V1 index then perform the search against the V2 index
                _logger.LogInformation("V2 Index is being used!!!");
                var searchClientV2 = _indexClient.GetSearchClient(indexName);
                var resultsV2 = await _aiSearchHelper.SearchCapabilities(
                        searchClientV2,
                        request.Query,
                        request.K,
                        request.Top,
                        request.Filter,
                        request.TextOnly,
                        request.Hybrid,
                        request.Semantic,
                        request.MinRerankerScore);
                return Ok(resultsV2);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching capabilities: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets detailed information about a search index
        /// </summary>
        /// <param name="indexName">Name of the index to retrieve details for</param>
        /// <returns>Details of the specified search index</returns>
        /// <response code="200">Returns the index details</response>
        /// <response code="400">If indexName is null</response>
        /// <response code="404">If the index is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [SwaggerOperation(
            Summary = "Get index details",
            Description = "Retrieves detailed information about a specific Azure AI Search index")]
        [HttpGet("details")]
        [ProducesResponseType(typeof(SearchIndexDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetIndexDetails([FromQuery][Required] string indexName)
        {
            try
            {
                // TODO: Implement logic to get specific index details
                var indexname = indexName ?? throw new ArgumentNullException(nameof(indexName));
                var details = await _aiSearchHelper.GetIndexDetailsAsync(indexname, _indexClient);
                return Ok(details); // Placeholder
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [SwaggerOperation(
            Summary = "Get index statistics",
            Description = "Returns statistics for an indxe given an indexName")]
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(IndexStatistics), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetIndexStatistics([FromQuery][Required] string indexName)
        {
            try
            {
                var stats = await _aiSearchHelper.GetIndexStatisticsAsync(indexName, _indexClient);
                return Ok(stats);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return NotFound($"Index '{indexName}' not found");
            }
        }

        // DELETE: a/indexes/{indexName}
        [SwaggerOperation(
            Summary = "Delete an index using the Index Name",
            Description = "Returns a 204 if the Index was deleted."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete]
        public async Task<IActionResult> DeleteIndex([FromQuery] string? indexName = null)
        {
            try
            {
                var indexname = indexName ?? throw new ArgumentNullException(nameof(indexName));
                await _aiSearchHelper.DeleteIndexAsync(indexname, _indexClient);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lists documents in a search index
        /// </summary>
        [SwaggerOperation(
            Summary = "Lists documents for an index",
            Description = "Returns a list of documents for an index")]
        [HttpGet("v1/documents")]
        [ProducesResponseType(typeof(List<SearchDocument>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ListDocuments(
           [FromQuery][Required] string indexName,
           [FromQuery] bool surpressVectorFields = true,
           [FromQuery] int maxResults = 1000)
        {
            try
            {
                var documents = await _aiSearchHelper.ListDocumentsAsync(indexName, _searchClient, _indexClient, surpressVectorFields, maxResults);
                return Ok(documents);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return NotFound($"Index '{indexName}' not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [SwaggerOperation(
           Summary = "Lists documents for an index",
           Description = "Returns a list of capabilitiy documents for an index V2 intended to be used with the new index")]
        [HttpGet("v2/documents")]
        [ProducesResponseType(typeof(List<SearchDocument>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ListCapabilityDocuments(
          [FromQuery][Required] string indexName,
          [FromQuery] bool surpressVectorFields = true,
          [FromQuery] int maxResults = 1000)
        {
            try
            {
                var documents = await _aiSearchHelper.ListCapabilityDocumentsAsync(indexName, _searchClient, _indexClient, surpressVectorFields, maxResults);
                return Ok(documents);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return NotFound($"Index '{indexName}' not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [SwaggerOperation(
            Summary = "Upload or update capability documents",
            Description = "Uploads new capabilities or updates existing ones in the specified index")]
        [HttpPost("capabilities")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpsertCapabilities(
            [FromQuery][Required] string indexName,
            [FromBody] List<Capability> capabilities)
        {
            try
            {
                var searchClient = _indexClient.GetSearchClient(indexName);
                await _aiSearchHelper.UpsertCapabilityDocumentsAsync(indexName, capabilities, searchClient, _azureOpenAIClient, _configuration);
                return Ok(new { message = $"Successfully uploaded/updated {capabilities.Count} capabilities" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return NotFound($"Index '{indexName}' not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    // DTOs for request/response models
    public class IndexCreateModel
    {
        // Add properties needed for index creation
        public string? Name { get; set; }
        // Add other configuration properties
    }

    public class IndexUpdateModel
    {
        // Add properties needed for index updates
        public string? Name { get; set; }
        // Add other configuration properties
    }
}
