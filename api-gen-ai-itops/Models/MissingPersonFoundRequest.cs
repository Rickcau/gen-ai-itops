using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace api_gen_ai_itops.Models
{
    public record MissingPersonFoundRequest
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Age must be a non-negative number.")]
        public int Age { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1753-01-01", "9999-12-31", ErrorMessage = "DateReported must be a valid date and cannot be in the future.")]
        public DateTime DateReported { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1753-01-01", "9999-12-31", ErrorMessage = "DateFound must be a valid date and cannot be in the future.")]
        public DateTime DateFound { get; set; }
    }
}
