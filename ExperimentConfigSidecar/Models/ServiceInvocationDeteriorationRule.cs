namespace ExperimentConfigSidecar.Models
{
    public record ServiceInvocationDeteriorationRule(string? Path, double? DelayProbability, int? Delay, double? ErrorProbability, int ErrorCode);
}