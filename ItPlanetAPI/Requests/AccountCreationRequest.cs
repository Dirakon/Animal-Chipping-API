using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AccountCreationRequest
{
    [NonWhitespace] public required  string FirstName { get; set; }

    [NonWhitespace] public required  string LastName { get; set; }

    [NonWhitespace] public required  string Password { get; set; }

    [EmailAddress] [NonWhitespace] public required  string Email { get; set; }
}