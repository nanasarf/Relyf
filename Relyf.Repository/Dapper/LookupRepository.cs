using Dapper;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class LookupRepository : BaseRepository, ILookupRepository
{
    public LookupRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<bool> UserExistsAsync(int userId, CancellationToken ct = default) =>
        WithConnection(c => c.ExecuteScalarAsync<int>(
            new CommandDefinition("SELECT COUNT(1) FROM app.[User] WHERE UserId=@userId;",
                new { userId }, cancellationToken: ct))).ContinueWith(t => t.Result > 0);

    public Task<bool> ItemExistsAsync(int itemId, CancellationToken ct = default) =>
        WithConnection(c => c.ExecuteScalarAsync<int>(
            new CommandDefinition("SELECT COUNT(1) FROM app.Item WHERE ItemId=@itemId;",
                new { itemId }, cancellationToken: ct))).ContinueWith(t => t.Result > 0);

    public Task<bool> ItemOwnedByUserAsync(int itemId, int userId, CancellationToken ct = default) =>
        WithConnection(c => c.ExecuteScalarAsync<int>(
            new CommandDefinition("SELECT COUNT(1) FROM app.Item WHERE ItemId=@itemId AND UserId=@userId;",
                new { itemId, userId }, cancellationToken: ct))).ContinueWith(t => t.Result > 0);

    public Task<bool> IdeaExistsAsync(int ideaId, CancellationToken ct = default) =>
        WithConnection(c => c.ExecuteScalarAsync<int>(
            new CommandDefinition("SELECT COUNT(1) FROM app.AiIdea WHERE IdeaId=@ideaId AND IsDeleted=0;",
                new { ideaId }, cancellationToken: ct))).ContinueWith(t => t.Result > 0);

    public Task<bool> IdeaOwnedByUserAsync(int ideaId, int userId, CancellationToken ct = default) =>
        WithConnection(c => c.ExecuteScalarAsync<int>(
            new CommandDefinition("SELECT COUNT(1) FROM app.AiIdea WHERE IdeaId=@ideaId AND UserId=@userId AND IsDeleted=0;",
                new { ideaId, userId }, cancellationToken: ct))).ContinueWith(t => t.Result > 0);

    public Task<bool> ProjectOwnedByUserAsync(int projectId, int userId, CancellationToken ct = default) =>
        WithConnection(c => c.ExecuteScalarAsync<int>(
            new CommandDefinition("SELECT COUNT(1) FROM app.Project WHERE ProjectId=@projectId AND UserId=@userId AND IsDeleted=0;",
                new { projectId, userId }, cancellationToken: ct))).ContinueWith(t => t.Result > 0);

    public Task<bool> ProjectExistsAsync(int projectId, CancellationToken ct = default) =>
    WithConnection(c => c.ExecuteScalarAsync<int>(
        new CommandDefinition("SELECT COUNT(1) FROM app.Project WHERE ProjectId=@projectId AND IsDeleted=0;",
            new { projectId }, cancellationToken: ct))).ContinueWith(t => t.Result > 0);
    public Task<bool> MaterialExistsAsync(int materialId, CancellationToken ct = default) =>
    WithConnection(c => c.ExecuteScalarAsync<int>(
        new CommandDefinition("SELECT COUNT(1) FROM app.Material WHERE MaterialId=@materialId;",
        new { materialId }, cancellationToken: ct))).ContinueWith(t => t.Result > 0);
    public Task<bool> DropoffSiteExistsAsync(int siteId, CancellationToken ct = default) =>
    WithConnection(c => c.ExecuteScalarAsync<int>(
        new CommandDefinition("SELECT COUNT(1) FROM app.DropoffSite WHERE DropoffSiteId=@siteId;",
            new { siteId }, cancellationToken: ct))).ContinueWith(t => t.Result > 0);


}
