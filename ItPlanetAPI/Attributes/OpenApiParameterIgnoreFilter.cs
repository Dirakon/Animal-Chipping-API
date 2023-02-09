namespace ItPlanetAPI;

public class OpenApiParameterIgnoreAttribute : System.Attribute
{
}

public class OpenApiParameterIgnoreFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        if (operation == null || context?.ApiDescription?.ParameterDescriptions == null)
            return;

        var parametersToHide = context.ApiDescription.ParameterDescriptions
            .Where(ParameterHasIgnoreAttribute)
            .ToList();

        if (parametersToHide.Count == 0)
            return;

        foreach (var parameter in parametersToHide.Select(parameterToHide => operation.Parameters.FirstOrDefault(parameter => string.Equals(parameter.Name, parameterToHide.Name, System.StringComparison.Ordinal))).Where(parameter => parameter != null))
        {
            operation.Parameters.Remove(parameter);
        }
    }

    private static bool ParameterHasIgnoreAttribute(Microsoft.AspNetCore.Mvc.ApiExplorer.ApiParameterDescription parameterDescription)
    {
        if (parameterDescription.ModelMetadata is Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadata metadata)
        {
            return metadata.Attributes.ParameterAttributes != null && metadata.Attributes.ParameterAttributes.Any(attribute => attribute.GetType() == typeof(OpenApiParameterIgnoreAttribute));
        }

        return false;
    }
}