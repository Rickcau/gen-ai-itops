namespace api_gen_ai_itops.Models
{
    public class SearchIndexDetails
    {
        public string Name { get; set; }
        public List<FieldInfo> Fields { get; set; }
        public bool HasVectorSearch { get; set; }
        public bool HasSemanticSearch { get; set; }
        public List<string> Vectorizers { get; set; }
        public List<string> SemanticConfigurations { get; set; }
    }

    public class FieldInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsFilterable { get; set; }
        public bool IsSortable { get; set; }
        public bool IsFacetable { get; set; }
        public bool IsKey { get; set; }
    }
}
