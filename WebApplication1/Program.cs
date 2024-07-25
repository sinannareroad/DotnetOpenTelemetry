using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(x =>
{
    x.SetResourceBuilder(ResourceBuilder.CreateEmpty()
        .AddService("Weather Service")
        .AddAttributes(new Dictionary<string, object>()
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName
        }));
    
    x.IncludeScopes = true;
    x.IncludeFormattedMessage = true;
    
    x.AddOtlpExporter(config =>
    {
        config.Endpoint = new Uri("http://localhost:5341/ingest/otpl/v1/logs");
        config.Protocol = OtlpExportProtocol.HttpProtobuf;
        config.Headers = "X-Seq-ApiKey=PBmFMJ5dVtvDM33PxiVE";
    });  
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (string city, int days, ILogger<Program> logger) =>
    {
        var forecast = Enumerable.Range(1, days).Select(index =>
                new WeatherForecast
                (
                    city,
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        logger.LogInformation("Retrieved {WeatherCount} weather forecasts for {City}", forecast.Length, city);
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(string city, DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}