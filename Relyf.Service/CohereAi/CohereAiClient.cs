using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Relyf.Service.Interfaces;

namespace Relyf.Service.CohereAi;

public sealed class CohereAiClient : ICohereClient
{
    private readonly HttpClient _http;
    private readonly ILogger<CohereAiClient> _log;
    private readonly string[] _modelPreference;

    public CohereAiClient(HttpClient http, IConfiguration config, ILogger<CohereAiClient> log)
    {
        _http = http;
        _log = log;
        var apiKey = config["Cohere:ApiKey"]
            ?? throw new Exception("Cohere:ApiKey is missing.");
        var configured = config["Cohere:Model"];

        _modelPreference = new[]
        {
            configured ?? string.Empty,
            "command-r-plus",
            "command"
        }
        .Where(m => !string.IsNullOrWhiteSpace(m))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _http.DefaultRequestHeaders.TryAddWithoutValidation("Cohere-Version", "2024-10-22");
    }

    public async Task<string> ChatAsync(string item, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(item))
            throw new ArgumentException("Item must not be empty.", nameof(item));

        var message = $"List 3 creative, safe ways to reuse or upcycle: {item}. Return as a numbered list (1., 2., 3.).";

        foreach (var model in _modelPreference)
        {
            var body = new
            {
                model,
                message,
                temperature = 0.2,
                max_tokens = 400
            };

            var json = JsonSerializer.Serialize(body);
            _log.LogInformation("Cohere request: {Json}", json);

            using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.cohere.ai/v1/chat")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            using var res = await _http.SendAsync(req, ct);
            var raw = await res.Content.ReadAsStringAsync(ct);

            _log.LogInformation("Cohere response ({Status}): {Raw}", res.StatusCode, raw);

            if (res.IsSuccessStatusCode)
                return ExtractText(raw) ?? "No suggestions.";

            if (res.StatusCode == HttpStatusCode.NotFound ||
                raw.Contains("model", StringComparison.OrdinalIgnoreCase) &&
                raw.Contains("removed", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            throw new HttpRequestException($"Cohere API error {(int)res.StatusCode}: {raw}");
        }

        throw new HttpRequestException(
            "All configured Cohere models failed. Update Cohere:Model in configuration to a supported model.");
    }

    private static string? ExtractText(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);

        if (doc.RootElement.TryGetProperty("message", out var msg) &&
            msg.TryGetProperty("content", out var contentArr) &&
            contentArr.ValueKind == JsonValueKind.Array &&
            contentArr.GetArrayLength() > 0 &&
            contentArr[0].TryGetProperty("text", out var firstText))
        {
            return firstText.GetString();
        }

        if (doc.RootElement.TryGetProperty("text", out var textProp))
            return textProp.GetString();

        return null;
    }
}
