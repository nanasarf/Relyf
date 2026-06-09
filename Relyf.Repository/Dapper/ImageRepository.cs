using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class ImageRepository : BaseRepository, IImageRepository
{
    public ImageRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<bool> OwnerExistsAsync(string ownerType, int ownerId) =>
        WithConnection(async conn =>
        {
            // Validate owner against the right table; parameterized to avoid injection
            string sql = ownerType switch
            {
                "Item" => "SELECT COUNT(1) FROM app.Item    WHERE ItemId    = @ownerId;",
                "Idea" => "SELECT COUNT(1) FROM app.AiIdea  WHERE IdeaId    = @ownerId;",
                "Project" => "SELECT COUNT(1) FROM app.Project WHERE ProjectId = @ownerId;",
                "User" => "SELECT COUNT(1) FROM app.[User] WHERE UserId = @ownerId;",
                _ => throw new ArgumentOutOfRangeException(nameof(ownerType))
            };
            var n = await conn.ExecuteScalarAsync<int>(sql, new { ownerId });
            return n > 0;
        });

    public Task<int> AddAsync(string ownerType, int ownerId, string source, string url, string? altText) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            @"INSERT INTO app.Image (OwnerType, OwnerId, Source, Url, AltText, CreatedAtUtc)
              VALUES (@OwnerType, @OwnerId, @Source, @Url, @AltText, SYSUTCDATETIME());
              SELECT CAST(SCOPE_IDENTITY() AS int);",
            new { OwnerType = ownerType, OwnerId = ownerId, Source = source, Url = url, AltText = altText }));

    public Task<IReadOnlyList<ImageRecord>> ListByOwnerAsync(string ownerType, int ownerId) =>
    WithConnection(async conn =>
    {
        const string sql = @"
        SELECT ImageId, OwnerType, OwnerId, Source, Url, AltText, CreatedAtUtc
        FROM app.Image
        WHERE OwnerType = @ownerType AND OwnerId = @ownerId
        ORDER BY ImageId DESC;";

        var rowsList = (await conn.QueryAsync<ImageRecord>(sql, new { ownerType, ownerId })).ToList();
        IReadOnlyList<ImageRecord> rows = rowsList; // explicit upcast fixes CS0029
        return rows;
    });

    public Task<int> DeleteAsync(int imageId) =>
        WithConnection(conn => conn.ExecuteAsync(
            "DELETE FROM app.Image WHERE ImageId = @imageId;", new { imageId }));
}
