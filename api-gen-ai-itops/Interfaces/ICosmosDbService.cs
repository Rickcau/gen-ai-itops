using api_gen_ai_itops.Models;
using Microsoft.Azure.Cosmos; 

using System.Text.Json;

namespace api_gen_ai_itops.Interfaces
{
    public interface ICosmosDbService
    {
        Task<IEnumerable<Capability>> GetCapabilitiesAsync(string? query = null);
        Task<Capability?> GetCapabilityAsync(string id);
        Task CreateCapabilityAsync(Capability capability);
        Task UpdateCapabilityAsync(string id, Capability capability);
        Task DeleteCapabilityAsync(string id);
    }
}
