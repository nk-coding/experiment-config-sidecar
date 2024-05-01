namespace ExperimentConfigSidecar.Models;

/// <summary>
/// Represents the service invocation deterioration configuration.
/// </summary>
/// <param name="Delay">The delay in milliseconds to introduce in the service invocation.</param>
/// <param name="ErrorCode">The error code to return in the service invocation.</param>
public record ServiceInvocationDeterioration(int? Delay, int? ErrorCode);