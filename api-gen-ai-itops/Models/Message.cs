using System.Text.Json.Serialization;

namespace api_gen_ai_itops.Models
{
    public record Message
    {
        /// <summary>
        /// Unique identifier
        /// </summary> 
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Partition key
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }

        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp { get; set; }

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; }

        [JsonPropertyName("sender")]
        public string Sender { get; set; }

        [JsonPropertyName("promptTokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion")]
        public string Completion { get; set; }

        [JsonPropertyName("completionTokens")]
        public int CompletionTokens { get; set; }
    }
}
