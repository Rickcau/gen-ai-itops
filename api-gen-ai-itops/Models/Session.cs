using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace api_gen_ai_itops.Models
{
    public record Session
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

        [JsonPropertyName("userId")]
        public string? UserId { get; set; } = string.Empty;

        [JsonPropertyName("tokens")]
        public int? Tokens { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonIgnore]
        public List<Message> Messages { get; set; }

        public void AddMessage(Message message)
        {
            Messages.Add(message);
        }

        public void UpdateMessage(Message message)
        {
            var match = Messages.Single(m => m.Id == message.Id);
            var index = Messages.IndexOf(match);
            Messages[index] = message;
        }
    }
}
