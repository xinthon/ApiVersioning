using System.Security.Claims;
using ApiVersioning.Setup;
using Asp.Versioning;
using Asp.Versioning.Builder;
using ApiVersioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader
        .Combine(new UrlSegmentApiVersionReader(), 
        new HeaderApiVersionReader("X-ApiVersion"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
});


builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowCorsPolicy", builder => 
    {
        builder.SetIsOriginAllowed(origin => true);
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
    });
});

builder.Services.AddScoped<JwtProvider>();
// Authentication
builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// SwaggerGen
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<SwaggerGenOptionsSetup>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

// Define the API version set
ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(2, 0))
    .ReportApiVersions()
    .Build();

// Route group for version 1 and 2
RouteGroupBuilder versionGroup = app.MapGroup("/api/v{apiVersion:apiVersion}")
    .WithApiVersionSet(apiVersionSet)
    .HasApiVersion(1, 0)  // Version 1
    .HasApiVersion(2, 0); // Version 2

versionGroup.MapGet("generateToken", (JwtProvider _jwt) =>
{
    Claim[] claims = [
        new Claim(ClaimTypes.Name, "Test App"),
        new Claim(ClaimTypes.Uri, "https://localhost:7039"),
    ];

    return new 
    {
        accessToken = _jwt.GenerateToken(claims),
        refreshToken = _jwt.GenerateRefreshToken()
    };
});

// Endpoint for version 1
versionGroup.MapGet("/users", () =>
{
    return "This is version 1";
})
.MapToApiVersion(1, 0);

// Endpoint for version 2
versionGroup.MapGet("/users", () =>
{
    return "This is version 2";
})
.MapToApiVersion(2, 0);

app.Run();
