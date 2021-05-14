using Microsoft.IdentityModel.Tokens;
using Shared.Lib;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

public class JWTAuthenticationManager 
{
    readonly IDictionary<string, string> users = new Dictionary<string, string> {{ "demo", "demo" },{ "admin", "password" }};
    private readonly byte[] tokenKey;
    public JWTAuthenticationManager(byte[] tokenKey) => this.tokenKey = tokenKey;
    public record TokenResponse(string access_token, long expires);
    public TokenResponse Authenticate(string username, string password)
    {
        if (!users.Any(u => u.Key == username && u.Value == password)) return null;
        var expires = DateTime.UtcNow.AddMinutes(1);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[] {new Claim(ClaimTypes.Name, username) }),
            Expires = expires,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
        };
        var desc = tokenHandler.CreateToken(tokenDescriptor);
        var token = new TokenResponse(tokenHandler.WriteToken(desc), expires.ToToUnixTimestamp());
        return token;
    }
}