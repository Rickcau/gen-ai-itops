using System.ComponentModel.DataAnnotations;

namespace api_gen_ai_itops.Models
{
    public class CosmosDbOptions
    {
        public const string CosmosDb = "CosmosDbOptions";

        [Required]
        public string DatabaseName { get; set; }
        [Required]
        public string ChatHistoryContainerName { get; set; }
        [Required]
        public string UsersContainerName { get; set; }
        [Required]
        public string CapabilitiesContainerName { get; set; }
        [Required]
        public string AccountUri { get; set; }
        [Required]
        public string TenantId { get; set; }
    }
}
