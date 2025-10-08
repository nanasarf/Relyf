namespace Relyf.Api.Models;

public class ApiRequestLog
{
    public int ApiRequestLogId { get; set; } 
    public int? UserId { get; set; }          
    public string Provider { get; set; } = "cohere";
    public string Endpoint { get; set; } = "/v2/chat";
    public string? Model { get; set; }
    public byte[]? PromptHash { get; set; }   
    public int? TokensIn { get; set; }
    public int? TokensOut { get; set; }
    public int StatusCode { get; set; }
    public int? DurationMs { get; set; }
}
