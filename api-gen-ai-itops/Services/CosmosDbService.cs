using api_gen_ai_itops.Interfaces;
using api_gen_ai_itops.Models;
using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Azure.Identity;


namespace api_gen_ai_itops.Services
{
    public class CosmosDbService : ICosmosDbService
    {
        private readonly Container _chatHistoryContainer;
        private readonly Container _usersContainer;
        private readonly Container _capibilitiesContainer;

        public CosmosDbService(IOptions<CosmosDbOptions> options)
            //CosmosClient cosmosClient,
            //string databaseName,
            //string containerName)
        {
            CosmosClient cosmosClient = new(
                accountEndpoint: options.Value.AccountUri,
                tokenCredential: new DefaultAzureCredential(
                new DefaultAzureCredentialOptions
                   {
                       TenantId = options.Value.TenantId,
                       ExcludeEnvironmentCredential = true
                   })
            );
            _chatHistoryContainer = cosmosClient.GetContainer(options.Value.DatabaseName, options.Value.ChatHistoryContainerName);
            _usersContainer = cosmosClient.GetContainer(options.Value.DatabaseName, options.Value.UsersContainerName);
            _capibilitiesContainer = cosmosClient.GetContainer(options.Value.DatabaseName, options.Value.CapabilitiesContainerName);
        }

        /// <summary>
        /// This Function is used to query the Capabilities Container for Capabilities
        /// </summary>
        /// <param name="query"></param>
        /// <returns>IEnumerable for Capability</returns>
        public async Task<IEnumerable<Capability>> GetCapabilitiesAsync(string? query = null)
        {
            var queryDefinition = string.IsNullOrEmpty(query)
                ? new QueryDefinition("SELECT * FROM c")
                : new QueryDefinition(query);

            var capabilities = new List<Capability>();
            var iterator = _capibilitiesContainer.GetItemQueryIterator<Capability>(queryDefinition);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                capabilities.AddRange(response);
            }

            return capabilities;
        }

        /// <summary>
        /// This Function is used to query the Capabilities Container for Capabilities by Id
        /// </summary>
        /// <param name="query"></param>
        /// <returns>Returns a specific Capability</returns>
        public async Task<Capability?> GetCapabilityAsync(string id)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                    .WithParameter("@id", id);

                var iterator = _capibilitiesContainer.GetItemQueryIterator<Capability>(query);
                var results = await iterator.ReadNextAsync();

                return results.FirstOrDefault();
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        /// <summary>
        /// This Function is used to Create a new the Capability in the Container 
        /// </summary>
        /// <param name="capability"></param>
        /// <returns></returns>
        public async Task CreateCapabilityAsync(Capability capability)
        {
            if (string.IsNullOrEmpty(capability.CapabilityType))
                throw new ArgumentException("CapabilityType cannot be null or empty as it is the partition key");

            await _capibilitiesContainer.CreateItemAsync(
                capability,
                new PartitionKey(capability.CapabilityType)
            );
        }

        /// <summary>
        /// This Function is used to Update a Capablity by Id 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="capability"></param>
        /// <returns></returns>
        public async Task UpdateCapabilityAsync(string id, Capability capability)
        {
            if (string.IsNullOrEmpty(capability.CapabilityType))
                throw new ArgumentException("CapabilityType cannot be null or empty as it is the partition key");

            await _capibilitiesContainer.UpsertItemAsync(
                capability,
                new PartitionKey(capability.CapabilityType)
            );
        }

        /// <summary>
        /// This Function is used to Delete a Capablity by Id 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteCapabilityAsync(string id)
        {
            // First, we need to get the capability to know its capabilityType
            var capability = await GetCapabilityAsync(id);
            if (capability == null)
                throw new ArgumentException($"Capability with id {id} not found");

            await _capibilitiesContainer.DeleteItemAsync<Capability>(
                id,
                new PartitionKey(capability.CapabilityType)
            );
        }

        // TBD:  Need to create the same types of functions for both the Users and ChatHistory Container.
    }
}
