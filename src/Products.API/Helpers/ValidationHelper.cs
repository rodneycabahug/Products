using System.ComponentModel.DataAnnotations;

namespace Products.API.Helpers;

public static class ValidationHelper
{
    public static (bool IsValid, List<string> Errors) ValidateObject(object obj)
    {
        var validationContext = new ValidationContext(obj);
        var validationResults = new List<ValidationResult>();
        
        bool isValid = Validator.TryValidateObject(obj, validationContext, validationResults, validateAllProperties: true);
        
        var errors = validationResults.Select(vr => vr.ErrorMessage ?? "Validation error").ToList();
        
        return (isValid, errors);
    }
}
