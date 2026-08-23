using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TimelogService.SwaggerSupport
{
    public class XUserIdHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var method = context.ApiDescription.HttpMethod;
            if (!string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(method, "PUT", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-User-Id",
                In = ParameterLocation.Header,
                Required = string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase),
                Description = "The acting user's GUID - normally injected by the API Gateway after authenticating the caller.",
                Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
            });
        }
    }
}
