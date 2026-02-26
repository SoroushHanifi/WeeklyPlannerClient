using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WeeklyPlannerAPI.Data;
using WeeklyPlannerAPI.Middleware;
using WeeklyPlannerAPI.Repositories;
using WeeklyPlannerAPI.Repositories.Interfaces;
using WeeklyPlannerAPI.Services;
using WeeklyPlannerAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(3)));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository,      UserRepository>();
builder.Services.AddScoped<IWeekRepository,      WeekRepository>();
builder.Services.AddScoped<ITimeBlockRepository, TimeBlockRepository>();
builder.Services.AddScoped<IWeekTaskRepository,  WeekTaskRepository>();
builder.Services.AddScoped<IWeekGoalRepository,  WeekGoalRepository>();
builder.Services.AddScoped<IWeekNoteRepository,  WeekNoteRepository>();
builder.Services.AddScoped<ILookupRepository,    LookupRepository>();

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService,      AuthService>();
builder.Services.AddScoped<IUserService,      UserService>();
builder.Services.AddScoped<IWeekService,      WeekService>();
builder.Services.AddScoped<ITimeBlockService, TimeBlockService>();
builder.Services.AddScoped<IWeekTaskService,  WeekTaskService>();
builder.Services.AddScoped<IWeekGoalService,  WeekGoalService>();
builder.Services.AddScoped<IWeekNoteService,  WeekNoteService>();
builder.Services.AddScoped<ILookupService,    LookupService>();

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey   = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(secretKey),
            ValidateIssuer           = true,
            ValidIssuer              = jwtSettings["Issuer"],
            ValidateAudience         = true,
            ValidAudience            = jwtSettings["Audience"],
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero,
        };

        // ── خواندن Token از HttpOnly Cookie ──────────────────────────────────
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // اگر Cookie وجود داشت از آن استفاده کن
                if (context.Request.Cookies.TryGetValue("wp_token", out var cookieToken)
                    && !string.IsNullOrWhiteSpace(cookieToken))
                {
                    context.Token = cookieToken;
                }
                return Task.CompletedTask;
            },

            // پاسخ ۴۰۱ به جای Redirect (مهم برای API)
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    """{"success":false,"message":"احراز هویت لازم است. لطفاً وارد شوید."}""");
            },
        };
    });

builder.Services.AddAuthorization();

// ── Controllers + JSON ────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNamingPolicy      = JsonNamingPolicy.CamelCase;
        opt.JsonSerializerOptions.DefaultIgnoreCondition    = JsonIgnoreCondition.WhenWritingNull;
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000", "http://localhost:5173", "http://weekly.mblt.ir", "https://weekly.mblt.ir" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));
// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Weekly Planner API",
        Version     = "v1",
        Description = "API برنامه‌ریزی هفتگی",
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "توکن JWT را اینجا وارد کنید (بدون Bearer)",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weekly Planner API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
