namespace Relyf.Api.Models;

public class CoherePrompt
{
    public int CoherePromptId { get; set; }   
    public int UserId { get; set; }          
    public int? ItemId { get; set; }          
    public string? Model { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? TopP { get; set; }
    public string PromptText { get; set; } = "";
}
