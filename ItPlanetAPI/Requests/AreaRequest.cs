using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Extensions.Geometry;
using ItPlanetAPI.Middleware.ValidationAttributes;
using ItPlanetAPI.Models;

namespace ItPlanetAPI.Requests;

public class AreaRequest : IValidatableObject
{
    [NonWhitespace] [Required] public string Name { get; set; }
    [Required] public List<AreaPointCreationRequest> AreaPoints { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AreaPoints
            .Any(firstAreaPoint =>
                AreaPoints
                    .Where(secondAreaPoint => secondAreaPoint != firstAreaPoint)
                    .Any(secondAreaPoint => secondAreaPoint.IsAlmostTheSameAs(firstAreaPoint))
            )
           )
            yield return new ValidationResult("'AreaPoints' contains duplicate elements");
        if (AreaPoints.Count < 3)
            yield return new ValidationResult("'AreaPoints' contains less then 3 elements");
        if (AreaPoints.AreOnOneLine())
            yield return new ValidationResult("'AreaPoints' elements are on one line");
        if (AreaPoints.AsSegments().ClosedShapeSelfIntersects())
            yield return new ValidationResult("'AreaPoints' elements self-intersect");
    }
}

public class AreaPointCreationRequest : ISpatial
{
    [Required] [Range(-180.0, 180.0)] public double Longitude { get; set; }
    [Required] [Range(-90.0, 90.0)] public double Latitude { get; set; }
}