using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace ItPlanetAPI.Middleware.ValidationAttributes;

public class NonEmptyAttribute : ValidationAttribute
{
    public NonEmptyAttribute()
    {
        ErrorMessage = "{0} has to be non-empty.";
    }


    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IList {Count: > 0} list) return ValidationResult.Success;

        return new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName));
    }
}