using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AuthService.Context;
using AuthService.Data;
using AuthService.ServiceCalls.User;
using AuthService.Services;

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
    setupAction.SwaggerDoc("AuthServiceOpenApiSpecification",
        new Microsoft.OpenApi.Models.OpenApiInfo()
        {
            Title = "Auth Service API",
            Version = "1",
            Description = "Pomoću ovog API-ja se vrši prijava korisnika, izdavanje i osvežavanje tokena, i upravljanje sesijama."
        });
});

// Servisi
builder.Services.AddScoped<ITokenService, TokenService>();

// Repositories
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

// Service calls
builder.Services.AddHttpClient<IUserService, UserService>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["Services:UserService"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
    client.Timeout = TimeSpan.FromSeconds(5);
});

// AutoMapper
builder.Services.AddAutoMapper(config => config.AddMaps(typeof(Program).Assembly));

// Database
builder.Services.AddDbContext<AuthContext>();

// JWT autentifikacija (AuthService izdaje tokene i takodje moze da ih validira)
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
    var context = scope.ServiceProvider.GetRequiredService<AuthContext>();
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
        setupAction.SwaggerEndpoint("/swagger/AuthServiceOpenApiSpecification/swagger.json", "Auth Service API");
        setupAction.RoutePrefix = "";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
public partial class Program { }
