using BacklogOptimizer.Api.Middleware;
using BacklogOptimizer.Infrastructure;

using Scalar.AspNetCore;

try
{
    Microsoft.Playwright.Program.Main(["install", "chromium"]);
}
catch (Exception ex)
{
    // Log warning, don't crash
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

const string devCorsPolicy = "WebDev";
builder.Services.AddCors(options => options.AddPolicy(devCorsPolicy, policy => policy
    .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));
// NOTE: AllowCredentials is intentionally OFF — we authenticate with
// Authorization: Bearer <jwt>, not cookies. If we ever switch to httpOnly
// cookies, this policy must add AllowCredentials() and explicit origins
// (AllowAnyOrigin is incompatible with credentials).

builder.Services
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// 1. Exception handler is outermost so CORS headers are still applied
//    (via OnStarting) to any error response GlobalExceptionHandler emits.
app.UseExceptionHandler();

// 2. HTTPS redirection only outside Development. In dev a 307 from
//    http://:5001 to https://:5000 mid-flight breaks CORS preflight.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 3. CORS BEFORE auth so OPTIONS preflight doesn't hit a 401.
app.UseCors(devCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

// 4. Endpoints last. Dev-only docs grouped together for readability.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

await app.RunAsync();