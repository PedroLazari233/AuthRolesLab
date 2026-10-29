using System.Security.Claims;
using System.Xml.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization(options => {
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/public", () => "public");

app.MapGet("/private", () => "private").RequireAuthorization();

app.MapGet("/admin", () => "admin").RequireAuthorization("AdminOnly");

app.MapGet("/me", (ClaimsPrincipal user) =>
{
    return new { Name = user.Identity?.Name, IsAdmin = user.IsInRole("admin"), Claims = user.Claims.Select(c => new { c.Type, c.Value }) };
}).RequireAuthorization();

app.Run();