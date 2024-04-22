using System.Text.Json;
using ExperimentConfigSidecar.Models;

namespace ExperimentConfigSidecar.Services
{

    public class ConfigService
    {

        public const string PubsubDeteriorationKey = "pubsubDeterioration";

        public const string ServiceInvocationDeteriorationKey = "serviceInvocationDeterioration";

        public const string MemoryLeakKey = "artificialMemoryLeak";

        public const string CPUUsageKey = "artificialCPUUsage";

        private readonly HashSet<string> configPropertyKeys = new([PubsubDeteriorationKey, ServiceInvocationDeteriorationKey, MemoryLeakKey, CPUUsageKey]);

        private List<ServiceInvocationDeteriorationRule> serviceInvocationDeteriorationRules = [];

        private PubsubDetertiorationRule pubsubDetertiorationRule = new()
        {
            DelayProbability = null,
            ErrorProbability = null,
            Delay = 0,
        };

        private long artificialMemoryLeak = 0;

        private int artificialCPUUsage = 0;

        private readonly Random random = new();

        public Dictionary<string, JsonElement> UpdateConfig(Dictionary<string, JsonElement> config)
        {
            UpdateServiceInvocationDeterioration(config);
            UpdatePubsubDeterioration(config);
            UpdateArtificialMemoryLeak(config);
            return config.Where(pair => configPropertyKeys.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private void UpdateArtificialMemoryLeak(Dictionary<string, JsonElement> config)
        {
            if (config.TryGetValue(MemoryLeakKey, out JsonElement value))
            {
                artificialMemoryLeak = value.TryGetInt64(out var leak) ? leak : 0;
            }
        }

        private void UpdatePubsubDeterioration(Dictionary<string, JsonElement> config)
        {
            if (config.TryGetValue(ServiceInvocationDeteriorationKey, out JsonElement value))
            {
                serviceInvocationDeteriorationRules = [];
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
            else
            {
                serviceInvocationDeteriorationRules = [];
            }
        }

        private void UpdateServiceInvocationDeterioration(Dictionary<string, JsonElement> config)
        {
            if (config.TryGetValue(PubsubDeteriorationKey, out JsonElement value))
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
            return new ServiceInvocationDeteriorationRule()
            {
                Path = rule.GetStringProperty("path"),
                DelayProbability = rule.GetDoubleProperty("delayProbability"),
                ErrorProbability = rule.GetDoubleProperty("errorProbability"),
                Delay = rule.GetIntProperty("delay") ?? 0,
                ErrorCode = rule.GetIntProperty("errorCode") ?? 500,
            };
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
    }

}