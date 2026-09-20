using System.Text.Json;

namespace Atlas.ConsoleApp.Storage;

internal static class JsonStorage
{
    public static T? Read<T>(string filePath, JsonSerializerOptions options)
    {
        if (!File.Exists(filePath)) return default;
        var json = File.ReadAllText(filePath);
        return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, options);
    }

    public static void Write<T>(string filePath, T value, JsonSerializerOptions options)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        File.WriteAllText(filePath, JsonSerializer.Serialize(value, options));
    }
}
