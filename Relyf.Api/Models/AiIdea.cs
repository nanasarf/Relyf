namespace Relyf.Api.Models;

public class AiIdea
{
    public int IdeaId { get; set; }           
    public int CoherePromptId { get; set; }   
    public int? ItemId { get; set; }         
    public int UserId { get; set; }          
    public string Title { get; set; } = "";
    public string IdeaText { get; set; } = "";
    public string? Difficulty { get; set; }   
    public int? EstTimeMin { get; set; }
    public decimal? EstCostUSD { get; set; }
    public int? TokensIn { get; set; }
    public int? TokensOut { get; set; }
    public int? ApiLatencyMs { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
}
