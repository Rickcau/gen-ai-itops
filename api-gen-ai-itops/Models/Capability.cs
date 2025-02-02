using System.Text.Json;
using System.Text.Json.Serialization;

namespace api_gen_ai_itops.Models
{
    public class Capability
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("capabilityType")]
        public string CapabilityType { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("parameters")]
        public List<Parameter> Parameters { get; set; }

        [JsonPropertyName("executionMethod")]
        public ExecutionMethod ExecutionMethod { get; set; }
    }

    public class Parameter
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class ExecutionMethod
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("details")]
        public string Details { get; set; }
    }
}
