using Asp.Versioning;

using Microsoft.EntityFrameworkCore;

using MohCovidInsights.Api.Behaviours;
using MohCovidInsights.Application;
using MohCovidInsights.Application.Abstractions.Interfaces;
using MohCovidInsights.Infrastructure;
using MohCovidInsights.Infrastructure.Persistence;

using Scalar.AspNetCore;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddRdsSecretIfPresent();

builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMemoryCache();
builder.Services.AddScoped(typeof(IQueryBehaviour<,>), typeof(LoggingBehaviour<,>));
builder.Services.AddScoped(typeof(IQueryBehaviour<,>), typeof(CachingBehaviour<,>));

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull);

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
}).AddMvc();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

const string DevCors = "dev-spa";
builder.Services.AddCors(o => o.AddPolicy(DevCors, policy => policy
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
}).AddMvc();

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddResponseCompression();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseCors(DevCors);

    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (app.Configuration.GetValue("Database:SeedDevData", true))
        await scope.ServiceProvider.GetRequiredService<DevDataSeeder>().SeedAsync();
}

app.MapControllers();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

await app.RunAsync();

public partial class Program;