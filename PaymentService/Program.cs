using System.Text.Json.Serialization;
using PaymentService.Context;
using PaymentService.Data;
using PaymentService.ServiceCalls.Project;
using PaymentService.ServiceCalls.User;

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
builder.Services.AddSwaggerGen();

//ucitava sve Profile klase iz ovog projekta
builder.Services.AddAutoMapper(config => config.AddMaps(typeof(Program).Assembly));

builder.Services.AddDbContext<PaymentContext>();

builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceItemRepository, InvoiceItemRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

//adrese servisa dolaze iz appsettings.json, timeout da nas tudji servis ne blokira
builder.Services.AddHttpClient<IUserService, UserService>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["Services:UserService"];
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
