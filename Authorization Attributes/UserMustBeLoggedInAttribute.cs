using Microsoft.AspNetCore.Http;
using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Indra.Net.Focuses.Actors;

namespace Vore.Game.Server {
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
  public class UserMustBeLoggedInAttribute : Attribute, IAuthorizationFilter {
    public void OnAuthorization(AuthorizationFilterContext context) {
      var user = (User)context.HttpContext.Items["User"];
      if (user == null) {
        // not logged in
        context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
      }
    }
  }
}
