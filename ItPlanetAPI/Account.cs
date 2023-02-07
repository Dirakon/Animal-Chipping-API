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
}