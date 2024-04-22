using System.Text.Json;

namespace ExperimentConfigSidecar.Models
{
    public static class Util
    {
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

        public static string? GetStringProperty(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                return property.GetString();
            }
            return null;
        }
    }
}