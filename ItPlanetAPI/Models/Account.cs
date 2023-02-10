using System.ComponentModel.DataAnnotations;

namespace ItPlanetAPI.Models;

public class Account
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int Id { get; set; }
    public virtual ICollection<Animal> ChippedAnimals { get; set; }

    public Account()
    {
        ChippedAnimals = new List<Animal>();
    }

    public override string ToString()
    {
        return $"{FirstName},{LastName},{Email},{Password},{Id}";
    }
}

public class AccountRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public bool IsValid()
    {
        var allStringFields = new[] {Email, Password, FirstName, LastName};

        var allFieldsNonEmpty = allStringFields
            .Select(field => field.Trim())
            .All(field => field != "");

        return allFieldsNonEmpty && IsValidEmail(Email);
    }


    private static bool IsValidEmail(string source)
    {
        return new EmailAddressAttribute().IsValid(source);
    }
}

public class AccountDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int Id { get; set; }
}