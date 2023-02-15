using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AccountSearchRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";

    [NonNegative] public int From { get; set; } = 0;

    [Positive] public int Size { get; set; } = 10;
}