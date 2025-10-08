namespace Relyf.Api.Models;

public class Feedback
{
    public int FeedbackId { get; set; }     // PK
    public int UserId { get; set; }         // FK -> app.User
    public string TargetType { get; set; } = "Idea"; // 'Idea' | 'Project' | 'App'
    public int? TargetId { get; set; }      // nullable for TargetType='App'
    public byte? Rating { get; set; }       // 1..5
    public string? Notes { get; set; }
}
