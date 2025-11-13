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
SELECT  
    i.IdeaId,
    i.Title,
    CASE WHEN LEN(i.IdeaText) > 140 THEN LEFT(i.IdeaText, 140) + '...' ELSE i.IdeaText END AS Preview,
    i.ImageUrl,
    s.SavedAtUtc
FROM app.SavedIdea s
JOIN app.AiIdea i ON i.IdeaId = s.IdeaId
WHERE s.UserId = @userId
  AND i.IsDeleted = 0
ORDER BY s.SavedAtUtc DESC;";
            
            var savedIdeas = (await conn.QueryAsync<SavedIdeaView>(
                new CommandDefinition(sql, new { userId }, cancellationToken: ct))).ToList();
            
            // Load tags for each idea
            if (savedIdeas.Any())
            {
                var ideaIds = savedIdeas.Select(s => s.IdeaId).ToArray();
                
                const string tagSql = @"
SELECT it.IdeaId, t.TagName
FROM app.IdeaTag it
JOIN app.Tag t ON t.TagId = it.TagId
WHERE it.IdeaId IN @ideaIds;";
                
                var tagRows = await conn.QueryAsync<(int IdeaId, string TagName)>(
                    new CommandDefinition(tagSql, new { ideaIds }, cancellationToken: ct));
                
                var tagsByIdea = tagRows.GroupBy(x => x.IdeaId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.TagName).ToList());
                
                // Assign tags to saved ideas
                foreach (var idea in savedIdeas)
                {
                    idea.Tags = tagsByIdea.TryGetValue(idea.IdeaId, out var tags) 
                        ? tags 
                        : new List<string>();
                }
            }
            
            IReadOnlyList<SavedIdeaView> result = savedIdeas;
            return result;
        });
}
