using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ItPlanetAPI.Middleware;

public class AuthorizedUser : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var userId = GetUserIdFromContext(context.HttpContext);

        context.ActionArguments["authorizedUserId"] = userId;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    /**
     * Get user Id with implicit assumption, that the method has already been validated by [Authorize] attribute
     */
    private static int GetUserIdFromContext(HttpContext httpContext)
    {
        return httpContext.GetAuthenticatedUserId() switch
        {
            { } id => id,
            null => throw new ValidationException(
                "The methods is assumed to be authorized, but the user ID can't be extracted!")
        };
    }
}