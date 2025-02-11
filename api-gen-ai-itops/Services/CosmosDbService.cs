using api_gen_ai_itops.Interfaces;
using api_gen_ai_itops.Models;
using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Azure.Identity;
using System.Net;
using User = api_gen_ai_itops.Models.User;
using Session = api_gen_ai_itops.Models.Session;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;


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
            var clientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                    IgnoreNullValues = true
                }
            };
            CosmosClient cosmosClient = new(
                accountEndpoint: options.Value.AccountUri,
                tokenCredential: new DefaultAzureCredential(
                new DefaultAzureCredentialOptions
                   {
                       TenantId = options.Value.TenantId,
                       ExcludeEnvironmentCredential = true
                   }
                ),
                clientOptions: clientOptions
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
        /// <param name="id"></param>
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
        public async Task<bool> CreateCapabilityAsync(Capability capability)
        {
            try
            {
                if (string.IsNullOrEmpty(capability.CapabilityType))
                    throw new ArgumentException("CapabilityType cannot be null or empty as it is the partition key");

                // Check if capability with same ID already exists
                var existingCapability = await GetCapabilityAsync(capability.Id);
                if (existingCapability != null)
                {
                    return false;
                }

                await _capibilitiesContainer.CreateItemAsync(
                    capability,
                    new PartitionKey(capability.CapabilityType)
                );

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                return false;
            }
        }

        /// <summary>
        /// This Function is used to Update a Capablity by Id 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="capability"></param>
        /// <returns></returns>
        public async Task<bool> UpdateCapabilityAsync(string id, Capability capability)
        {
            try
            {
                if (string.IsNullOrEmpty(capability.CapabilityType))
                    throw new ArgumentException("CapabilityType cannot be null or empty as it is the partition key");

                // Verify the capability exists before updating
                var existingCapability = await GetCapabilityAsync(id);
                if (existingCapability == null)
                {
                    return false;
                }

                await _capibilitiesContainer.UpsertItemAsync(
                    capability,
                    new PartitionKey(capability.CapabilityType)
                );

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        /// <summary>
        /// This Function is used to Delete a Capablity by Id 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public async Task<bool> DeleteCapabilityAsync(string id)
        {
            try
            {
                var capability = await GetCapabilityAsync(id);
                if (capability == null)
                {
                    return false;
                }

                await _capibilitiesContainer.DeleteItemAsync<Capability>(
                    id,
                    new PartitionKey(capability.CapabilityType)
                );

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        // TBD:  Need to create the same types of functions for both the Users and ChatHistory Container.

        // Get all users or query users
        public async Task<IEnumerable<User>> GetUsersAsync(string? query = null)
        {
            var queryDefinition = string.IsNullOrEmpty(query)
                ? new QueryDefinition("SELECT * FROM c")
                : new QueryDefinition(query);

            var users = new List<User>();
            var iterator = _usersContainer.GetItemQueryIterator<User>(queryDefinition);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                users.AddRange(response);
            }

            return users;
        }

        // Get single user
        public async Task<User?> GetUserAsync(string id)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                    .WithParameter("@id", id);

                var iterator = _usersContainer.GetItemQueryIterator<User>(query);
                var results = await iterator.ReadNextAsync();

                return results.FirstOrDefault();
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        // Create user
        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Role))
                    throw new ArgumentException("Role cannot be null or empty as it is the partition key");

                var existingUser = await GetUserAsync(user.Id);
                if (existingUser != null)
                {
                    return false;
                }

                await _usersContainer.CreateItemAsync(
                    user,
                    new PartitionKey(user.UserInfo.Email)
                );

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                return false;
            }
        }

        // Update user
        public async Task<bool> UpdateUserAsync(string id, User user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Role))
                    throw new ArgumentException("Role cannot be null or empty as it is the partition key");

                var existingUser = await GetUserAsync(id);
                if (existingUser == null)
                {
                    return false;
                }

                await _usersContainer.UpsertItemAsync(
                    user,
                    new PartitionKey(user.UserInfo.Email)
                );

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        // Delete user
        public async Task<bool> DeleteUserAsync(string id)
        {
            try
            {
                var user = await GetUserAsync(id);
                if (user == null)
                {
                    return false;
                }

                await _usersContainer.DeleteItemAsync<User>(
                    id,
                    new PartitionKey(user.UserInfo.Email)
                );

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        // ChatHistory Data Functions
        /// <summary>
        /// Creates a new chat session.
        /// </summary>
        /// <param name="session">Chat session item to create.</param>
        /// <returns>Newly created chat session item.</returns>
        public async Task<Session> InsertSessionAsync(Session session)
        {
            PartitionKey partitionKey = new(session.SessionId);
            return await _chatHistoryContainer.CreateItemAsync<Session>(
                item: session,
                partitionKey: partitionKey
            );
        }

        /// <summary>
        /// Creates a new chat message.
        /// </summary>
        /// <param name="message">Chat message item to create.</param>
        /// <returns>Newly created chat message item.</returns>
        public async Task<Message> InsertMessageAsync(Message message)
        {
            PartitionKey partitionKey = new(message.SessionId);
            Message newMessage = message with { TimeStamp = DateTime.UtcNow };
            return await _chatHistoryContainer.CreateItemAsync<Message>(
                item: message,
                partitionKey: partitionKey
            );
        }

       
        public async Task<bool> SessionExists(string sessionId)
        {
            QueryDefinition query = new QueryDefinition("SELECT * FROM c WHERE c.sessionId = @sessionId and c.type = 'session'")
                .WithParameter("@sessionId", sessionId);

            FeedIterator<Session> response = _chatHistoryContainer.GetItemQueryIterator<Session>(query);

            while (response.HasMoreResults)  // Use while instead of if to ensure we check all results
            {
                FeedResponse<Session> results = await response.ReadNextAsync();
                if (results.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a list of all current chat sessions.
        /// </summary>
        /// <returns>List of distinct chat session items.</returns>
        public async Task<List<Session>> GetSessionsAsync(string? query = null, string? userId = null)
        {
            var queryDefinition = string.IsNullOrEmpty(query)
                ? new QueryDefinition("SELECT DISTINCT * FROM c WHERE c.type = 'session'")
                : new QueryDefinition(query)
                    .WithParameter("@userId", userId);

            FeedIterator<Session> response = _chatHistoryContainer.GetItemQueryIterator<Session>(queryDefinition);
            List<Session> output = new();
            while (response.HasMoreResults)
            {
                FeedResponse<Session> results = await response.ReadNextAsync();
                output.AddRange(results);
            }
            return output;
        }

        /// <summary>
        /// Gets a list of all current chat messages for a specified session identifier.
        /// </summary>
        /// <param name="sessionId">Chat session identifier used to filter messsages.</param>
        /// <returns>List of chat message items for the specified session.</returns>
        public async Task<List<Message>> GetSessionMessagesAsync(string sessionId)
        {
            // Add logging to debug the query parameters
            Console.WriteLine($"Querying messages for SessionId: {sessionId}");
            QueryDefinition query = new QueryDefinition("SELECT * FROM c WHERE c.sessionId = @sessionId AND c.type = @type")
                .WithParameter("@sessionId", sessionId)
                .WithParameter("@type", "message");

            Console.WriteLine($"Query: {query.QueryText}");

            FeedIterator<Message> results = _chatHistoryContainer.GetItemQueryIterator<Message>(query);

            List<Message> output = new();
            while (results.HasMoreResults)
            {
                FeedResponse<Message> response = await results.ReadNextAsync();
                Console.WriteLine($"Retreived {response.Count} messages in this batch");
                output.AddRange(response);
            }
            // Log final count
            Console.WriteLine($"Total messages retrieved: {output.Count}");
            return output;
        }

        /// <summary>
        /// Updates an existing chat session.
        /// </summary>
        /// <param name="session">Chat session item to update.</param>
        /// <returns>Revised created chat session item.</returns>
        public async Task<Session> UpdateSessionAsync(Session session)
        {
            PartitionKey partitionKey = new(session.SessionId);
            return await _chatHistoryContainer.ReplaceItemAsync(
                item: session,
                id: session.Id,
                partitionKey: partitionKey
            );
        }

        /// <summary>
        /// Returns an existing chat session.
        /// </summary>
        /// <param name="sessionId">Chat session id for the session to return.</param>
        /// <returns>Chat session item.</returns>
        public async Task<Session> GetSessionAsync(string sessionId)
        {
            PartitionKey partitionKey = new(sessionId);
            return await _chatHistoryContainer.ReadItemAsync<Session>(
                partitionKey: partitionKey,
                id: sessionId
                );
        }

        /// <summary>
        /// Batch create chat message and update session.
        /// </summary>
        /// <param name="messages">Chat message and session items to create or replace.</param>
        public async Task UpsertSessionBatchAsync(params dynamic[] messages)
        {

            //Make sure items are all in the same partition
            if (messages.Select(m => m.SessionId).Distinct().Count() > 1)
            {
                throw new ArgumentException("All items must have the same partition key.");
            }

            PartitionKey partitionKey = new(messages[0].SessionId);
            TransactionalBatch batch = _chatHistoryContainer.CreateTransactionalBatch(partitionKey);

            foreach (var message in messages)
            {
                batch.UpsertItem(item: message);
            }

            await batch.ExecuteAsync();
        }

        /// <summary>
        /// Batch deletes an existing chat session and all related messages.
        /// </summary>
        /// <param name="sessionId">Chat session identifier used to flag messages and sessions for deletion.</param>
        public async Task DeleteSessionAndMessagesAsync(string sessionId)
        {
            PartitionKey partitionKey = new(sessionId);

            QueryDefinition query = new QueryDefinition("SELECT VALUE c.id FROM c WHERE c.sessionId = @sessionId")
                    .WithParameter("@sessionId", sessionId);

            FeedIterator<string> response = _chatHistoryContainer.GetItemQueryIterator<string>(query);

            TransactionalBatch batch = _chatHistoryContainer.CreateTransactionalBatch(partitionKey);
            while (response.HasMoreResults)
            {
                FeedResponse<string> results = await response.ReadNextAsync();
                foreach (var itemId in results)
                {
                    batch.DeleteItem(
                        id: itemId
                    );
                }
            }
            await batch.ExecuteAsync();
        }

        /// <summary>
        /// Deletes all sessions and their associated messages from the chat history container.
        /// This operation performs a bulk delete using transactional batches grouped by session partitions.
        /// </summary>
        /// <remarks>
        /// The deletion process:
        /// 1. Queries all items of type 'session' and 'message'
        /// 2. Groups items by their partition key (sessionId)
        /// 3. Executes transactional batch deletions for each partition
        /// </remarks>
        /// <returns>
        /// A task that represents the asynchronous delete operation.
        /// </returns>
        /// <exception cref="CosmosException">
        /// Thrown when there's an issue with the Cosmos DB operation:
        /// - Status 429: Rate limiting/request throttling
        /// - Status 503: Service unavailable
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when an unexpected error occurs during the deletion process.
        /// </exception>

        public async Task DeleteAllSessionsAndMessagesAsync()
        {
            // Query to get all items (both sessions and messages)
            QueryDefinition query = new QueryDefinition("SELECT * FROM c WHERE c.type IN ('session', 'message')");

            // Get all items that need to be deleted
            FeedIterator<dynamic> response = _chatHistoryContainer.GetItemQueryIterator<dynamic>(query);

            // Create a dictionary to group items by partition key (sessionId)
            var itemsByPartition = new Dictionary<string, List<(string id, string sessionId)>>();

            // Collect all items grouped by their partition key
            while (response.HasMoreResults)
            {
                FeedResponse<dynamic> results = await response.ReadNextAsync();
                foreach (var item in results)
                {
                    string sessionId = item.sessionId.ToString();
                    string id = item.id.ToString();

                    if (!itemsByPartition.ContainsKey(sessionId))
                    {
                        itemsByPartition[sessionId] = new List<(string, string)>();
                    }
                    itemsByPartition[sessionId].Add((id, sessionId));
                }
            }

            // Delete all items in batches, partition by partition
            foreach (var partition in itemsByPartition)
            {
                string sessionId = partition.Key;
                var items = partition.Value;

                TransactionalBatch batch = _chatHistoryContainer.CreateTransactionalBatch(new PartitionKey(sessionId));

                foreach (var (id, _) in items)
                {
                    batch.DeleteItem(id);
                }

                await batch.ExecuteAsync();
            }
        }

    }

}

