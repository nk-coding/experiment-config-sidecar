namespace ExperimentConfigSidecar.Models;

/// <summary>
/// Event to signal a heartbeat.
/// </summary>
/// <param name="InstanceId">The replica ID.</param>
/// <param name="ServiceName">The service name.</param>
public record HeartbeatEvent(Guid InstanceId, string ServiceName);