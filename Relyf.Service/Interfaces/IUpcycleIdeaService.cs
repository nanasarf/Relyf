namespace Relyf.Service.Interfaces;

public interface IUpcycleIdeaService
{
    Task<string> GetIdeasAsync(string item, CancellationToken ct = default);

    // NEW: multi-message support
    Task<string> GetIdeasFromMessagesAsync(IEnumerable<(string Role, string Content)> messages, CancellationToken ct = default);
}
