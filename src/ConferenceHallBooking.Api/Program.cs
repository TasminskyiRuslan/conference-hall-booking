using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using ConferenceHallBooking.Api.Authorization;
using ConferenceHallBooking.Api.ExceptionHandlers;
using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.Interfaces;
using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

// Fallback chain: appsettings first, then the JWT_SECRET_KEY environment variable.
// Missing both fails fast at startup instead of issuing unverifiable tokens.
var secretKey = !string.IsNullOrWhiteSpace(jwtSettings.SecretKey)
    ? jwtSettings.SecretKey
    : Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
      ?? throw new InvalidOperationException(
          "JWT secret key is not configured. Set JwtSettings:SecretKey in appsettings or the JWT_SECRET_KEY environment variable.");

builder.Services.PostConfigure<JwtSettings>(settings => settings.SecretKey = secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});
builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ProblemDetailsAuthorizationHandler>();

var authRateLimitSection = builder.Configuration.GetSection("RateLimiting:Auth");
var rateLimitingEnabled = builder.Configuration.GetValue("RateLimiting:Enabled", true);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (rejectedContext, _) =>
    {
        if (rejectedContext.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            rejectedContext.HttpContext.Response.Headers["Retry-After"] =
                ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
        }

        return ValueTask.CompletedTask;
    };

    var authFixedWindowOptions = new FixedWindowRateLimiterOptions
    {
        PermitLimit = rateLimitingEnabled
            ? authRateLimitSection.GetValue("PermitLimit", 10)
            : int.MaxValue,
        Window = TimeSpan.FromMinutes(authRateLimitSection.GetValue("WindowMinutes", 15)),
        QueueLimit = 0,
        AutoReplenishment = false,
    };

    options.AddPolicy("auth", httpContext =>
    {
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => authFixedWindowOptions);
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.SeedAsync();
    return;
}

app.UseExceptionHandler();

app.UseStatusCodePages(async statusContext =>
{
    var response = statusContext.HttpContext.Response;
    var request = statusContext.HttpContext.Request;
    var statusCode = response.StatusCode;

    if (response.HasStarted || statusCode < 400 || HttpMethods.IsHead(request.Method))
    {
        return;
    }

    var problemDetails = new ProblemDetails
    {
        Type = "https://tools.ietf.org/html/rfc7807",
        Status = statusCode,
        Title = ReasonPhrases.GetReasonPhrase(statusCode),
        Detail = statusCode switch
        {
            StatusCodes.Status401Unauthorized => "Authentication was required but no valid credentials were provided.",
            StatusCodes.Status404NotFound => "The requested resource was not found.",
            _ => $"The server produced no content for status code {statusCode}."
        },
        Extensions = { ["traceId"] = statusContext.HttpContext.TraceIdentifier }
    };

    await response.WriteAsJsonAsync(
        problemDetails,
        options: null,
        contentType: "application/problem+json",
        cancellationToken: statusContext.HttpContext.RequestAborted);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

/// <summary>Exposes the entry point to WebApplicationFactory-based integration tests.</summary>
public partial class Program
{
}
