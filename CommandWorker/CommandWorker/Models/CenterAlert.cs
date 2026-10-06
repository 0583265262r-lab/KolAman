namespace CommandWorker.Models
{
    public class CenterAlert
    {
        public int Id { get; set; }
        public string AlertId { get; set; } = "";
        public string Source { get; set; } = "";
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string Priority { get; set; } = "";
        public string Classification { get; set; } = "";
        public double Lat { get; set; }
        public double Lon { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = "";
        public string Command { get; set; } = "";
        public DateTime ReceivedAt { get; set; }
    }
}
