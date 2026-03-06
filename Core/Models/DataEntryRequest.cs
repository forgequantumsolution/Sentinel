using System.Collections.Generic;

namespace Core.Models
{
    public class DataEntryRequest
    {
        public string TableName { get; set; } = string.Empty;
        
        // Key is the Column Name, Value is the data to insert
        // The frontend dynamically generates fields, mapping them to these Keys
        public Dictionary<string, object?> Data { get; set; } = new Dictionary<string, object?>();
    }
}
