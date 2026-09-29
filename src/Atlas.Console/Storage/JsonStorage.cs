using System.Text.Json;

namespace Atlas.ConsoleApp.Storage;

internal static class JsonStorage
{
    public static T? Read<T>(string filePath, JsonSerializerOptions options)
    {
        if (!SqlStorage.Exists(filePath)) return default;
        var json = SqlStorage.ReadText(filePath);
        return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, options);
    }

    public static void Write<T>(string filePath, T value, JsonSerializerOptions options)
    {
        SqlStorage.WriteText(filePath, JsonSerializer.Serialize(value, options));
    }
}
