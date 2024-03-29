using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Middleware.ValidationAttributes;
using ItPlanetAPI.Models;

namespace ItPlanetAPI.Requests;

public class AccountCreationRequest
{
    [Required] [NonWhitespace] public string FirstName { get; set; }

    [Required] [NonWhitespace] public string LastName { get; set; }

    [Required] [NonWhitespace] public string Password { get; set; }

    [Required] public AccountRole Role { get; set; }

    [Required]
    [EmailAddress]
    [NonWhitespace]
    public string Email { get; set; }
}