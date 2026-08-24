var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

app.Run();

// Potrebno da bi WebApplicationFactory<Program> u integracionim testovima mogao da referencira
// ovu klasu (top-level statements generisu je kao internal po default-u).
public partial class Program { }
