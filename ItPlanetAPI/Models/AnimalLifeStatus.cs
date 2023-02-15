namespace ItPlanetAPI.Models;

public static class AnimalLifeStatus
{
    public const string Alive = "ALIVE", Dead = "DEAD";

    public static bool IsValid(string gender)
    {
        return gender is Alive or Dead;
    }
}