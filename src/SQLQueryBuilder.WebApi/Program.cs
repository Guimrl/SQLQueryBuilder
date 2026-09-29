using System.Text.Json;
using System.Text.Json.Serialization;
using SQLQueryBuilder.Application.Queries;
using SQLQueryBuilder.WebApi.Extensions;
using SQLQueryBuilder.WebApi.Routing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.AddHandlers();
builder.AddRepositories();
builder.Services.AddControllers(options =>
    options.Conventions.Insert(0, new ApiRoutePrefixConvention("api")))
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter<WhereOperator>(JsonNamingPolicy.CamelCase, allowIntegerValues: false)));

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");
app.MapControllers();

app.Run();
