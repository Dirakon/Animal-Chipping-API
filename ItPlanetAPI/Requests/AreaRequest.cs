using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Extensions;
using ItPlanetAPI.Extensions.Geometry;
using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AreaRequest : IValidatableObject
{
    [NonWhitespace] [Required] public string Name { get; set; }

    [Required] public List<AreaPointCreationRequest> AreaPoints { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AreaPoints.HasDuplicates((point1,point2)=>point1.IsAlmostTheSameAs(point2)))
            yield return new ValidationResult("'AreaPoints' contains duplicate elements");
        if (AreaPoints.Count < 3)
            yield return new ValidationResult("'AreaPoints' contains less then 3 elements");
        if (AreaPoints.AreOnOneLine())
            yield return new ValidationResult("'AreaPoints' elements are on one line");
        if (AreaPoints.AsSegments().ClosedShapeSelfIntersects())
            yield return new ValidationResult("'AreaPoints' elements self-intersect");
    }
}