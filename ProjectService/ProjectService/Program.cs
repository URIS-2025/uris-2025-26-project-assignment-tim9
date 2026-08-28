using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectService.Context;
using ProjectService.Data;
using ProjectService.ServiceCalls.User;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(setupAction =>
{
    setupAction.SwaggerDoc("ProjectServiceOpenApiSpecification",
        new Microsoft.OpenApi.Models.OpenApiInfo()
        {
            Title = "Project Service API",
            Version = "1",
            Description = "Pomoću ovog API-ja može da se vrši upravljanje projektima, članovima projekta, milestone-ovima i zahtevima."
        });
});

// Repositories
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
builder.Services.AddScoped<IMilestoneRepository, MilestoneRepository>();
builder.Services.AddScoped<IRequirementsRepository, RequirementsRepository>();

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
builder.Services.AddDbContext<ProjectContext>();

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
    var context = scope.ServiceProvider.GetRequiredService<ProjectContext>();
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
        setupAction.SwaggerEndpoint("/swagger/ProjectServiceOpenApiSpecification/swagger.json", "Project Service API");
        setupAction.RoutePrefix = "";
    });
}

// Iza API Gateway-a (YARP) koji prosleđuje HTTP na :5230, UseHttpsRedirection
// vraća 307 na HTTPS port (:7065); browser prati taj cross-origin redirect,
// zaobilazi Gateway i gubi Authorization header -> CORS blok + 401.
// U Production-u (bez tog reverse-proxy hopa) redirect na HTTPS zadržavamo.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
public partial class Program { }