using System.Text.Json;
using ExperimentConfigSidecar.Models;
using ExperimentConfigSidecar.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();
var httpClient = new HttpClient();
var logger = app.Logger;
var configService = new ConfigService();

var appPort = int.Parse(app.Configuration["APP_PORT"] ?? "8080");
var serviceName = app.Configuration["SERVICE_NAME"] ?? "missing-service-name";
var heartbeatInterval = int.Parse(app.Configuration["HEARTBEAT_INTERVAL"] ?? "1000");

var appUrl = $"http://localhost:{appPort}";
var replikaId = Guid.NewGuid();
const string pubsubName = "experiment-config-pubsub";

app.MapGet("/dapr/subscribe", async () =>
{
    logger.LogInformation("Received subscription request");
    var responseMessage = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"{appUrl}/dapr/subscribe"));
    var subscriptionSpecs = await responseMessage.Content.ReadFromJsonAsync<List<SubscriptionSpec>>();
    foreach (var spec in subscriptionSpecs)
    {
        spec.Route = spec.Route.StartsWith('/') ? $"/_ecs/pubsub{spec.Route}" : $"/_esc/pubsub/{spec.Route}";
    }
    subscriptionSpecs.Add(new SubscriptionSpec
    {
        Topic = $"config/{serviceName}",
        Pubsubname = pubsubName,
        Route = "/_ecs/variables-event",
    });
    return subscriptionSpecs;
});

app.MapPost("/_ecs/variables-event", async context =>
{
    var config = await context.Request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
    var remainingConfig = configService.UpdateConfig(config);
    if (remainingConfig.Count > 0)
    {
        var responseMessage = await httpClient.PostAsJsonAsync($"{appUrl}/ecs/variables", remainingConfig);
        responseMessage.EnsureSuccessStatusCode();
    }
    context.Response.StatusCode = 200;
});

app.MapGet("/_ecs/defined-variables", async () => {
    Dictionary<string, VariableDefinition> config;
    try
    {
        var responseMessage = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"{appUrl}/ecs/defined-variables"));
        config = await responseMessage.Content.ReadFromJsonAsync<Dictionary<string, VariableDefinition>>();
        logger.LogInformation($"Received config properties: {config}");
    }
    catch (Exception e)
    {
        config = [];
        logger.LogError(e, "Failed to get defined variables from service");
    }
    configService.AddVariableDefinitions(config);
    return config;
});

app.MapPost("/_ecs/pubsub/{**path}", async context => {
    var path = context.Request.RouteValues["path"];
    var requestUrl = $"{appUrl}/{path}";
    var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl)
    {
        Content = new StreamContent(context.Request.Body)
    };
    var deterioration = configService.GetPubsubDeterioration();
    if (deterioration.Delay.HasValue) {
        await Task.Delay(deterioration.Delay.Value);
    }
    if (deterioration.Error) {
        context.Response.StatusCode = 500;
        return;
    }
    var responseMessage = await httpClient.SendAsync(requestMessage);
    context.Response.StatusCode = (int)responseMessage.StatusCode;
    await context.Response.WriteAsync(await responseMessage.Content.ReadAsStringAsync());
});

app.MapFallback(async context =>
{
    var path = context.Request.Path;
    var method = context.Request.Method;
    var requestUrl = $"{appUrl}/{path}";
    var requestMessage = new HttpRequestMessage(new HttpMethod(method), requestUrl)
    {
        Content = new StreamContent(context.Request.Body)
    };
    var deterioration = configService.GetServiceInvocationDeterioration(path);
    if (deterioration.Delay.HasValue) {
        await Task.Delay(deterioration.Delay.Value);
    }
    if (deterioration.ErrorCode.HasValue) {
        context.Response.StatusCode = deterioration.ErrorCode.Value;
        return;
    }

    var responseMessage = await httpClient.SendAsync(requestMessage);
    context.Response.StatusCode = (int)responseMessage.StatusCode;
    await context.Response.WriteAsync(await responseMessage.Content.ReadAsStringAsync());
});

logger.LogInformation($"Waiting for application on port {appPort}");
new StartupService().WaitForStartup(appPort).Wait();
logger.LogInformation($"Application is running on port {appPort}");

new HeartbeatService(heartbeatInterval, pubsubName, replikaId, serviceName, logger).StartAsync();

app.Run();
