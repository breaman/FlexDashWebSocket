using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlexDashWebSocket.Models
{
    internal class Dynamic
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}