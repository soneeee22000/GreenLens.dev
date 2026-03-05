using System.Threading.RateLimiting;
using GreenLens.Api.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using GreenLens.Core.Interfaces;
using GreenLens.Core.Services;
using GreenLens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration ---
builder.Configuration.AddEnvironmentVariables();

// --- Database ---
var connectionString = builder.Configuration["DATABASE_CONNECTION_STRING"]
    ?? "Data Source=greenlens.db";
builder.Services.AddDbContext<GreenLensDbContext>(options =>
    options.UseSqlite(connectionString));

// --- Services (Clean Architecture DI) ---
builder.Services.AddScoped<IEstimateRepository, EstimateRepository>();
builder.Services.AddScoped<CarbonEstimationService>();

// --- Controllers ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// --- Swagger / OpenAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GreenLens API",
        Version = "v1",
        Description = "Carbon Footprint Intelligence API for Azure cloud infrastructure. " +
                      "Estimates CO2e emissions and provides AI-powered reduction recommendations.",
        Contact = new OpenApiContact
        {
            Name = "Pyae Sone (Seon)",
        }
    });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "X-Api-Key",
        Type = SecuritySchemeType.ApiKey,
        Description = "API key for authentication. Pass in the X-Api-Key header."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
    foreach (var xmlFile in xmlFiles)
    {
        options.IncludeXmlComments(xmlFile);
    }
});

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// --- Rate Limiting ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 10;
    });
});

var app = builder.Build();

// --- Middleware Pipeline ---
app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "GreenLens API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors();
app.UseRateLimiter();
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.MapControllers();

// --- Auto-migrate database on startup (skip in testing) ---
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GreenLensDbContext>();
    db.Database.Migrate();
}

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
