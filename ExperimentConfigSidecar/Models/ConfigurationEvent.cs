using System.Text.Json;

namespace ExperimentConfigSidecar.Models
{
    public class ConfigurationEvent
    {
        public string ReplicaId { get; set; }

        public Dictionary<string, JsonElement> Configurations { get; set; }
    }
}