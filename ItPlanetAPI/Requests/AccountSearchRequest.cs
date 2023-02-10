namespace ItPlanetAPI.Controllers;

public class AccountSearchRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string EmailName { get; set; } = "";
    public int From { get; set; } = 0;
    public int Size { get; set; } = 10;
}