

using AuctionApi.Data;
using AuctionApi.Hubs;
using AuctionApi.Services;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ✅ 1. Add CORS (must be defined before UseCors)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ✅ 2. Add Controllers + Fix JSON serialization to avoid circular references
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// ✅ 3. Add OpenAPI (Swagger)
builder.Services.AddOpenApi();

// ✅ 4. Add SignalR
builder.Services.AddSignalR();

// ✅ 5. Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ 6. Add JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// ✅ 7. Add Authorization
builder.Services.AddAuthorization();

builder.Services.AddHostedService<AuctionBackgroundService>();
builder.Services.AddScoped<EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // ✅ Map OpenAPI with optional caching
    app.MapOpenApi()
        .ExcludeFromDescription(); // Optional: hide OpenAPI endpoint from its own docs
}

// ✅ Middleware Order is Critical:
app.UseHttpsRedirection();

app.UseStaticFiles(); // Serve wwwroot/images

app.UseCors("AllowFrontend"); // ← Must come before UseAuthentication

app.UseAuthentication();
app.UseAuthorization();

// ✅ Map Controllers and SignalR Hub
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.UseExceptionHandler("/error");
// Optional: Cache OpenAPI doc in dev
// app.UseOutputCache();

app.Run();