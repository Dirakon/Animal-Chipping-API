using Microsoft.AspNetCore.Mvc;

namespace ItPlanetAPI;

public class Account
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int Id { get; set; }

    public override string ToString()
    {
        return $"{FirstName},{LastName},{Email},{Password},{Id}";
    }

    public AccountWithoutPassword GetWithoutPassword()
    {
        return AccountWithoutPassword.From(this);
    }
}

public class AccountWithoutPassword
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int Id { get; set; }
    
    public static AccountWithoutPassword From(Account account)
    {
        return new AccountWithoutPassword()
        {
            Email = account.Email,
            FirstName = account.FirstName,
            Id = account.Id,
            LastName = account.LastName
        };
    }
}