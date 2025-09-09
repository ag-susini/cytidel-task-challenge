using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Tasker.Api.Extensions;
using Tasker.Api.Hubs;
using Tasker.Api.Services;
using Tasker.Application;
using Tasker.Application.Extensions;
using Tasker.Application.Services.Interfaces;
using Tasker.Infrastructure.Extensions;
using Tasker.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.ConfigureSerilog();

// Add services to the container.

// Configure JSON options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add ProblemDetails support
builder.Services.AddProblemDetails();
builder.Services.AddCustomExceptionHandler();

// Add rate limiting
builder.Services.AddRateLimiting();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Location")
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// Add SignalR
builder.Services.AddSignalR();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var signingKey = jwtSettings["SigningKey"] ?? throw new InvalidOperationException("JWT SigningKey not configured");
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Tasker API", Version = "v1" });
    c.EnableAnnotations();
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TaskDbContext>("database");

// Add Real-time notifications (API layer concern)
builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

// Add Application services
builder.Services.AddApplication(builder.Configuration);

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Initialize infrastructure (migrations and seeding)
await app.InitializeInfrastructureAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tasker API V1");
    });
    app.UseCors("Development");
}
else
{
    app.UseHttpsRedirection();
}

// Add rate limiting
app.UseRateLimiter();

// Add request logging middleware
app.UseRequestLogging();

// Add ProblemDetails middleware
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TasksHub>("/hubs/tasks");

// Add health check endpoint
app.MapHealthChecks("/health");

app.Run();
