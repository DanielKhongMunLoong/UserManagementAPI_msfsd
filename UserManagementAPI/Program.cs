using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using System.Text;
using System.Text.Json;

using UserManagementAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure services, appsettings.json needs to be updated
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

// Register Authentication Services using token
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = Credentials.Issuer,

        ValidateAudience = true,
        ValidAudience = Credentials.Audience,

        ValidateLifetime = true,

        ValidateIssuerSigningKey = true,

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Credentials.SecretKey))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// Add middleware for error handling
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Global exception caught: {ex.Message}");
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var errorResponse = new { error = "Internal Server Error occured" };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
});
// Add middleware for AA
app.UseAuthentication(); 
app.UseAuthorization();
// Add middleware for Logging
app.UseHttpLogging();

app.MapControllers();

app.Run();