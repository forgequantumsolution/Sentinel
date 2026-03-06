using System.Collections.Generic;

namespace Core.Models
{
    public class GraphData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<GraphDataset> Datasets { get; set; } = new List<GraphDataset>();
    }

    public class GraphDataset
    {
        public string Label { get; set; } = string.Empty;
        public List<object?> Data { get; set; } = new List<object?>();
        public string? BackgroundColor { get; set; }
        public string? BorderColor { get; set; }
        public string? Type { get; set; } // useful for mixed charts (e.g., line and bar together)
    }
}
