using System.Text;
using ConferenceHallBooking.Api.Authorization;
using ConferenceHallBooking.Api.ExceptionHandlers;
using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;

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
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ProblemDetailsAuthorizationHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Configuration.GetValue("Database:AutoMigrateAndSeed", false))
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.SeedAsync();
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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>Exposes the entry point to WebApplicationFactory-based integration tests.</summary>
public partial class Program
{
}
