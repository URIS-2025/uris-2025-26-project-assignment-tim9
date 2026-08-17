using Amazon.S3;
using AttachmentService.Context;
using AttachmentService.Data;
using AttachmentService.ServiceCalls.Project;
using AttachmentService.ServiceCalls.WorkPackage;
using AttachmentService.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(config => config.AddMaps(typeof(Program).Assembly));
builder.Services.AddDbContext<AttachmentContext>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();

builder.Services.Configure<ObjectStorageOptions>(builder.Configuration.GetSection("ObjectStorage"));
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var options = sp.GetRequiredService<IOptions<ObjectStorageOptions>>().Value;
    var config = new AmazonS3Config
    {
        ServiceURL = options.ServiceUrl,
        ForcePathStyle = options.ForcePathStyle,
        UseHttp = options.ServiceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
    };
    return new AmazonS3Client(options.AccessKey, options.SecretKey, config);
});
builder.Services.AddScoped<IFileStorageService, S3FileStorageService>();

builder.Services.AddHttpClient<IWorkPackageService, WorkPackageService>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["Services:WorkPackageService"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddHttpClient<IProjectService, ProjectService>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["Services:ProjectService"];
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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Top-level statements make Program implicitly internal - this makes it accessible to
// WebApplicationFactory<Program> in AttachmentService.Tests. No behavior change.
public partial class Program { }
