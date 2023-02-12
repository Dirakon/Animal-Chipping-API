using System.Security.Claims;
using ItPlanetAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

public static class ControllerExtensions
{
    public static int? GetAuthenticatedUserId(this ControllerBase controller)
    {
        return controller.HttpContext.GetAuthenticatedUserId();
    }

    public static async Task<Account?> GetAuthenticatedAccount(this ControllerBase controller, DatabaseContext context)
    {
        return controller.GetAuthenticatedUserId() switch{
            null => null, 
            {} id => await context.Accounts.SingleOrDefaultAsync(u =>
                u.Id == id),
        };
    }


    public static int? GetAuthenticatedUserId(this HttpContext context)
    {
        var idAsString = context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idAsString)) return null;
        return int.Parse(idAsString);
    }
}