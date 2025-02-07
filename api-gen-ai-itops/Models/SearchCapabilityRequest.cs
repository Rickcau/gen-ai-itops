using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace api_gen_ai_itops.Models
{
    /// <summary>
    /// Request model for capability search
    /// </summary>
    /// <example>
    /// {
    ///   "query": "sample query",
    ///   "k": 3,
    ///   "top": 10,
    ///   "filter": null,
    ///   "textOnly": false,
    ///   "hybrid": true,
    ///   "semantic": false,
    ///   "minRerankerScore": 2.0
    /// }
    /// </example>
    public class SearchCapabilityRequest
    {
        [DefaultValue("sample query")]
        public string Query { get; set; } = "sample query";

        [DefaultValue(3)]
        public int K { get; set; } = 3;

        [DefaultValue(10)]
        public int Top { get; set; } = 10;

        [DefaultValue(null)]
        public string? Filter { get; set; } = null;

        [DefaultValue(false)]
        public bool TextOnly { get; set; } = false;

        [DefaultValue(true)]
        public bool Hybrid { get; set; } = true;

        [DefaultValue(false)]
        public bool Semantic { get; set; } = false;

        [DefaultValue(2.0)]
        public double MinRerankerScore { get; set; } = 2.0;
    }

}
