using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ItPlanetAPI;

public class OpenApiParameterIgnoreAttribute : Attribute
{
}

public class OpenApiParameterIgnoreFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation == null || context?.ApiDescription?.ParameterDescriptions == null)
            return;

        var parametersToHide = context.ApiDescription.ParameterDescriptions
            .Where(ParameterHasIgnoreAttribute)
            .ToList();

        if (parametersToHide.Count == 0)
            return;

        foreach (var parameter in parametersToHide
                     .Select(parameterToHide => operation.Parameters.FirstOrDefault(parameter =>
                         string.Equals(parameter.Name, parameterToHide.Name, StringComparison.Ordinal)))
                     .Where(parameter => parameter != null)) operation.Parameters.Remove(parameter);
    }

    private static bool ParameterHasIgnoreAttribute(ApiParameterDescription parameterDescription)
    {
        if (parameterDescription.ModelMetadata is DefaultModelMetadata metadata)
            return metadata.Attributes.ParameterAttributes != null &&
                   metadata.Attributes.ParameterAttributes.Any(attribute =>
                       attribute.GetType() == typeof(OpenApiParameterIgnoreAttribute));

        return false;
    }
}