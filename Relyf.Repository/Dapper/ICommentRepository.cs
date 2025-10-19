using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface ICommentRepository
{
    Task<int> CreateAsync(int userId, string targetType, int targetId, string body, CancellationToken ct = default);

    // public listing for a target (no auth required)
    Task<IReadOnlyList<CommentRecord>> ListForTargetAsync(string targetType, int targetId, int take, CancellationToken ct = default);

    // delete only if user owns the comment
    Task<int> DeleteIfOwnerAsync(int commentId, int authUserId, CancellationToken ct = default);
}
