using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Middleware.ValidationAttributes;
using ItPlanetAPI.Models;

namespace ItPlanetAPI.Requests;

public class AnimalUpdateRequest
{
    [Required] [Positive] public float Weight { get; set; }

    [Required] [Positive] public float Length { get; set; }

    [Required] [Positive] public float Height { get; set; }

    [Required] public AnimalGender Gender { get; set; }

    [Required] [Positive] public int ChipperId { get; set; }

    [Required] [Positive] public long ChippingLocationId { get; set; }

    [Required] public AnimalLifeStatus LifeStatus { get; set; }
}