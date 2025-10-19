using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class IdeaSearchRepository : BaseRepository, IIdeaSearchRepository
{
    public IdeaSearchRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<(IReadOnlyList<IdeaSearchRow> Rows, int Total)> SearchAsync(
        string? q, string? tag, int? userId, int skip, int take, CancellationToken ct = default)
    => WithConnection(async conn =>
    {
        take = Math.Clamp(take, 1, 100);
        skip = Math.Max(0, skip);

        // WHERE builder
        var where = "i.IsDeleted = 0";
        var dyn = new DynamicParameters();
        dyn.Add("skip", skip);
        dyn.Add("take", take);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = "%" + EscapeLike(q.Trim()) + "%";
            where += " AND (i.Title LIKE @s ESCAPE '\\' OR i.IdeaText LIKE @s ESCAPE '\\')";
            dyn.Add("s", s);
        }

        if (userId is not null)
        {
            where += " AND i.UserId = @userId";
            dyn.Add("userId", userId.Value);
        }

        var join = "";
        if (!string.IsNullOrWhiteSpace(tag))
        {
            join = "JOIN app.IdeaTag it ON it.IdeaId = i.IdeaId JOIN app.Tag t ON t.TagId = it.TagId";
            where += " AND t.Name = @tag";
            dyn.Add("tag", tag.Trim());
        }

        var countSql = $@"SELECT COUNT(1)
FROM app.AiIdea i
{join}
WHERE {where};";

        var dataSql = $@"
SELECT i.IdeaId,
       i.Title,
       CASE WHEN LEN(i.IdeaText) > 140 THEN LEFT(i.IdeaText, 140) + '...' ELSE i.IdeaText END AS Preview,
       i.UserId,
       i.ItemId
FROM app.AiIdea i
{join}
WHERE {where}
ORDER BY i.IdeaId DESC
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

        var total = await conn.ExecuteScalarAsync<int>(new CommandDefinition(countSql, dyn, cancellationToken: ct));
        var rowsList = (await conn.QueryAsync<IdeaSearchRow>(new CommandDefinition(dataSql, dyn, cancellationToken: ct))).ToList();
        IReadOnlyList<IdeaSearchRow> rows = rowsList;
        return (rows, total);
    });
}
