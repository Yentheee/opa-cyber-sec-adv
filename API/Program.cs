//-------start point //

// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.AspNetCore.Authorization;

// var builder = WebApplication.CreateBuilder(args);

// // Read values from appsettings.json
// var jwtAuthority = builder.Configuration["Jwt:Authority"];
// var jwtAudience = builder.Configuration["Jwt:Audience"];
// var corsOrigin = builder.Configuration["Cors:Origin"];

// // Add services to the container.

// builder.Services.AddControllers();
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// }).AddJwtBearer(options =>
// {
//     options.Authority = jwtAuthority;
//     options.Audience = jwtAudience;

// });

// builder.Services.AddAuthorization(options =>
// {
//     options.FallbackPolicy = new AuthorizationPolicyBuilder()
//         .RequireAuthenticatedUser()
//         .Build();
// });



// var app = builder.Build();

// // Configure the HTTP request pipeline.
// app.UseCors(options => options
//     .WithOrigins(corsOrigin)
//     .AllowAnyMethod()
//     .AllowAnyHeader());

// app.UseAuthentication();
// app.UseAuthorization();

// app.MapControllers();

// app.Run();




using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Opa.AspNetCore; // Assuming OPA.AspNetCore package is installed

var builder = WebApplication.CreateBuilder(args);

// Read values from appsettings.json
var jwtAuthority = builder.Configuration["Jwt:Authority"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var corsOrigin = builder.Configuration["Cors:Origin"];
var opaUrl = builder.Configuration["Opa:Url"]; // Add configuration for OPA URL

// Install OPA.AspNetCore package (if not already done)
builder.Services.AddOpa(options =>
{
    options.Url = opaUrl; // Set OPA URL
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = jwtAuthority;
    options.Audience = jwtAudience;
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(options => options
    .WithOrigins(corsOrigin)
    .AllowAnyMethod()
    .AllowAnyHeader());

// Add OPA middleware before authentication and authorization
app.UseOpa(async (context, opa) =>
{
    // Extract bearer token from Authorization header
    var authorizationHeader = context.Request.Headers["Authorization"];
    if (string.IsNullOrEmpty(authorizationHeader))
    {
        context.Response.StatusCode = StatusCodes.Unauthorized;
        return;
    }

    var token = authorizationHeader.ToString().Split(' ').ElementAtOrDefault(1);
    if (string.IsNullOrEmpty(token))
    {
        context.Response.StatusCode = StatusCodes.Unauthorized;
        return;
    }

    // Create input for OPA evaluation
    var input = new
    {
        user = context.User.Identity?.Name,
        role = context.User.Claims.FirstOrDefault(c => c.Type == "role")?.Value,
        // You can add more claims as needed
    };

    // Send input to OPA and await response
    var result = await opa.EvaluateAsync("barmanagement", input);

    if (!result.IsSuccess || result.Result == null)
    {
        context.Response.StatusCode = StatusCodes.Forbidden;
        return;
    }

    // Check if access is allowed
    if (result.Result.Get<bool>("allow_access"))
    {
        // Hier komt de logica voor het verwerken van de aanvraag
        await context.Response.WriteAsync("Access granted to the endpoint."); // Example response
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Forbidden;
        return;
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
