using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using PaymentService.Controllers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PaymentService.Swagger
{
    //dodaje polje za X-User-Id u Swagger UI na akcijama oznacenim sa [RequiresUserId].
    //header inace ne bi bio vidljiv, jer se ne cita kao parametar akcije nego iz zahteva.
    public class UserIdHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var requiresUserId = context.MethodInfo
                .GetCustomAttributes(typeof(RequiresUserIdAttribute), false)
                .Any();

            if (!requiresUserId)
            {
                return;
            }

            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = UserHeader.Name,
                In = ParameterLocation.Header,
                Required = true,
                Description = "ID korisnika koji izvrsava akciju. U produkciji ga postavlja API Gateway.",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "uuid",
                    Default = new OpenApiString("66666666-6666-6666-6666-666666666666")
                }
            });
        }
    }
}
