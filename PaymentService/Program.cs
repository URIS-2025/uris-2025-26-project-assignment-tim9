using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PaymentService.Context;
using PaymentService.Data;
using PaymentService.ServiceCalls;
using PaymentService.ServiceCalls.Project;
using PaymentService.ServiceCalls.User;
using PaymentService.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//enumi se u JSON-u salju kao tekst ("Unpaid") umesto kao broj (0)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

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
        Description = "Nalepi accessToken iz POST /api/auth/login (AuthService) - bez rec Bearer, Swagger je sam dodaje."
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

//ucitava sve Profile klase iz ovog projekta
builder.Services.AddAutoMapper(config => config.AddMaps(typeof(Program).Assembly));

builder.Services.AddDbContext<PaymentContext>();

builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceItemRepository, InvoiceItemRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

//ista Jwt konfiguracija kao u ostalim servisima tima - tokene izdaje AuthService
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

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthForwardingHandler>();

//adrese servisa dolaze iz appsettings.json, timeout da nas tudji servis ne blokira
builder.Services.AddHttpClient<IUserService, UserService>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["Services:UserService"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
    client.Timeout = TimeSpan.FromSeconds(5);
})
    .AddHttpMessageHandler<AuthForwardingHandler>();

builder.Services.AddHttpClient<IProjectService, ProjectService>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["Services:ProjectService"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
    client.Timeout = TimeSpan.FromSeconds(5);
})
    .AddHttpMessageHandler<AuthForwardingHandler>();

var app = builder.Build();

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

//aplikacija koristi top-level statements, pa je klasa Program podrazumevano internal.
//ovim postaje dostupna WebApplicationFactory-ju u integracionim testovima.
public partial class Program { }
