using Dapper;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class ApiRequestLogRepository : BaseRepository, IApiRequestLogRepository
{
    public ApiRequestLogRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<int> CreateAsync(int userId, string provider, string endpoint, string model,
                                 byte[]? promptHash, int? tokensIn, int? tokensOut,
                                 int statusCode, int durationMs, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteAsync(
            new CommandDefinition(
                @"INSERT INTO app.ApiRequestLog (UserId, Provider, Endpoint, Model, PromptHash, TokensIn, TokensOut, StatusCode, DurationMs, CreatedAtUtc)
                  VALUES (@userId, @provider, @endpoint, @model, @promptHash, @tokensIn, @tokensOut, @statusCode, @durationMs, SYSUTCDATETIME());",
                new { userId, provider, endpoint, model, promptHash, tokensIn, tokensOut, statusCode, durationMs },
                cancellationToken: ct
            )));
}
