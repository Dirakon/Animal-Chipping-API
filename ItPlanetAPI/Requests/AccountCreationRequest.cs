using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AccountCreationRequest
{
    [NonWhitespace] public string FirstName { get; set; }

    [NonWhitespace] public string LastName { get; set; }

    [NonWhitespace] public string Password { get; set; }

    [EmailAddress] [NonWhitespace] public string Email { get; set; }
}