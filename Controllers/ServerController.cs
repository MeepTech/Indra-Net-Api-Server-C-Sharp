using Indra.Api.Configuration;
using Indra.Net.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Indra.Api.Controllers {
  [Route("/")]
  [ApiController]
  public class ServerController : ControllerBase {
    readonly DbContext _dbContext;
    readonly IUserService _userService;

    ServerController(DbContext dbContext, IUserService userService) {
      _dbContext = dbContext;
      _userService = userService;
    }

    public async Task<ServerInfoDto> Info() {
      return await Server.Current.ToDto(
        _userService.GetLoggedInUser(out var user) ? user : null,
        _dbContext
      );
    }
  }
}
