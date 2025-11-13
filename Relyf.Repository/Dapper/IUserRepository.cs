using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IUserRepository
{
    Task<UserRecord?> GetByIdAsync(int userId);
    Task<UserRecord?> GetByEmailAsync(string email);
    Task<UserRecord?> GetByUserNameAsync(string userName);

    Task<int> CreateAsync(string email, string userName, string displayName, string? countryCode);
    Task<int> UpdateAsync(int userId, string email, string displayName, string? countryCode, bool isDeleted);
    Task<int> SoftDeleteAsync(int userId); // set IsDeleted = 1
    
    /// <summary>
    /// Check if a username already exists (case-insensitive).
    /// </summary>
    Task<bool> UserNameExistsAsync(string userName);
    
    /// <summary>
    /// Check if a username already exists excluding a specific user ID.
    /// </summary>
    Task<bool> UserNameExistsAsync(string userName, int excludeUserId);
    
    /// <summary>
    /// Update user profile fields (partial update support).
    /// </summary>
    Task<int> UpdateProfileAsync(int userId, string? userName, string? displayName, string? bio, string? avatarUrl);
    
    /// <summary>
    /// Search users by query string (searches DisplayName, UserName, Email).
    /// </summary>
    Task<UserSearchResult> SearchAsync(string query, int skip, int take, int? requestingUserId = null);
    
    /// <summary>
    /// Get user profile with counts and relationship status.
    /// </summary>
    Task<UserProfileDto?> GetProfileAsync(int userId, int? requestingUserId = null);
}
