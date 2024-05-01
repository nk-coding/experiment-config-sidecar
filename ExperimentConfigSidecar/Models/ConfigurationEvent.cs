using System.Text.Json;

namespace ExperimentConfigSidecar.Models;

/// <summary>
/// Event to signal a configuration change.
/// </summary>
public class ConfigurationEvent
{
    /// <summary>
    /// The ID of the replica that triggered the event.
    /// </summary>
    public string ReplicaId { get; set; }

    /// <summary>
    /// The new configurations.
    /// </summary>
    public Dictionary<string, JsonElement> Configurations { get; set; }
}