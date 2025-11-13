using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Api.Security;
using Relyf.Repository.Dapper;
using System.Text.RegularExpressions;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthRepository _auth;
    private readonly IJwtTokenService _tokens;
    public AuthController(IAuthRepository auth, IJwtTokenService tokens)
    {
        _auth = auth;
        _tokens = tokens;
    }

    public sealed record RegisterRequest(string Email, string Password, string UserName, string DisplayName, string? CountryCode);
    public sealed record LoginRequest(string Email, string Password);
    public sealed record AuthResponse(int UserId, string Email, string UserName, string DisplayName, string Token);

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        var userName = (req.UserName ?? "").Trim();
        var display = (req.DisplayName ?? "").Trim();
        var country = req.CountryCode?.Trim();

        // Validation
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password) || 
            string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(display))
            return BadRequest("Email, Password, UserName, and DisplayName are required.");

        // Username validation
        if (userName.Length < 3 || userName.Length > 20)
            return BadRequest("Username must be between 3 and 20 characters.");

        if (!Regex.IsMatch(userName, @"^[a-zA-Z0-9_]+$"))
            return BadRequest("Username can only contain letters, numbers, and underscores.");

        // Check if email or username already exists
        if (await _auth.EmailExistsAsync(email, ct))
            return Conflict("Email already exists.");

        if (await _auth.UserNameExistsAsync(userName, ct))
            return Conflict("Username is already taken.");

        var (hash, salt) = PasswordHasher.Hash(req.Password);

        var user = await _auth.CreateUserWithCredentialAsync(email, userName, display, country, hash, salt, ct);

        var token = _tokens.Create(user.UserId, user.Email, user.DisplayName);
        return Created(string.Empty, new AuthResponse(user.UserId, user.Email, user.UserName, user.DisplayName, token));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email and Password are required.");

        var user = await _auth.GetUserByEmailAsync(email, ct);
        if (user is null) return Unauthorized("Invalid credentials.");

        var cred = await _auth.GetCredentialAsync(user.UserId, ct);
        if (cred is null) return Unauthorized("Invalid credentials.");

        if (!PasswordHasher.Verify(req.Password, cred.PasswordSalt, cred.PasswordHash))
            return Unauthorized("Invalid credentials.");

        var token = _tokens.Create(user.UserId, user.Email, user.DisplayName);
        return Ok(new AuthResponse(user.UserId, user.Email, user.UserName ?? "", user.DisplayName, token));
    }
}
