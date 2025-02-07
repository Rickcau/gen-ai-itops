using api_gen_ai_itops.Models;
using Microsoft.Azure.Cosmos; 

using System.Text.Json;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using User = api_gen_ai_itops.Models.User;
using Session = api_gen_ai_itops.Models.Session;

namespace api_gen_ai_itops.Interfaces
{
    public interface ICosmosDbService
    {
        // Capability methods
        Task<IEnumerable<Capability>> GetCapabilitiesAsync(string? query = null);
        Task<Capability?> GetCapabilityAsync(string id);
        Task<bool> CreateCapabilityAsync(Capability capability);
        Task<bool> UpdateCapabilityAsync(string id, Capability capability);
        Task<bool> DeleteCapabilityAsync(string id);

        // User Operations
        Task<IEnumerable<User>> GetUsersAsync(string? query = null);
        Task<User?> GetUserAsync(string id);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(string id, User user);
        Task<bool> DeleteUserAsync(string id);

        // ChatHistory methods
        Task<Session> InsertSessionAsync(Session session);
        Task<Message> InsertMessageAsync(Message message);
        Task<List<Session>> GetSessionsAsync(string? query = null, string? userId = null);
        Task<List<Message>> GetSessionMessagesAsync(string sessionId);
        Task<Session> UpdateSessionAsync(Session session);
        Task<bool> SessionExists(string sessionId);
        Task<Session> GetSessionAsync(string sessionId);
        Task UpsertSessionBatchAsync(params dynamic[] messages);
        Task DeleteSessionAndMessagesAsync(string sessionId);
        Task DeleteAllSessionsAndMessagesAsync();

    }
}
