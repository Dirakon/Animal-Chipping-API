namespace ItPlanetAPI.Models;

public static class AnimalGender
{
    public const string Male = "MALE", Female = "FEMALE", Other = "OTHER";

    public static bool IsValid(string gender)
    {
        return gender is Male or Female or Other;
    }
}