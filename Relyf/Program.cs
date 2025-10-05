using Relyf.Service;
using Relyf.Service.CohereAi;
using Relyf.Service.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// Allow large JSON bodies (e.g., 100 MB)
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 100 * 1024 * 1024);

builder.Services.Configure<CohereAiOptions>(builder.Configuration.GetSection("Cohere"));
builder.Services.AddHttpClient<ICohereClient, CohereAiClient>();
builder.Services.AddScoped<IUpcycleIdeaService, UpcycleIdeaService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Relyf.Api v1");
    c.RoutePrefix = "swagger";
});
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
