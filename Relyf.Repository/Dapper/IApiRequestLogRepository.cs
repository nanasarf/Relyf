namespace Relyf.Repository.Dapper;

public interface IApiRequestLogRepository
{
    Task<int> CreateAsync(int userId, string provider, string endpoint, string model,
                          byte[]? promptHash, int? tokensIn, int? tokensOut,
                          int statusCode, int durationMs, CancellationToken ct = default);
}
