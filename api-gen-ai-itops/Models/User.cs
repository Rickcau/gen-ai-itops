using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace api_gen_ai_itops.Models
{
    public class UserInfo
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
    }

    public class UserPreferences
    {
        [JsonPropertyName("theme")]
        public string Theme { get; set; }
    }

    public class User
    {
        [JsonIgnore]
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("userInfo")]
        public UserInfo UserInfo { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("tier")]
        public string Tier { get; set; }

        [JsonPropertyName("mockMode")]
        public bool MockMode { get; set; }

        [JsonPropertyName("preferences")]
        public UserPreferences Preferences { get; set; }
    }
}
