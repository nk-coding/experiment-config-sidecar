using System.Text.Json;

namespace ExperimentConfigSidecar.Models
{
    public record VariableDefinition(JsonElement Type, JsonElement DefaultValue);
}