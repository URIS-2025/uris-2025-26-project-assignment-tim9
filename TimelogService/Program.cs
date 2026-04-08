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

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddDbContext<TimelogContext>();

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IWorkPackageService, WorkPackageService>();

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
