using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using ShopCoAPI.Data;
using ShopCoAPI.Hubs;
using ShopCoAPI.Service;
using ShopCoAPI.Services;
using ShopCoAPI.Services.AiChat;
using ShopCoAPI.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapibuilder
builder.Services.AddOpenApi();
builder.Services.AddSignalR(); // Registers SignalR services

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Essential for SignalR connections with auth
    });
});

// Database
builder.Services.AddDbContext<ShopCoDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Settings & Services
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.Configure<CloudflareR2Settings>(builder.Configuration.GetSection("CloudflareR2"));
builder.Services.AddScoped<IR2StorageService, R2StorageService>();

// AI Support Chat (Groq)
builder.Services.Configure<GroqSettings>(builder.Configuration.GetSection("Groq"));
builder.Services.AddScoped<IAiContextService, AiContextService>();
builder.Services.AddHttpClient<IGroqChatService, GroqChatService>();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings are not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuerSigningKey = true
        };

        // 🔑 Intercept the connection request to extract the JWT token for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our Hub, grab the token from the query string
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// CORS middleware must be registered before Authentication, Authorization, and endpoints mapping
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chat"); // Maps our real-time ChatHub to the "/chat" endpoint

app.Run();