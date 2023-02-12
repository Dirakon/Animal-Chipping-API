using ItPlanetAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ItPlanetAPI;

public class ForbidOnIncorrectAuthorizationHeader : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.Request.Headers.ContainsKey("Authorization") &&
            context.HttpContext.GetAuthenticatedUserId() is null)
            context.Result = new UnauthorizedResult();
    }
}