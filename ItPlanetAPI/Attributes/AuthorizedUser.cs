using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ItPlanetAPI;

public class AuthorizedUser : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Your implementation here to get the user ID from the HttpContext
        var userId = GetUserIdFromContext(context.HttpContext);

        // Pass the user to the controller method as a parameter
        context.ActionArguments["authorizedUserId"] = userId;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    /**
     * Get user Id with implicit assumption, that the method has already been validated by [Authorize] attribute
     */
    private int GetUserIdFromContext(HttpContext httpContext)
    {
        return httpContext.GetAuthenticatedUserId()
            .Some(id => id)
            .None(() => throw new ValidationException(
                "The methods has been authorized, but the user ID can't be found!"));
    }
}