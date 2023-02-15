using System.ComponentModel.DataAnnotations;

namespace ItPlanetAPI.Middleware.ValidationAttributes;

public class NonWhitespaceAttribute : ValidationAttribute
{
    public NonWhitespaceAttribute()
    {
        ErrorMessage = "{0} consist of whitespace-characters only.";
    }


    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value switch
        {
            string str when !string.IsNullOrWhiteSpace(str) => ValidationResult.Success,

            _ => new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName))
        };
    }
}