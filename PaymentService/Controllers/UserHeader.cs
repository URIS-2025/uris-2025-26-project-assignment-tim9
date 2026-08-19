namespace PaymentService.Controllers
{
    //identitet korisnika stize kroz header koji postavlja API Gateway
    public static class UserHeader
    {
        public const string Name = "X-User-Id";

        public static bool TryGetUserId(this HttpRequest request, out Guid userId)
        {
            userId = Guid.Empty;

            return request.Headers.TryGetValue(Name, out var value)
                && Guid.TryParse(value, out userId);
        }
    }
}
