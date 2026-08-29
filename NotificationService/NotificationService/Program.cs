using Microsoft.EntityFrameworkCore;
using NotificationService.Context;
using NotificationService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("NotificationServiceConnection");
// ServerVersion.AutoDetect opens its own throwaway connection to probe the server on every
// call - since this runs per-request (not once at startup), it was quietly leaking connections
// outside the pool until the server ran out and started timing out/502-ing. A fixed version
// (matching NotificationServiceContextFactory's design-time config) skips that probe entirely.
builder.Services.AddDbContext<NotificationServiceContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 35))));

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NotificationServiceContext>();
    if (context.Database.IsRelational())
    {
        context.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Potrebno da bi WebApplicationFactory<Program> u integracionim testovima mogao da referencira
// ovu klasu (top-level statements generisu je kao internal po default-u).
public partial class Program { }
