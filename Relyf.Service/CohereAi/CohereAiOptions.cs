public sealed class CohereAiOptions
{
    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://api.cohere.com";
    public string Model { get; init; } = "command-r";
    public int MaxTokens { get; init; } = 1000;  
    public double Temperature { get; init; } = 0.2;
}
