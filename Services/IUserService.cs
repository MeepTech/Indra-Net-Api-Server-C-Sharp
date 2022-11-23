using Indra.Api.Configuration;
using Indra.Net.Dtos;
using Indra.Net.Focuses.Actors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Indra.Api {

  public interface IUserService {
    Response Authenticate(UserLoginDto model, out User user);
    bool GetLoggedInUser(out User user);
    internal string _hashSaltedPassword(string saltedPassword);
  }

  public class UserService : IUserService {
    readonly DbContext _db;
    readonly string _jwtSecret;
    readonly string _passwordHasher;
    readonly HttpContext _context;
    byte[] _key;

    public UserService(
      HttpContext context,
      DbContext db,
      IOptions<JwtSettings> jwtSecretOptions,
      IOptions<AuthorizationSettings> authOptions
    ) {
      _db = db;
      _jwtSecret = jwtSecretOptions.Value.Secret;
      _passwordHasher = authOptions.Value.PasswordHasher;
      _context = context;
    }

    #region Authentication

    public Response Authenticate(UserLoginDto loginData, out User user) {
      if ((user = _db.Set<User>().FirstOrDefault(u => u.Key.ToLower() == loginData.UserName.ToLower())) != null 
        && user.PasswordHash == loginData.SaltedPassword
      ) {
        var token = _generateJwtToken(user);
        return new ResultResponse<object>(new { UserName = user.Key, Result = token });
      } else {
        return new FailureResponse { Message = "No user with matching username and password found." };
      }
    }

    public bool GetLoggedInUser(out User user) {
      if (_context.Items.TryGetValue("User", out var found)) {
        user = (User)found;
        return true;
      }

      user = null;
      return false;
    }

    #endregion

    // helper methods

    string IUserService._hashSaltedPassword(string saltedPassword) {
      throw new NotImplementedException();
    }

    string _generateJwtToken(User user) {
      // generate token that is valid for 7 days
      var tokenHandler = new JwtSecurityTokenHandler();
      _key ??= Encoding.ASCII.GetBytes(_jwtSecret);

      var tokenDescriptor = new SecurityTokenDescriptor {
        Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
        Expires = DateTime.UtcNow.AddDays(5),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
      };

      var token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
    }
  }
}