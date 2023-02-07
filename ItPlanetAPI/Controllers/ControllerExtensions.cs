using System.Security.Claims;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;

namespace ItPlanetAPI.Controllers;

public static class ControllerExtensions
{
    public static Option<int> GetAuthenticatedUserId(this ControllerBase controller)
    {
        var idAsString = controller.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idAsString))
        {
            return Option<int>.None;
        }
        return Option<int>.Some(int.Parse(idAsString));
    }
}