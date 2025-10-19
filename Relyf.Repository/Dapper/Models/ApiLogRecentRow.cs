namespace Relyf.Repository.Dapper.Models;

public sealed class ApiLogRecentRow
{
    public int ApiRequestLogId { get; init; }
    public int? UserId { get; init; }
    public string Provider { get; init; } = "";
    public string Endpoint { get; init; } = "";
    public string? Model { get; init; }
    public int? TokensIn { get; init; }
    public int? TokensOut { get; init; }
    public int StatusCode { get; init; }
    public int? DurationMs { get; init; }
}

public sealed class ApiLogSummaryRow
{
    public int Total { get; init; }
    public int Errors { get; init; }
    public double ErrorRate { get; init; }
    public double? AvgLatencyMs { get; init; }
    public int TokensIn { get; init; }
    public int TokensOut { get; init; }
}

public sealed class ApiLogTopModelRow
{
    public string Model { get; init; } = "(unknown)";
    public int Calls { get; init; }
    public double AvgLatencyMs { get; init; }
    public double ErrorRate { get; init; }
}
