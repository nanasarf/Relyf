using System.Text;
using Relyf.Service.Interfaces;
using Relyf.Service.CohereAi;

namespace Relyf.Service;

public sealed class UpcycleIdeaService : IUpcycleIdeaService
{
    private readonly ICohereClient _cohere;
    public UpcycleIdeaService(ICohereClient cohere) => _cohere = cohere;

    // Single-item overload (kept)
    public Task<string> GetIdeasAsync(string item, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(item)) throw new ArgumentException("Item is required.");

        var prompt = new StringBuilder()
            .AppendLine("You are an expert, safety-first upcycling assistant.")
            .AppendLine("Return exactly 3 concise ideas with: title, tools, steps, safety.")
            .AppendLine("Keep total under 200 words. Avoid power tools if not necessary.")
            .Append("Item: ").Append(item).Append(".")
            .ToString();

        return _cohere.ChatAsync(prompt, ct);
    }

    // NEW: multi-message overload (no limit on count/length)
    public Task<string> GetIdeasFromMessagesAsync(IEnumerable<(string Role, string Content)> messages, CancellationToken ct = default)
    {
        var sb = new StringBuilder()
            .AppendLine("You are an expert, safety-first upcycling assistant.")
            .AppendLine("Return exactly 3 concise ideas with: title, tools, steps, safety.")
            .AppendLine("Keep total under 200 words. Avoid power tools if not necessary.");

        foreach (var (role, content) in messages)
        {
            if (!string.IsNullOrWhiteSpace(content))
                sb.AppendLine($"{role.ToUpperInvariant()}: {content}");
        }

        return _cohere.ChatAsync(sb.ToString(), ct);
    }
}
