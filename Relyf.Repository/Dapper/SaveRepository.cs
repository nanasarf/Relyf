using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class SaveRepository : BaseRepository, ISaveRepository
{
    public SaveRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<bool> PutAsync(int userId, int ideaId, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM app.SavedIdea WHERE UserId=@userId AND IdeaId=@ideaId)
BEGIN
    INSERT INTO app.SavedIdea (UserId, IdeaId, SavedAtUtc)
    VALUES (@userId, @ideaId, SYSUTCDATETIME());
    SELECT 1;
END
ELSE SELECT 0;";
            var inserted = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { userId, ideaId }, cancellationToken: ct));
            return inserted == 1;
        });

    public Task<int> DeleteAsync(int userId, int ideaId, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM app.SavedIdea WHERE UserId=@userId AND IdeaId=@ideaId;",
                new { userId, ideaId }, cancellationToken: ct)));

    public Task<IReadOnlyList<SavedIdeaView>> ListForUserAsync(int userId, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = @"
SELECT  i.IdeaId,
        i.Title,
        CASE WHEN LEN(i.IdeaText) > 140 THEN LEFT(i.IdeaText, 140) + '...' ELSE i.IdeaText END AS Preview,
        s.SavedAtUtc
FROM app.SavedIdea s
JOIN app.AiIdea i ON i.IdeaId = s.IdeaId
WHERE s.UserId = @userId
ORDER BY s.SavedAtUtc DESC;";
            var rows = (await conn.QueryAsync<SavedIdeaView>(
                new CommandDefinition(sql, new { userId }, cancellationToken: ct))).ToList();
            IReadOnlyList<SavedIdeaView> list = rows;
            return list;
        });
}
