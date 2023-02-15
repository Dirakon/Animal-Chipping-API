using System.ComponentModel.DataAnnotations;

namespace ItPlanetAPI.Models;

public class AccountCreationRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public bool IsValid()
    {
        var allStringFields = new[] {Email, Password, FirstName, LastName};

        var allFieldsNonEmpty = allStringFields
            .All(field => !string.IsNullOrWhiteSpace(field));

        return allFieldsNonEmpty && IsValidEmail(Email);
    }


    private static bool IsValidEmail(string source)
    {
        return new EmailAddressAttribute().IsValid(source);
    }
}