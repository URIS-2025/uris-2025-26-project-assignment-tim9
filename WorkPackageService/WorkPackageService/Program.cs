using Microsoft.EntityFrameworkCore;
using WorkPackageService.Context;
using WorkPackageService.Data;
using WorkPackageService.ServiceCalls.Notification;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("WorkPackageServiceConnection");
builder.Services.AddDbContext<WorkPackageServiceContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<IWorkPackageRepository, WorkPackageRepository>();
builder.Services.AddScoped<IBacklogRepository, BacklogRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IDependencyRepository, DependencyRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

// Placeholder adresa dok kolega ne definise pravu - uskladiti "ServiceUrls:NotificationService"
// u appsettings.json (ili appsettings.Development.json) kad Notification servis dobije pravi port/URL.
builder.Services.AddHttpClient<INotificationService, NotificationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:NotificationService"]
        ?? "http://localhost:5100/");
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
