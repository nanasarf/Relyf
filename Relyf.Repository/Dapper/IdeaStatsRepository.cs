using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class IdeaStatsRepository : BaseRepository, IIdeaStatsRepository
{
    public IdeaStatsRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<IdeaStatsDto?> GetIdeaStatsAsync(int ideaId, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = @"
SELECT i.IdeaId,
       ISNULL(l.cnt, 0) AS Likes,
       ISNULL(s.cnt, 0) AS Saves,
       ISNULL(c.cnt, 0) AS Comments
FROM app.AiIdea i
LEFT JOIN (
    SELECT TargetId, COUNT(1) AS cnt
    FROM app.Reaction
    WHERE TargetType = 'Idea' AND Kind = 'like'
    GROUP BY TargetId
) l ON l.TargetId = i.IdeaId
LEFT JOIN (
    SELECT IdeaId, COUNT(1) AS cnt
    FROM app.SavedIdea
    GROUP BY IdeaId
) s ON s.IdeaId = i.IdeaId
LEFT JOIN (
    SELECT TargetId, COUNT(1) AS cnt
    FROM app.Comment
    WHERE TargetType = 'Idea'
    GROUP BY TargetId
) c ON c.TargetId = i.IdeaId
WHERE i.IdeaId = @ideaId AND i.IsDeleted = 0;";

            return await conn.QueryFirstOrDefaultAsync<IdeaStatsDto>(
                new CommandDefinition(sql, new { ideaId }, cancellationToken: ct));
        });

    public Task<IReadOnlyList<TopIdeaDto>> GetTopIdeasAsync(int take, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            take = Math.Clamp(take, 1, 50);

            const string sql = @"
WITH L AS (
  SELECT TargetId, COUNT(1) AS Likes
  FROM app.Reaction
  WHERE TargetType = 'Idea' AND Kind = 'like'
  GROUP BY TargetId
),
S AS (
  SELECT IdeaId, COUNT(1) AS Saves
  FROM app.SavedIdea
  GROUP BY IdeaId
),
C AS (
  SELECT TargetId, COUNT(1) AS Comments
  FROM app.Comment
  WHERE TargetType = 'Idea'
  GROUP BY TargetId
)
SELECT TOP (@take)
       i.IdeaId,
       i.Title,
       ISNULL(L.Likes,0)    AS Likes,
       ISNULL(S.Saves,0)    AS Saves,
       ISNULL(C.Comments,0) AS Comments,
       (ISNULL(L.Likes,0) * 3) + (ISNULL(S.Saves,0) * 2) + ISNULL(C.Comments,0) AS Score
FROM app.AiIdea i
LEFT JOIN L ON L.TargetId = i.IdeaId
LEFT JOIN S ON S.IdeaId  = i.IdeaId
LEFT JOIN C ON C.TargetId = i.IdeaId
WHERE i.IsDeleted = 0
ORDER BY Score DESC, i.IdeaId DESC;";
            var rows = (await conn.QueryAsync<TopIdeaDto>(
                new CommandDefinition(sql, new { take }, cancellationToken: ct))).ToList();
            IReadOnlyList<TopIdeaDto> list = rows;
            return list;
        });
}
