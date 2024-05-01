using System.Text.Json;

namespace ExperimentConfigSidecar.Models;

/// <summary>
/// Utility methods.
/// </summary>
public static class Util
{
    /// <summary>
    /// Gets a double property from a JSON element.
    /// </summary>
    /// <param name="element">The JSON element</param>
    /// <param name="propertyName">The property name</param>
    /// <returns>The double value or null if the property does not exist or is not a double</returns>
    public static double? GetDoubleProperty(this JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            if (property.TryGetDouble(out var value))
            {
                return value;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets an integer property from a JSON element.
    /// </summary>
    /// <param name="element">The JSON element</param>
    /// <param name="propertyName">The property name</param>
    /// <returns>The integer value or null if the property does not exist or is not an integer</returns>
    public static int? GetIntProperty(this JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            if (property.TryGetInt32(out var value))
            {
                return value;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets a string property from a JSON element.
    /// </summary>
    /// <param name="element">The JSON element</param>
    /// <param name="propertyName">The property name</param>
    /// <returns>The string value or null if the property does not exist or is not a string</returns>
    public static string? GetStringProperty(this JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            return property.GetString();
        }
        return null;
    }

    /// <summary>
    /// Checks if a JSON element is null.
    /// </summary>
    /// <param name="element">The JSON element</param>
    /// <returns>True if the element is null, false otherwise</returns>
    public static bool IsNull(this JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Null;
    }

    /// <summary>
    /// Converts a string to a JSON element.
    /// </summary>
    /// <param name="json">The JSON string</param>
    /// <returns>The JSON element</returns>
    public static JsonElement AsJsonElement(this string json)
    {
        return JsonDocument.Parse(json).RootElement;
    }
}