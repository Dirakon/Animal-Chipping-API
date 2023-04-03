namespace ItPlanetAPI.Models;

public class Account
{
    public Account()
    {
        ChippedAnimals = new List<Animal>();
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int Id { get; set; }
    public AccountRole Role { get; set; } = AccountRole.User;
    public virtual ICollection<Animal> ChippedAnimals { get; set; }

    public override string ToString()
    {
        return $"{FirstName},{LastName},{Email},{Password},{Id}";
    }
}