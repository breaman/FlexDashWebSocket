using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlexDashWebSocket.Models
{
    internal class Grid
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("widgets")]
        public string[] Widgets { get; set; }
    }
}