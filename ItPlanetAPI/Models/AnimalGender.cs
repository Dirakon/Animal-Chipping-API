using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ItPlanetAPI.Models;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum AnimalGender
{
    [EnumMember(Value = "MALE")] Male,
    [EnumMember(Value = "FEMALE")] Female,
    [EnumMember(Value = "OTHER")] Other
}