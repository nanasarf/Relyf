using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class CoherePromptRepository : BaseRepository, ICoherePromptRepository
{
    public CoherePromptRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<int> CreateAsync(int userId, int? itemId, string? model, decimal? temperature, decimal? topP, string promptText, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                @"INSERT INTO app.CoherePrompt (UserId, ItemId, Model, Temperature, TopP, PromptText, CreatedAtUtc)
                  VALUES (@userId, @itemId, @model, @temperature, @topP, @promptText, SYSUTCDATETIME());
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                new { userId, itemId, model, temperature, topP, promptText },
                cancellationToken: ct
            )));

    public Task<CoherePromptRecord?> GetAsync(int coherePromptId, int userId, CancellationToken ct = default) =>
        WithConnection(conn => conn.QuerySingleOrDefaultAsync<CoherePromptRecord>(
            new CommandDefinition(
                @"SELECT CoherePromptId, UserId, ItemId, Model, Temperature, TopP, PromptText, CreatedAtUtc
                  FROM app.CoherePrompt
                  WHERE CoherePromptId = @coherePromptId AND UserId = @userId;",
                new { coherePromptId, userId },
                cancellationToken: ct
            )));
}
