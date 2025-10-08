using Microsoft.EntityFrameworkCore;          
using Relyf.Api;                  

using Relyf.Service;
using Relyf.Service.CohereAi;
using Relyf.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Allow large JSON bodies (e.g., 100 MB)
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 100 * 1024 * 1024);

// 1) Configure Cohere DI (unchanged)
builder.Services.Configure<CohereAiOptions>(builder.Configuration.GetSection("Cohere"));
builder.Services.AddHttpClient<ICohereClient, CohereAiClient>();
builder.Services.AddScoped<IUpcycleIdeaService, UpcycleIdeaService>();

// 2) Register EF Core DbContext (NEW)
builder.Services.AddDbContext<RelyfDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("RelyfDb")));

// 3) Usual ASP.NET stuff
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Relyf.Api v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.MapControllers();

// 4) Quick DB connectivity probe (NEW)
app.MapGet("/ping-db", async (RelyfDbContext db) =>
{
    var ok = await db.Database.CanConnectAsync();
    return new { ok };
});

app.Run();
