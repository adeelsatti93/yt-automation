using Hangfire;
using KidsCartoonPipeline.API;
using KidsCartoonPipeline.API.Middleware;
using KidsCartoonPipeline.Infrastructure;
using KidsCartoonPipeline.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Kids Cartoon Pipeline API", Version = "v1" });
});

// Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// CORS
builder.Services.AddCors(opts => opts.AddPolicy("Frontend", p =>
    p.WithOrigins(builder.Configuration["Frontend:Url"] ?? "http://localhost:5173")
     .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Run migrations + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedAsync(scope.ServiceProvider);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseSerilogRequestLogging();
app.UseCors("Frontend");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Kids Cartoon Pipeline — Jobs",
    // Allow access without auth in development
    Authorization = [new HangfireAuthFilter()]
});
app.MapControllers();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program { }
