using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface ITagRepository
{
    Task<(int TagId, bool Created)> CreateIfNotExistsAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<TagRecord>> ListAllAsync(CancellationToken ct = default);

    // Ensure tags exist (create any missing) and attach them to an Idea (idempotent)
    Task AttachToIdeaAsync(int ideaId, IEnumerable<string> tagNames, CancellationToken ct = default);

    Task<IReadOnlyList<IdeaTagView>> IdeasByTagAsync(string tagName, int take, CancellationToken ct = default);
}
