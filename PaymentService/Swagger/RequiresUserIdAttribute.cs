namespace PaymentService.Swagger
{
    //oznacava akcije koje citaju X-User-Id iz zahteva, da bi Swagger prikazao polje za njega
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequiresUserIdAttribute : Attribute
    {
    }
}
