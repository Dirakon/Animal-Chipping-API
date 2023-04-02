using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ItPlanetAPI.Models;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum AnimalLifeStatus
{
    [EnumMember(Value = "ALIVE")] Alive,
    [EnumMember(Value = "DEAD")] Dead
}