using System.ComponentModel.DataAnnotations;
using ItPlanetAPI.Models;

namespace ItPlanetAPI.Middleware.ValidationAttributes;

public class AnimalGenderAttribute : ValidationAttribute
{
    private readonly bool _allowNull;

    public AnimalGenderAttribute(bool allowNull = false)
    {
        _allowNull = allowNull;
        ErrorMessage = $"{0} should be either '{AnimalGender.Male}', '{AnimalGender.Female}' or '{AnimalGender.Other}'";
    }


    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value switch
        {
            string str when AnimalGender.IsValid(str) => ValidationResult.Success,
            null when _allowNull => ValidationResult.Success,
            _ => new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName))
        };
    }
}