using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AccountRegistrationRequest
{
    [Required] [NonWhitespace] public string FirstName { get; set; }

    [Required] [NonWhitespace] public string LastName { get; set; }

    [Required] [NonWhitespace] public string Password { get; set; }

    [Required]
    [EmailAddress]
    [NonWhitespace]
    public string Email { get; set; }
}