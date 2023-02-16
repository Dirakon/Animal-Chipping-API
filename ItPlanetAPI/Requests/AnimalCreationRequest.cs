using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Extensions;
using ItPlanetAPI.Middleware.ValidationAttributes;

namespace ItPlanetAPI.Requests;

public class AnimalCreationRequest : IValidatableObject
{
    [NonEmpty] public required  List<long> AnimalTypes { get; set; }

    [Positive] public required  float Weight { get; set; }

    [Positive] public required  float Length { get; set; }

    [Positive] public required  float Height { get; set; }

    [AnimalGender] public required  string Gender { get; set; }

    [Positive] public required  int ChipperId { get; set; }

    [Positive] public required  long ChippingLocationId { get; set; }

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