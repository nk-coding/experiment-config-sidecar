namespace ExperimentConfigSidecar.Models
{
    public class SubscriptionSpec
    {
        public string Topic { get; set; }
        public string Pubsubname { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public string Route { get; set; }
        public BulkSubscribe BulkSubscribe { get; set; }
        public string? DeadLetterTopic { get; set; }
    }

    public class BulkSubscribe
    {
        public bool Enabled { get; set; }
        public int? MaxMessagesCount { get; set; }
        public int? MaxAwaitDurationMs { get; set; }
    }
}