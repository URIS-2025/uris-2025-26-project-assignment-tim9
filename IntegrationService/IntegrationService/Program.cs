using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using IntegrationService.Context;
using IntegrationService.Data;
using IntegrationService.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("IntegrationServiceConnection");
builder.Services.AddDbContext<IntegrationServiceContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<IIntegrationRepository, IntegrationRepository>();

// Kljucevi za enkripciju API kljuceva se perzistiraju na disk da bi ostali citljivi i
// posle restarta servisa (u kontejnerskom okruzenju ovaj folder treba da bude na volume-u).
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")));
builder.Services.AddScoped<IApiKeyProtector, DataProtectionApiKeyProtector>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IntegrationServiceContext>();
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
