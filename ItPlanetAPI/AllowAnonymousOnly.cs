using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ItPlanetAPI;
public class AllowAnonymousOnly : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity is {IsAuthenticated: true})
        {
            context.Result = new ForbidResult();
        }
    }
}