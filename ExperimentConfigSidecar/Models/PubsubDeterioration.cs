namespace ExperimentConfigSidecar.Models;

/// <summary>
/// Represents the Pubsub deterioration configuration.
/// </summary>
/// <param name="Delay">The delay in milliseconds to introduce in the Pubsub message processing.</param>
/// <param name="Error">Whether to introduce an error in the Pubsub message processing.</param>
public record PubsubDetertioration(int? Delay, bool Error);