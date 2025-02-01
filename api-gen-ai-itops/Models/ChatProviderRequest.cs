using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace api_gen_ai_itops.Models
{
    public class ChatProviderRequest
    {
        [Required]
        [JsonPropertyName("sessionId")]
        public string? SessionId { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("userId")]
        public string? UserId { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("prompt")]
        public required string Prompt { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("chatName")]
        public required string ChatName { get; set; } = string.Empty;
    }
}
