namespace ExperimentConfigSidecar.Models
{
    public record PubsubDetertiorationRule
    {
        public required double? DelayProbability { get; set; }
        public required int Delay { get; set; }
        public required double? ErrorProbability { get; set; }
    }
}