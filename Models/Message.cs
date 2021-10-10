using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlexDashWebSocket.Models
{
    internal class Message
    {
        [JsonPropertyName("topic")]
        public string Topic { get; set; }
        [JsonPropertyName("payload")]
        public JsonElement Payload { get; set; }
    }
}