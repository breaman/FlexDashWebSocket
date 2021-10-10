using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlexDashWebSocket.Models
{
    internal class Static
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("unit")]
        public string Unit { get; set; }
    }
}