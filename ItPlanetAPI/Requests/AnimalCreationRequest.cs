using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Extensions;
using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalCreationRequest : IValidatableObject
{
    [Required] [NonEmpty] public List<long> AnimalTypes { get; set; }

    [Required] [Positive] public float Weight { get; set; }

    [Required] [Positive] public float Length { get; set; }

    [Required] [Positive] public float Height { get; set; }

    [Required] [AnimalGender] public string Gender { get; set; }

    [Required] [Positive] public int ChipperId { get; set; }

    [Required] [Positive] public long ChippingLocationId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AnimalTypes.Any(animalType => animalType <= 0))
            yield return new ValidationResult("'AnimalTypes' field should contain only positive items");
    }


    public bool HasConflicts()
    {
        return AnimalTypes.HasDuplicates();
    }
}