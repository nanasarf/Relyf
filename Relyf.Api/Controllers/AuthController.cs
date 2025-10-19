using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Api.Security;
using Relyf.Repository.Dapper;

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

    public sealed record RegisterRequest(string Email, string Password, string DisplayName, string? CountryCode);
    public sealed record LoginRequest(string Email, string Password);
    public sealed record AuthResponse(int UserId, string Email, string DisplayName, string Token);

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        var display = (req.DisplayName ?? "").Trim();
        var country = req.CountryCode?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(display))
            return BadRequest("Email, Password, and DisplayName are required.");

        if (await _auth.EmailExistsAsync(email, ct))
            return Conflict("Email already exists.");

        var (hash, salt) = PasswordHasher.Hash(req.Password);

        var user = await _auth.CreateUserWithCredentialAsync(email, display, country, hash, salt, ct);

        var token = _tokens.Create(user.UserId, user.Email, user.DisplayName);
        return Created(string.Empty, new AuthResponse(user.UserId, user.Email, user.DisplayName, token));
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
        return Ok(new AuthResponse(user.UserId, user.Email, user.DisplayName, token));
    }
}
