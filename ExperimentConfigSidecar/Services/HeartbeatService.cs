using Dapr.Client;
using ExperimentConfigSidecar.Models;

namespace ExperimentConfigSidecar.Services
{
    public class HeartbeatService(int heartbeatInterval, string pubsubName, Guid instanceId, string serviceName, ILogger logger)
    {
        private readonly int heartbeatInterval = heartbeatInterval;

        private readonly Guid instanceId = instanceId;

        private readonly string serviceName = serviceName;

        private readonly ILogger logger = logger;

        private readonly DaprClient daprClient = new DaprClientBuilder().Build();

        public async Task StartAsync()
        {
            while (true)
            {
                try {
                    await Task.Delay(heartbeatInterval);
                    await daprClient.PublishEventAsync(pubsubName, "heartbeat", new HeartbeatEvent(instanceId, serviceName));
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to send heartbeat");
                }
            }
        }
    }
}