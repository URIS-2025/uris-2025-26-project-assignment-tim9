using System.Text;
using Amazon.S3;
using AttachmentService.Context;
using AttachmentService.Data;
using AttachmentService.ServiceCalls.Project;
using AttachmentService.ServiceCalls.User;
using AttachmentService.ServiceCalls.WorkPackage;
using AttachmentService.Storage;
using AttachmentService.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the accessToken from POST /api/auth/login (AuthService) here - no need to type \"Bearer \" first."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
    options.OperationFilter<UserIdHeaderOperationFilter>();
});

builder.Services.AddAutoMapper(config => config.AddMaps(typeof(Program).Assembly));
builder.Services.AddDbContext<AttachmentContext>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();

builder.Services.Configure<ObjectStorageOptions>(builder.Configuration.GetSection("ObjectStorage"));

static AmazonS3Client BuildS3Client(ObjectStorageOptions options, string serviceUrl)
{
    var config = new AmazonS3Config
    {
        ServiceURL = serviceUrl,
        ForcePathStyle = options.ForcePathStyle,
        UseHttp = serviceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
    };
    return new AmazonS3Client(options.AccessKey, options.SecretKey, config);
}

// Server-to-server client (HeadObject, etc.) - talks to storage over
// options.ServiceUrl, which in docker-compose is the internal container
// hostname (e.g. "http://minio:9000").
builder.Services.AddKeyedSingleton<IAmazonS3>(S3ClientKeys.Internal, (sp, _) =>
{
    var options = sp.GetRequiredService<IOptions<ObjectStorageOptions>>().Value;
    return BuildS3Client(options, options.ServiceUrl);
});

// Presigning-only client - its ServiceUrl is baked into the Host header of the
// signature, so it must use whatever endpoint the browser can actually reach
// (options.EffectivePublicServiceUrl), not the internal one.
builder.Services.AddKeyedSingleton<IAmazonS3>(S3ClientKeys.Public, (sp, _) =>
{
    var options = sp.GetRequiredService<IOptions<ObjectStorageOptions>>().Value;
    return BuildS3Client(options, options.EffectivePublicServiceUrl);
});
builder.Services.AddScoped<IFileStorageService, S3FileStorageService>();

builder.Services.AddHttpClient<ITaskService, TaskService>((sp, client) =>
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
builder.Services.AddHttpClient<IUserService, UserService>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["Services:UserService"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
    client.Timeout = TimeSpan.FromSeconds(5);
});

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
    var context = scope.ServiceProvider.GetRequiredService<AttachmentContext>();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Top-level statements make Program implicitly internal - this makes it accessible to
// WebApplicationFactory<Program> in AttachmentService.Tests. No behavior change.
public partial class Program { }
