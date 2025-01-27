using api_gen_ai_itops.Interfaces;
using api_gen_ai_itops.Models;
using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos;


namespace api_gen_ai_itops.Services
{
    public class CosmosDbService : ICosmosDbService
    {
        private readonly Container _container;

        public CosmosDbService(
            CosmosClient cosmosClient,
            string databaseName,
            string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<IEnumerable<Capability>> GetCapabilitiesAsync(string? query = null)
        {
            var queryDefinition = string.IsNullOrEmpty(query)
                ? new QueryDefinition("SELECT * FROM c")
                : new QueryDefinition(query);

            var capabilities = new List<Capability>();
            var iterator = _container.GetItemQueryIterator<Capability>(queryDefinition);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                capabilities.AddRange(response);
            }

            return capabilities;
        }

        public async Task<Capability?> GetCapabilityAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<Capability>(
                    id,
                    new PartitionKey(id)
                );
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task CreateCapabilityAsync(Capability capability)
        {
            await _container.CreateItemAsync(
                capability,
                new PartitionKey(capability.Id)
            );
        }

        public async Task UpdateCapabilityAsync(string id, Capability capability)
        {
            await _container.UpsertItemAsync(
                capability,
                new PartitionKey(id)
            );
        }

        public async Task DeleteCapabilityAsync(string id)
        {
            await _container.DeleteItemAsync<Capability>(
                id,
                new PartitionKey(id)
            );
        }
    }
}
