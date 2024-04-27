using System.Text.Json;
using ExperimentConfigSidecar.Models;

namespace ExperimentConfigSidecar.Services
{

    public class ConfigService
    {

        public const string PubsubDeteriorationKey = "pubsubDeterioration";

        public const string ServiceInvocationDeteriorationKey = "serviceInvocationDeterioration";

        public const string MemoryUsageKey = "artificialMemoryUsage";

        public const string CPUUsageKey = "artificialCPUUsage";

        private const string PubsubDeteriorationSchema = """
        {
            "$schema": "http://json-schema.org/draft-07/schema#",
            "oneOf": [
                {
                    "type": "object",
                    "properties": {
                        "delayProbability": {"type": "number"},
                        "errorProbability": {"type": "number"},
                        "delay": {"type": "integer"}
                    },
                    "additionalProperties": false
                },
                {
                    "type": "null"
                }
            ]
        }
        """;

        private const string ServiceInvocationDeteriorationSchema = """
        {
            "$schema": "http://json-schema.org/draft-07/schema#",
            "$defs": {
                "item": {
                    "type": "object",
                    "properties": {
                        "path": {"type": "string"},
                        "delayProbability": {"type": "number"},
                        "delay": {"type": "integer"},
                        "errorProbability": {"type": "number"},
                        "errorCode": {"type": "integer"}
                    },
                    "additionalProperties": false
                }
            },
            "oneOf": [
                { "$ref": "#/$defs/item" },
                {
                    "type": "array",
                    "items": { "$ref": "#/$defs/item" }
                },
                {
                    "type": "null"
                }
            ]
        }
        """;

        private const string MemoryUsageSchema = """
        {
            "$schema": "http://json-schema.org/draft-07/schema#",
            "oneOf": [
                {
                    "type": "integer"
                },
                {
                    "type": "null"
                }
            ]
        }
        """;

        private const string CPUUsageSchema = """
        {
            "$schema": "http://json-schema.org/draft-07/schema#",
            "$defs": {
                "item": {
                    "type": "object",
                    "properties": {
                        "usageDuration": {"type": "integer"},
                        "pauseDuration": {"type": "integer"}
                    },
                    "required": ["usageduration", "pauseduration"],
                    "additionalProperties": false
                }
            },
            "oneOf": [
                { "$ref": "#/$defs/item" },
                {
                    "type": "array",
                    "items": { "$ref": "#/$defs/item" }
                },
                {
                    "type": "null"
                }
            ]
        }
        """;

        private readonly HashSet<string> configPropertyKeys = new([PubsubDeteriorationKey, ServiceInvocationDeteriorationKey, MemoryUsageKey, CPUUsageKey]);

        private List<ServiceInvocationDeteriorationRule> serviceInvocationDeteriorationRules = [];

        private PubsubDetertiorationRule pubsubDetertiorationRule = new()
        {
            DelayProbability = null,
            ErrorProbability = null,
            Delay = 0,
        };

        private long artificialMemoryUsage = 0;

        private List<CPUUsage> artificialCPUUsage = [];

        private readonly Random random = new();

        private readonly MemoryUsageService memoryUsageService = new();

        public Dictionary<string, JsonElement> UpdateConfig(Dictionary<string, JsonElement> config)
        {
            UpdatePubsubDeterioration(config);
            UpdateServiceInvocationDeterioration(config);
            UpdateArtificialMemoryUsage(config);
            UpdateArtificialCPUUsage(config);
            return config.Where(pair => !configPropertyKeys.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private void UpdateArtificialMemoryUsage(Dictionary<string, JsonElement> config)
        {
            if (config.TryGetValue(MemoryUsageKey, out JsonElement value))
            {
                artificialMemoryUsage = value.TryGetInt64(out var leak) ? leak : 0;
            }
            memoryUsageService.UpdateMemoryUsage(artificialMemoryUsage);
        }

        private void UpdateArtificialCPUUsage(Dictionary<string, JsonElement> config)
        {
            artificialCPUUsage = [];
            if (config.TryGetValue(CPUUsageKey, out JsonElement value) && !value.IsNull())
            {
                if (value.ValueKind == JsonValueKind.Object)
                {
                    artificialCPUUsage.Add(ParseCPUUsage(value));
                }
                else
                {
                    foreach (var usage in value.EnumerateArray())
                    {
                        artificialCPUUsage.Add(ParseCPUUsage(usage));
                    }
                }
            }
        }

        private void UpdateServiceInvocationDeterioration(Dictionary<string, JsonElement> config)
        {
            serviceInvocationDeteriorationRules = [];
            if (config.TryGetValue(ServiceInvocationDeteriorationKey, out JsonElement value) && !value.IsNull())
            {
                if (value.ValueKind == JsonValueKind.Object)
                {
                    serviceInvocationDeteriorationRules.Add(ParseServiceInvocationDeteriorationRule(value));
                }
                else
                {
                    foreach (var rule in value.EnumerateArray())
                    {
                        serviceInvocationDeteriorationRules.Add(ParseServiceInvocationDeteriorationRule(rule));
                    }
                }
            }
        }

        private void UpdatePubsubDeterioration(Dictionary<string, JsonElement> config)
        {
            if (config.TryGetValue(PubsubDeteriorationKey, out JsonElement value) && !value.IsNull())
            {
                pubsubDetertiorationRule = new()
                {
                    DelayProbability = value.GetDoubleProperty("delayProbability"),
                    ErrorProbability = value.GetDoubleProperty("errorProbability"),
                    Delay = value.GetIntProperty("delay") ?? 0,
                };
            }
            else
            {
                pubsubDetertiorationRule = new()
                {
                    DelayProbability = null,
                    ErrorProbability = null,
                    Delay = 0,
                };
            }
        }

        private ServiceInvocationDeteriorationRule ParseServiceInvocationDeteriorationRule(JsonElement rule)
        {
            return new ServiceInvocationDeteriorationRule
            (
                rule.GetStringProperty("path"),
                rule.GetDoubleProperty("delayProbability"),
                rule.GetIntProperty("delay") ?? 0,
                rule.GetDoubleProperty("errorProbability"),
                rule.GetIntProperty("errorCode") ?? 500
            );
        }

        private CPUUsage ParseCPUUsage(JsonElement usage)
        {
            return new CPUUsage
            (
                usage.GetIntProperty("usageDuration") ?? 0,
                usage.GetIntProperty("pauseDuration") ?? 0
            );
        }

        public PubsubDetertioration GetPubsubDeterioration()
        {
            return new PubsubDetertioration()
            {
                Error = random.NextDouble() < pubsubDetertiorationRule.ErrorProbability,
                Delay = random.NextDouble() < pubsubDetertiorationRule.DelayProbability ? pubsubDetertiorationRule.Delay : null,
            };
        }

        public ServiceInvocationDeterioration GetServiceInvocationDeterioration(string path)
        {
            foreach (var rule in serviceInvocationDeteriorationRules)
            {
                if (rule.Path == null || path.StartsWith(rule.Path))
                {
                    return new ServiceInvocationDeterioration()
                    {
                        Delay = random.NextDouble() < rule.DelayProbability ? rule.Delay : null,
                        ErrorCode = random.NextDouble() < rule.ErrorProbability ? rule.ErrorCode : null,
                    };
                }
            }
            return new ServiceInvocationDeterioration();
        }

        public void AddVariableDefinitions(Dictionary<string, VariableDefinition> existingDefinitions)
        {
            existingDefinitions.Add(PubsubDeteriorationKey, new VariableDefinition(PubsubDeteriorationSchema.AsJsonElement(), "null".AsJsonElement()));
            existingDefinitions.Add(ServiceInvocationDeteriorationKey, new VariableDefinition(ServiceInvocationDeteriorationSchema.AsJsonElement(), "null".AsJsonElement()));
            existingDefinitions.Add(MemoryUsageKey, new VariableDefinition(MemoryUsageSchema.AsJsonElement(), "null".AsJsonElement()));
            existingDefinitions.Add(CPUUsageKey, new VariableDefinition(CPUUsageSchema.AsJsonElement(), "null".AsJsonElement()));
        }
    }

}