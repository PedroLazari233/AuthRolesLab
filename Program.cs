using System.Security.Claims;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

var authBuilder = builder.Services.AddAuthentication();

authBuilder.AddMicrosoftIdentityWebApi(
    builder.Configuration.GetSection("AzureAd"),
    jwtBearerScheme: "Entra");

// Which schemes the policies accept
string[] schemes = builder.Environment.IsDevelopment()
    ? ["Bearer", "Entra"]
    : ["Entra"];

// Authorization: every policy says which schemes to try
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(schemes)
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly", policy => policy
        .AddAuthenticationSchemes(schemes)
        .RequireRole("Admin"));
});

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    authBuilder.AddJwtBearer();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/public", () => "public");

app.MapGet("/private", () => "private").RequireAuthorization();

app.MapGet("/admin", () => "Admin").RequireAuthorization("AdminOnly");

app.MapGet("/me", (ClaimsPrincipal user) =>
{
    return new { Name = user.Identity?.Name, IsAdmin = user.IsInRole("Admin"), Claims = user.Claims.Select(c => new { c.Type, c.Value }) };
}).RequireAuthorization();

app.Run();