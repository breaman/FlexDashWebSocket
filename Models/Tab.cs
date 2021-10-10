using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlexDashWebSocket.Models
{
    internal class Tab
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("Title")]
        public string Title { get; set; }
        [JsonPropertyName("icon")]
        public string Icon { get; set; }
        [JsonPropertyName("grids")]
        public string[] Grids { get; set; }
    }
}