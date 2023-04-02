using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalLocationUpdateRequest
{
    [Required] [Positive] public long VisitedLocationPointId { get; set; }

    [Required] [Positive] public long LocationPointId { get; set; }
}