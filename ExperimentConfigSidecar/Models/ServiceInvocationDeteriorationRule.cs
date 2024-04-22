namespace ExperimentConfigSidecar.Models
{
    public class ServiceInvocationDeteriorationRule
    {
        public string? Path { get; set; }
        public double? DelayProbability { get; set; }
        public int? Delay { get; set; }
        public double? ErrorProbability { get; set; }
        public int ErrorCode { get; set; }
    }
}