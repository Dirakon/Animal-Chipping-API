using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Models;

namespace ItPlanetAPI.Dtos;

public class AccountDto
{
    [Required] public string FirstName { get; set; }
    [Required] public string LastName { get; set; }
    [Required] public string Email { get; set; }
    [Required] public AccountRole Role { get; set; }
    [Required] public int Id { get; set; }
}