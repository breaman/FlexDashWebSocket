using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlexDashWebSocket.Models
{
    internal class Widget
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("static")]
        public Static Static { get; set; }
        [JsonPropertyName("dynamic")]
        public Dynamic Dynamic { get; set; }
        [JsonPropertyName("rows")]
        public int Rows { get; set; }
        [JsonPropertyName("cols")]
        public int Cols { get; set; }
    }
}