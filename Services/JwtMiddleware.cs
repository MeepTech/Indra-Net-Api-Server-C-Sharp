using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;
using Indra.Api.Configuration;

namespace Indra.Api {

  public class JwtMiddleware {
    readonly RequestDelegate _next;
    readonly string _jwtSecret;
    byte[] _key;

    public JwtMiddleware(RequestDelegate next, IOptions<JwtSettings> jwtSettings) {
      _next = next;
      _jwtSecret = jwtSettings.Value.Secret;
    }

    public async Task Invoke(HttpContext context, IUserService userService) {
      var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

      if (token != null) {
        // TODO: check if it's got an id, if not check if it's a valid age token and attach accordingly.
        _attachUserToContext(context, userService, token);
      }

      await _next(context);
    }

    void _attachUserToContext(HttpContext context, IUserService userService, string token) {
      try {
        var tokenHandler = new JwtSecurityTokenHandler();
        _key ??= Encoding.ASCII.GetBytes(_jwtSecret);
        tokenHandler.ValidateToken(token, new TokenValidationParameters {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(_key),
          ValidateIssuer = false,
          ValidateAudience = false,
          // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
          ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        string userId = jwtToken.Claims.First(x => x.Type == "id").Value;

        // attach user to context on successful jwt validation
        context.Items["User"] = userService.TryToGetById(userId, out var user) ? user : null;
      }
      catch {
        // do nothing if jwt validation fails
        // user is not attached to context so request won't have access to secure routes
      }
    }
  }
}
