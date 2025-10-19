namespace Relyf.Repository.Dapper.Models;

public sealed class FeedbackSummary
{
    public string TargetType { get; init; } = "";
    public int TargetId { get; init; }
    public int Count { get; init; }
    public double? Average { get; init; }
}
