using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Relyf.Api;
using Relyf.Api.Jwt;
using Relyf.Api.Security;
using Relyf.Service;
using Relyf.Service.CohereAi;
using Relyf.Service.Interfaces;
using Relyf.Repository.Infrastructure;
using Relyf.Repository.Dapper;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RelyfDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("RelyfDb")));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
var keyBytes = Convert.FromBase64String(jwt.Key);                 // key is stored as Base64 (user-secrets / env)
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
builder.Services.AddScoped<IMaterialRepository, MaterialRepository > ();
builder.Services.AddScoped<IItemMaterialRepository, ItemMaterialRepository>();
builder.Services.AddScoped<IProjectMaterialRepository, ProjectMaterialRepository>();
builder.Services.AddScoped<IIdeaSearchRepository, IdeaSearchRepository>();
builder.Services.AddScoped<IIdeaStatsRepository, IdeaStatsRepository>();
builder.Services.AddScoped<IUserDropoffRepository, UserDropoffRepository>();
builder.Services.AddScoped<IDropoffSiteRepository, DropoffSiteRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAdminLogsRepository, AdminLogsRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();












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
    o.AddPolicy(ClientCors, p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod());
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

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(ClientCors);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
