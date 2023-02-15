using System.ComponentModel.DataAnnotations;

namespace ItPlanetAPI.Middleware.ValidationAttributes;

public class NonNegativeAttribute : ValidationAttribute
{
    private readonly bool _allowNull;

    public NonNegativeAttribute(bool allowNull = false)
    {
        _allowNull = allowNull;
        ErrorMessage = "{0} has to be non-negative.";
    }


    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value switch
        {
            float and >= 0 => ValidationResult.Success,
            long and >= 0 => ValidationResult.Success,
            int and >= 0 => ValidationResult.Success,
            double and >= 0 => ValidationResult.Success,
            null when _allowNull => ValidationResult.Success,
            _ => new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName))
        };
    }
}