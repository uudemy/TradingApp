using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TradingApp.Api.Extensions;
using TradingApp.Api.Middleware;
using TradingApp.Application;
using TradingApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using TradingApp.Infrastructure.Persistence;
using TradingApp.Infrastructure.Persistence.Seed;
using TradingApp.Api.Hubs;
var builder = WebApplication.CreateBuilder(args);

// ---------------- Serilog ----------------
builder.Host.UseSerilog((ctx, services, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .ReadFrom.Services(services)
       .Enrich.FromLogContext());

// ---------------- Katmanlar ----------------
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// ---------------- API ----------------
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<TradingApp.Application.Abstractions.IMarketBroadcaster,
                              TradingApp.Api.Hubs.SignalRMarketBroadcaster>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();
builder.Services.AddAppHealthChecks(builder.Configuration);

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

// ---------------- Pipeline ----------------
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseDefaultFiles();   // / → /index.html
app.UseStaticFiles();    // wwwroot içeriğini sunar

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TradingApp API v1"));
}

app.UseSerilogRequestLogging();
app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MarketHub>("/hubs/market");
app.MapAppHealthChecks();

// --- Seed (Development'ta otomatik) ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db, loggerFactory.CreateLogger("DataSeeder"));
}

app.Run();

// Integration testler için görünürlük
public partial class Program;