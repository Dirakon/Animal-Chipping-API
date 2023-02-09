using System.Security.Claims;
using ItPlanetAPI.Models;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItPlanetAPI.Controllers;

public static class ControllerExtensions
{
    public static Option<int> GetAuthenticatedUserId(this ControllerBase controller)
    {
        return controller.HttpContext.GetAuthenticatedUserId();
    }
    public static async Task<Account?> GetAuthenticatedAccount(this ControllerBase controller, DatabaseContext context)
    {
        return await controller.GetAuthenticatedUserId().Match(
            Some: async id => await context.Accounts.SingleOrDefaultAsync(u =>
                u.Id == id),
            None:Task.FromResult((Account?)null)
            );
    }


    public static Option<int> GetAuthenticatedUserId(this HttpContext context)
    {
        var idAsString = context.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idAsString)) return Option<int>.None;
        return Option<int>.Some(int.Parse(idAsString));
    }
}