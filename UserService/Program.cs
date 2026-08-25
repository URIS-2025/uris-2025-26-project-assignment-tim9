using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserService.Context;
using UserService.Data;
using UserService.ServiceCalls.Auth;
using UserService.ServiceCalls.Notification;
using UserService.Services;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(setupAction =>
{
    setupAction.SwaggerDoc("UserServiceOpenApiSpecification",
        new Microsoft.OpenApi.Models.OpenApiInfo()
        {
            Title = "User Service API",
            Version = "1",
            Description = "Pomoću ovog API-ja može da se vrši upravljanje korisnicima, ulogama i statusom naloga."
        });
});

// Servisi
builder.Services.AddScoped<IPasswordService, PasswordService>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Service calls
builder.Services.AddHttpClient<IAuthService, AuthService>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["Services:AuthService"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Placeholder adresa dok kolega ne definise pravu - uskladiti "Services:NotificationService"
// u appsettings.json kad Notification servis dobije pravi port/URL.
builder.Services.AddHttpClient<INotificationService, NotificationService>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["Services:NotificationService"];
    client.BaseAddress = new Uri(baseUrl ?? "http://localhost:5100/");
    client.Timeout = TimeSpan.FromSeconds(5);
});

// AutoMapper
builder.Services.AddAutoMapper(config => config.AddMaps(typeof(Program).Assembly));

// Database
builder.Services.AddDbContext<UserContext>();

// JWT autentifikacija (tokene izdaje AuthService, ovde se samo validiraju)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSection = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserContext>();
    if (context.Database.IsRelational())
    {
        context.Database.Migrate();
    }
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(setupAction =>
    {
        setupAction.SwaggerEndpoint("/swagger/UserServiceOpenApiSpecification/swagger.json", "User Service API");
        setupAction.RoutePrefix = "";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
public partial class Program { }
