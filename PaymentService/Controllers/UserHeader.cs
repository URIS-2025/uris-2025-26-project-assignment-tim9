using System.Security.Claims;

namespace PaymentService.Controllers
{
    //identitet korisnika koji izvrsava akciju
    public static class UserHeader
    {
        public const string Name = "X-User-Id";

        public static bool TryGetUserId(this HttpRequest request, out Guid userId)
        {
            userId = Guid.Empty;

            //identitet iz tokena ima prednost jer je proveren potpisom
            var subject = request.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? request.HttpContext.User.FindFirst("sub")?.Value;

            if (Guid.TryParse(subject, out userId))
            {
                return true;
            }

            //rezerva: header koji postavlja API Gateway
            return request.Headers.TryGetValue(Name, out var value)
                && Guid.TryParse(value, out userId);
        }
    }
}
