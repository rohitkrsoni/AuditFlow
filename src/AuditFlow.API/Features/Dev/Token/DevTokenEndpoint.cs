using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuditFlow.API.Infrastructure.Auth;
using AuditFlow.API.Infrastructure.Configurations;
using AuditFlow.API.Shared;
using AuditFlow.API.Shared.Endpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

namespace AuditFlow.API.Features.Dev.Token;

[Route(ApiRoute.DevRoute)]
public sealed class DevTokenEndpoint : EndpointBase
{
    /// <summary>
    /// Issue a short-lived JWT for local testing (Development only).
    /// </summary>
    [HttpPost("token")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = ["Dev"])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DevTokenResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Handle([FromServices] IConfiguration cfg, [FromServices] IHostEnvironment env)
    {
        // Hide this endpoint outside Development
        //if (!env.IsDevelopment())
        //{
        //    return NotFound();
        //}

        var jwtCfg = cfg.GetSection(nameof(JwtConfigurationsSettings)).Get<JwtConfigurationsSettings>()!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtCfg.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
      new Claim(ClaimTypes.NameIdentifier, "dev-user"),
      new Claim(ClaimTypes.Name, "Dev User"),
      new Claim(ClaimTypes.Role, InternalRole.GetRoles[0]) // e.g., SuperUser
    };

        var expires = DateTime.UtcNow.AddHours(8);
        var token = new JwtSecurityToken(
          issuer: jwtCfg.Issuer,
          audience: jwtCfg.Audience,
          claims: claims,
          notBefore: DateTime.UtcNow,
          expires: expires,
          signingCredentials: creds);

        var jwtString = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new DevTokenResponse(jwtString, "Bearer", (int)TimeSpan.FromHours(8).TotalSeconds));
    }
}
