namespace Relyf.Service.Interfaces;

public interface ICohereClient
{
    Task<string> ChatAsync(string prompt, CancellationToken ct = default);
}
