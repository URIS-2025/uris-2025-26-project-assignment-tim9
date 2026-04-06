using System.Globalization;
using ProjectService.Context;
using ProjectService.Data;
using ProjectService.ServiceCalls.User;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);
// var config = builder.Configuration; // potrebno za JWT

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
builder.Services.AddScoped<IUserService, UserService>();

// AutoMapper
builder.Services.AddAutoMapper(config => config.AddMaps(typeof(Program).Assembly));

// Database
builder.Services.AddDbContext<ProjectContext>();

//builder.Services.AddAuthentication

var app = builder.Build();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
