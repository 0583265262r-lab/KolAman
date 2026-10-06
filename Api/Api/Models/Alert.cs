using System.Text.Json.Serialization;

namespace Api.Models
{
    public class Alert
    {
        public int Id { get; set; }

        [JsonPropertyName("alert_id")]
        public string AlertId { get; set; } = "";

        [JsonPropertyName("source")]
        public string Source { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("content")]
        public string Content { get; set; } = "";

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = "";

        [JsonPropertyName("classification")]
        public string Classification { get; set; } = "";

        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("command")]
        public string Command { get; set; } = "";

        public DateTime ReceivedAt { get; set; }
    }

}
