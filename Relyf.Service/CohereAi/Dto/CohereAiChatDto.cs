using System.Text.Json.Serialization;

namespace Relyf.Service.CohereAi.Dto;

public sealed class CohereChatRequest
{
    [JsonPropertyName("model")] public string Model { get; init; } = "command-r";
    [JsonPropertyName("messages")] public List<CohereMessage> Messages { get; init; } = new();
    [JsonPropertyName("max_tokens")] public int MaxTokens { get; init; } = 400;
    [JsonPropertyName("temperature")] public double Temperature { get; init; } = 0.2;
}

public sealed class CohereMessage
{
    [JsonPropertyName("role")] public string Role { get; init; } = "user";
    [JsonPropertyName("content")] public string Content { get; init; } = string.Empty;
}

public sealed class CohereChatResponse
{
    [JsonPropertyName("message")] public CohereMessageWrapper? Message { get; init; }
    [JsonPropertyName("text")] public string? Text { get; init; }
    [JsonPropertyName("generations")] public List<CohereGeneration>? Generations { get; init; }

    [JsonIgnore]
    public string? FirstText =>
        Message?.Content?.FirstOrDefault(p => string.Equals(p.Type, "text", StringComparison.OrdinalIgnoreCase))?.Text
        ?? Text
        ?? Generations?.FirstOrDefault()?.Text;
}

public sealed class CohereMessageWrapper
{
    [JsonPropertyName("content")] public List<CohereContentPart> Content { get; init; } = new();
}

public sealed class CohereContentPart
{
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("text")] public string? Text { get; init; }
}

public sealed class CohereGeneration
{
    [JsonPropertyName("text")] public string? Text { get; init; }
}
