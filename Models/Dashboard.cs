using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlexDashWebSocket.Models
{
    internal class Dashboard
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("tabs")]
        public string[] Tabs { get; set; }
    }
}