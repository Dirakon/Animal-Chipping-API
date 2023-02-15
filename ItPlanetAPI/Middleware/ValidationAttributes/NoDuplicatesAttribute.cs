using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Extensions;

namespace ItPlanetAPI.Middleware.ValidationAttributes;

public class NoDuplicatesAttribute : ValidationAttribute
{
    public NoDuplicatesAttribute()
    {
        ErrorMessage = "{0} has to be non-empty.";
    }


    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IEnumerable<object> items && !items.HasDuplicates()) return ValidationResult.Success;

        return new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName));
    }
}