using System.Data;
using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class TagRepository : BaseRepository, ITagRepository
{
    public TagRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<(int TagId, bool Created)> CreateIfNotExistsAsync(string name, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            // try get existing
            const string getSql = "SELECT TagId FROM app.Tag WHERE Name = @name;";
            var id = await conn.ExecuteScalarAsync<int?>(new CommandDefinition(getSql, new { name }, cancellationToken: ct));
            if (id.HasValue) return (id.Value, false);

            // insert new
            const string insSql = @"INSERT INTO app.Tag (Name) VALUES (@name); SELECT CAST(SCOPE_IDENTITY() AS int);";
            var newId = await conn.ExecuteScalarAsync<int>(new CommandDefinition(insSql, new { name }, cancellationToken: ct));
            return (newId, true);
        });

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = "SELECT COUNT(1) FROM app.Tag WHERE Name = @name;";
            var n = await conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { name }, cancellationToken: ct));
            return n > 0;
        });

    public Task<IReadOnlyList<TagRecord>> ListAllAsync(CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = "SELECT TagId, Name FROM app.Tag ORDER BY Name ASC;";
            var rows = (await conn.QueryAsync<TagRecord>(new CommandDefinition(sql, cancellationToken: ct))).ToList();
            IReadOnlyList<TagRecord> list = rows;
            return list;
        });

    public Task AttachToIdeaAsync(int ideaId, IEnumerable<string> tagNames, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

            // Normalize names
            var names = tagNames.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (names.Count == 0) { tx.Commit(); return; }

            // fetch existing tag ids
            const string getExistingSql = "SELECT TagId, Name FROM app.Tag WHERE Name IN @names;";
            var existing = (await conn.QueryAsync<TagRecord>(new CommandDefinition(getExistingSql, new { names }, tx, cancellationToken: ct))).ToList();

            // create missing
            var existingNames = existing.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missing = names.Where(n => !existingNames.Contains(n)).ToList();
            foreach (var m in missing)
            {
                var id = await conn.ExecuteScalarAsync<int>(
                    new CommandDefinition(@"INSERT INTO app.Tag (Name) VALUES (@name); SELECT CAST(SCOPE_IDENTITY() AS int);",
                                          new { name = m }, tx, cancellationToken: ct));
                existing.Add(new TagRecord { TagId = id, Name = m });
            }

            // idempotent attach
            const string attachSql = @"
IF NOT EXISTS (SELECT 1 FROM app.IdeaTag WHERE IdeaId = @ideaId AND TagId = @tagId)
    INSERT INTO app.IdeaTag (IdeaId, TagId) VALUES (@ideaId, @tagId);";

            foreach (var t in existing)
            {
                await conn.ExecuteAsync(new CommandDefinition(attachSql, new { ideaId, tagId = t.TagId }, tx, cancellationToken: ct));
            }

            tx.Commit();
        });

    public Task<IReadOnlyList<IdeaTagView>> IdeasByTagAsync(string tagName, int take, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = @"
SELECT  i.IdeaId,
        i.Title,
        CASE WHEN LEN(i.IdeaText) > 140 THEN LEFT(i.IdeaText, 140) + '...' ELSE i.IdeaText END AS Preview
FROM app.IdeaTag it
JOIN app.Tag t ON t.TagId = it.TagId
JOIN app.AiIdea i ON i.IdeaId = it.IdeaId
WHERE t.Name = @tagName
ORDER BY i.IdeaId DESC
OFFSET 0 ROWS FETCH NEXT @take ROWS ONLY;";
            var rows = (await conn.QueryAsync<IdeaTagView>(new CommandDefinition(sql, new { tagName, take }, cancellationToken: ct))).ToList();
            IReadOnlyList<IdeaTagView> list = rows;
            return list;
        });
}
