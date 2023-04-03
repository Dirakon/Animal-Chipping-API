using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ItPlanetAPI.Models;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum AccountRole
{
    [EnumMember(Value = "ADMIN")] Admin,
    [EnumMember(Value = "CHIPPER")] Chipper,
    [EnumMember(Value = "USER")] User
}