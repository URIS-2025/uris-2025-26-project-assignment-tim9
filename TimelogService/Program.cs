using TimelogService.Context;
using TimelogService.Data;
using TimelogService.ServiceCalls.Project;
using TimelogService.ServiceCalls.WorkPackage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<ITimelogRepository, TimelogRepository>();

builder.Services.AddAutoMapper(config => config.AddMaps(typeof(Program).Assembly));

builder.Services.AddDbContext<TimelogContext>();

builder.Services.AddHttpClient<IProjectService, ProjectService>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["Services:ProjectService"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddHttpClient<IWorkPackageService, WorkPackageService>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["Services:WorkPackageService"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
