using Azure.Search.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Runbook.Models
{
    //public class SearchIgnoreAttribute : Attribute { }
    public class RunbookData
    {
        [SimpleField(IsKey = true)]
        [JsonPropertyName("id")]
        public string id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [JsonPropertyName("example")]
        public string? Example { get; set; }

        //[SearchIgnore]
        //public List<RunbookParameter>? Parameters { get; set; } = new List<RunbookParameter>();

        // Store parameters as a JSON string
        [JsonPropertyName("parameters")]
        [JsonConverter(typeof(ParametersJsonConverter))]
        public string? ParametersJson { get; set; }

        //[SimpleField(IsFilterable = false)]
        //[JsonPropertyName("parametersJson")]
        //public string? ParametersJson { get; set; }     

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [SearchableField]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("synopsis")]
        public string? Synopsis { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [SearchableField(IsFilterable = true)]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("lastEdit")]
        public string? LastEdit { get; set; }

        [JsonPropertyName("dependencies")]
        public List<string> Dependencies { get; set; } = new List<string>();

        // Vector field for embeddings
        public IList<float>? DescriptionVector { get; set; }
    }

    public class ParametersJsonConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // If the token is null, return null
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            // Serialize the array or object to a JSON string
            using (var document = JsonDocument.ParseValue(ref reader))
            {
                return document.RootElement.GetRawText();
            }
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                // Remove \r\n characters and extra spaces
                string cleanedValue = value
                    .Replace("\r\n", "")
                    .Replace("\n", "")
                    .Replace("\r", "")  // Remove any remaining carriage returns
                    .Trim()  // Trim leading/trailing spaces
                    .Replace("  ", " "); // Optionally, replace multiple spaces with a single space

                // Optionally, replace multiple spaces with one
                while (cleanedValue.Contains("  ")) // Repeat until no double spaces are left
                {
                    cleanedValue = cleanedValue.Replace("  ", " ");
                }

                // Write the cleaned string
                writer.WriteStringValue(cleanedValue);
            }
        }
    }
    public class RunbookParameter
    {
        [JsonPropertyName("required")]
        public bool Required { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("default")]
        public string? Default { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}




