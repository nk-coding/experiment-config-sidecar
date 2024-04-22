namespace ExperimentConfigSidecar.Models
{
    public record HeartbeatEvent(Guid InstanceId, string ServiceName);
}