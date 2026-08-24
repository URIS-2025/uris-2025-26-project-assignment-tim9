using AttachmentService.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AttachmentService.Swagger
{
    public class UserIdHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.DeclaringType != typeof(AttachmentController))
            {
                return;
            }

            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-User-Id",
                In = ParameterLocation.Header,
                Required = true,
                Description = "The acting user's id. Normally injected by the API Gateway after authenticating the caller - for manual testing, paste a real User Id (GUID) from UserService here.",
                Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
            });
        }
    }
}
