using System.Text.Json.Serialization;

namespace api_gen_ai_itops.Models
{
    public class CapabilityDetails
    {
        [JsonPropertyName("id")]
        public string id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("parameters")]
        public string? Parameters { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("rerankerscore")]
        public double RerankerScore { get; set; }

    }
}
