// Models/UserCredential.cs
namespace Relyf.Api.Models;
public sealed class UserCredential
{
    public int UserId { get; set; }
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public DateTime CreatedUtc { get; set; }
    public DateTime? LastLoginUtc { get; set; }
    public User? User { get; set; }
}
