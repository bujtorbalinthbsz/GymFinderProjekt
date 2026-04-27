using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using konditeremApiv2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace konditeremApiv2.Services;

public class AuthService(AppDbContext context, IConfiguration configuration) : IAuthService
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
        {
            return null;
        }

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(null, user.Password, request.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(CreateTokenDescriptor(user));

        return new LoginResponse
        {
            Token = tokenHandler.WriteToken(token),
            UserId = user.Id
        };
    }

    public async Task<UserResponse?> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await context.Users.AnyAsync(u => u.Email == request.Email);
        if (existingUser)
        {
            return null;
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Password = _passwordHasher.HashPassword(null, request.Password),
            Role = "user"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.GetResponse();
    }

    public Task LogoutAsync()
    {
        return Task.CompletedTask;
    }

    private SecurityTokenDescriptor CreateTokenDescriptor(User user)
    {
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing in configuration.");
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role)
        };

        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature)
        };
    }
}