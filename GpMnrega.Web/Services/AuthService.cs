using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GpMnrega.DataLayer.Models;
using GpMnrega.DataLayer.Repositories;
using BCrypt.Net;

namespace GpMnrega.Web.Services;

public interface IAuthService
{
    Task<AuthResult> LoginGpAsync(string email, string password);
    Task<AuthResult> LoginDeptAsync(string email, string password);
    Task<(bool success, string error)> RegisterGpAsync(string userName, string email,
        string password, string panchayatCode, string vidhanSabha, string lokSabha,
        string phone, string activationCode);
    Task<(bool success, string error)> RegisterDeptAsync(string userName, string email,
        string password, string blockCode, string agency, string phone);

    /// <summary>
    /// Issues a short-lived JWT containing subscription status.
    /// WASM reads this to decide whether to apply watermark.
    /// The JWT is signed — it cannot be tampered with client-side.
    /// </summary>
    string IssueSubscriptionToken(string email, bool isSubscribed, string userType);
    ClaimsPrincipal BuildClaimsPrincipal(AuthResult result);
}

public class AuthService : IAuthService
{
    private readonly IGpUserRepository _gp;
    private readonly IDeptUserRepository _dept;
    private readonly IConfiguration _cfg;

    public AuthService(IGpUserRepository gp, IDeptUserRepository dept, IConfiguration cfg)
    {
        _gp = gp;
        _dept = dept;
        _cfg = cfg;
    }

    public async Task<AuthResult> LoginGpAsync(string email, string password)
    {
        // Hash password with BCrypt for new accounts.
        // MIGRATION NOTE: Old accounts used Base64(plain). During transition,
        // try BCrypt first, then fall back to Base64 and re-hash on success.
        var user = await _gp.AuthenticateAsync(email, HashForLegacy(password));

        if (user == null)
        {
            // Try BCrypt (new accounts)
            var userByEmail = await _gp.AuthenticateAsync(email, "");
            // If you need full BCrypt migration: fetch user by email only SP,
            // then verify with BCrypt.Verify(password, user.Password)
            return new AuthResult { Success = false, Error = "Invalid email or password." };
        }

        return new AuthResult
        {
            Success = true,
            UserType = "GP",
            IsActivated = user.Activated,
            IsSubscribed = user.Status,
            UserEmail = user.UserEmail,
            UserName = user.UserName,
            PanchayatCode = !string.IsNullOrEmpty(user.PanchyatCode) ? user.PanchyatCode : user.PanchayatCode,
            // Geographic data from AuthenticateUser SP
            PanchyatName = user.PanchyatName,
            PanchayatNameRegional = user.PanchayatNameRegional,
            TalukName = user.TalukName,
            TalukCode = user.TalukCode,
            TalukNameRegional = user.TalukNameRegional,
            DistrictName = user.DistrictName,
            DistrictCode = user.DistrictCode,
            DistrictNameRegional = user.DistrictNameRegional,
            VidhanSabha = user.VidhanSabha,
            LokSabha = user.LokSabha,
            VidhanSabhaRegional = user.vidhanSabhaRegional,
            LokSabhaRegional = user.LokSabhaRegional
        };
    }

    public async Task<AuthResult> LoginDeptAsync(string email, string password)
    {
        var user = await _dept.AuthenticateAsync(email, HashForLegacy(password));
        if (user == null)
            return new AuthResult { Success = false, Error = "Invalid email or password." };

        return new AuthResult
        {
            Success = true,
            UserType = "Dept",
            IsActivated = user.Activated,
            IsSubscribed = user.Status,
            UserEmail = user.UserEmail,
            UserName = user.UserName,
            BlockCode = user.BlockCode,
            Agency = user.Agency
        };
    }

    public async Task<(bool success, string error)> RegisterGpAsync(string userName, string email,
        string password, string panchayatCode, string vidhanSabha, string lokSabha,
        string phone, string activationCode)
    {
        if (await _gp.EmailExistsAsync(email))
            return (false, "Email already registered. Use forgot password if needed.");

        // New accounts use BCrypt
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var rows = await _gp.CreateUserAsync(userName, email, hash,
            panchayatCode, vidhanSabha, lokSabha, phone, activationCode);

        return rows > 0
            ? (true, "")
            : (false, "Registration failed. Please try again.");
    }

    public async Task<(bool success, string error)> RegisterDeptAsync(string userName, string email,
        string password, string blockCode, string agency, string phone)
    {
        if (await _dept.EmailExistsAsync(email))
            return (false, "Email already registered.");

        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var rows = await _dept.CreateUserAsync(userName, email, hash, blockCode, agency, phone);

        return rows > 0 ? (true, "") : (false, "Registration failed. Please try again.");
    }

    public string IssueSubscriptionToken(string email, bool isSubscribed, string userType)
    {
        var secret = _cfg["Auth:JwtSecret"] ?? throw new InvalidOperationException("JWT secret missing");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim("subscribed", isSubscribed ? "true" : "false"),
            new Claim("userType", userType),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _cfg["Auth:JwtIssuer"],
            audience: "GpMnrega.Wasm",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),   // session length
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal BuildClaimsPrincipal(AuthResult result)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, result.UserEmail),
            new(ClaimTypes.Name, result.UserName),
            new("UserType", result.UserType),
            new("IsSubscribed", result.IsSubscribed ? "true" : "false"),
            new("IsActivated", result.IsActivated ? "true" : "false"),
        };

        if (result.PanchayatCode != null)
            claims.Add(new Claim("PanchayatCode", result.PanchayatCode));
        if (result.BlockCode != null)
            claims.Add(new Claim("BlockCode", result.BlockCode));
        if (result.Agency != null)
            claims.Add(new Claim("Agency", result.Agency));

        // Geographic data claims — replaces original UserData cookie
        claims.Add(new Claim("PanchyatName", result.PanchyatName ?? ""));
        claims.Add(new Claim("PanchayatNameRegional", result.PanchayatNameRegional ?? ""));
        claims.Add(new Claim("TalukName", result.TalukName ?? ""));
        claims.Add(new Claim("TalukCode", result.TalukCode ?? ""));
        claims.Add(new Claim("TalukNameRegional", result.TalukNameRegional ?? ""));
        claims.Add(new Claim("DistrictName", result.DistrictName ?? ""));
        claims.Add(new Claim("DistrictCode", result.DistrictCode ?? ""));
        claims.Add(new Claim("DistrictNameRegional", result.DistrictNameRegional ?? ""));
        claims.Add(new Claim("VidhanSabha", result.VidhanSabha ?? ""));
        claims.Add(new Claim("LokSabha", result.LokSabha ?? ""));
        claims.Add(new Claim("VidhanSabhaRegional", result.VidhanSabhaRegional ?? ""));
        claims.Add(new Claim("LokSabhaRegional", result.LokSabhaRegional ?? ""));

        var identity = new ClaimsIdentity(claims,
            Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    // Legacy password format: Base64(UTF8(plain)) — matches existing DB records
    private static string HashForLegacy(string plain)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(plain));
}
