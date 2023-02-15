using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Models;

namespace ItPlanetAPI.Middleware.ValidationAttributes;

public class AnimalLifeStatusAttribute : ValidationAttribute
{
    private readonly bool _allowNull;

    public AnimalLifeStatusAttribute(bool allowNull = false)
    {
        _allowNull = allowNull;
        ErrorMessage = "{0} should be either 'ALIVE' or 'DEAD'";
    }


    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value switch
        {
            string str when AnimalLifeStatus.IsValid(str) => ValidationResult.Success,
            null when _allowNull => ValidationResult.Success,
            _ => new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName))
        };
    }
}