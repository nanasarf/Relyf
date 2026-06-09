using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Relyf.Api;
using Relyf.Api.Jwt;
using Relyf.Api.Security;
using Relyf.Service;
using Relyf.Service.CohereAi;
using Relyf.Service.Interfaces;
using System.Text;
using Relyf.Repository.Infrastructure;
using Relyf.Repository.Dapper;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RelyfDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("RelyfDb")));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwt = jwtSection.Get<JwtOptions>();
if (jwt == null || string.IsNullOrWhiteSpace(jwt.Key))
    throw new InvalidOperationException("JWT Key is not configured. Set Jwt:Key via appsettings.json, environment, or user-secrets (dotnet user-secrets set 'Jwt:Key' <Base64>). Suggested: 64 random bytes Base64.");

// Provide clearer dev fallback: if key isn't Base64, attempt UTF8 bytes; reject obviously weak placeholder.
byte[] keyBytes;
try
{
    keyBytes = Convert.FromBase64String(jwt.Key);
}
catch
{
    if (jwt.Key.StartsWith("CHANGE-ME", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Jwt:Key placeholder detected. Replace with a secure Base64 64-byte key. Example: [Convert]::ToBase64String((New-Object byte[] 64 |% {0})) using RNG.");
    keyBytes = Encoding.UTF8.GetBytes(jwt.Key);
    if (keyBytes.Length < 32)
        throw new InvalidOperationException("Jwt:Key is not Base64 and its UTF8 length < 32 bytes (256 bits). Provide a stronger key.");
    Console.WriteLine("WARNING: Jwt:Key is not Base64; using raw UTF8 bytes. For production, supply a Base64-encoded 512-bit secret.");
}

var signingKey = new SymmetricSecurityKey(keyBytes);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

builder.Services.Configure<CohereAiOptions>(builder.Configuration.GetSection("Cohere"));
builder.Services.AddHttpClient<ICohereClient, CohereAiClient>();
builder.Services.AddScoped<IUpcycleIdeaService, UpcycleIdeaService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IAiIdeaRepository, AiIdeaRepository>();
builder.Services.AddScoped<ILookupRepository, LookupRepository>();
builder.Services.AddScoped<ICoherePromptRepository, CoherePromptRepository>();
builder.Services.AddScoped<IApiRequestLogRepository, ApiRequestLogRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectStepRepository, ProjectStepRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IReactionRepository, ReactionRepository>();
builder.Services.AddScoped<ISaveRepository, SaveRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<IItemMaterialRepository, ItemMaterialRepository>();
builder.Services.AddScoped<IProjectMaterialRepository, ProjectMaterialRepository>();
builder.Services.AddScoped<IIdeaSearchRepository, IdeaSearchRepository>();
builder.Services.AddScoped<IIdeaStatsRepository, IdeaStatsRepository>();
builder.Services.AddScoped<IUserDropoffRepository, UserDropoffRepository>();
builder.Services.AddScoped<IDropoffSiteRepository, DropoffSiteRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAdminLogsRepository, AdminLogsRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<ISavedAIIdeaRepository, SavedAIIdeaRepository>();
builder.Services.AddScoped<IFeedRepository, FeedRepository>();





// Bind connection string
builder.Services.Configure<DbConnectionOptions>(o =>
{
    o.Default = builder.Configuration.GetConnectionString("Default")!;
});

// Register factory as Singleton
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();


const string ClientCors = "Client";
builder.Services.AddCors(o =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    o.AddPolicy(ClientCors, p => p
        .WithOrigins(origins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()); // allow cookies / auth header scenarios
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Relyf.Api", Version = "v1" });

    // Bearer button in Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new()
    {
        [new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
        }
        ] = Array.Empty<string>()
    });
});

var app = builder.Build();

// Developer diagnostics
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Global exception handling middleware - return JSON errors instead of HTML
app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var log = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalException");
        log.LogError(ex, "Unhandled request exception for {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
        
        // Return JSON error response instead of HTML
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = 500;
        
        var errorResponse = new
        {
            error = "Internal server error",
            message = app.Environment.IsDevelopment() ? ex.Message : "An error occurred while processing your request.",
            details = app.Environment.IsDevelopment() ? ex.ToString() : null
        };
        
        await ctx.Response.WriteAsJsonAsync(errorResponse);
    }
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    var log = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AppLifetime");
    log.LogWarning("ApplicationStopping triggered");
});

// Enable Swagger in all environments for testing on Render
app.UseSwagger();
app.UseSwaggerUI();

// Ensure uploads directory exists and serve static files from it
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseHttpsRedirection();

app.UseCors(ClientCors);

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { status = "root-ok", utc = DateTime.UtcNow }));
app.MapControllers();

app.Run();app.Run();