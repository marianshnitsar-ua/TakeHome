using Domain;

namespace IngestionApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseUrls("https://localhost:7296", "http://localhost:5265");

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<IMeasurementStore, InMemoryStore>();
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" }));

        app.MapPost("/api/v1/measurements", async (Measurement m, IMeasurementStore store, HttpContext ctx, IConfiguration config) =>
        {
            if (!ValidateApiKey(ctx, config))
                return Results.Unauthorized();

            if (!MeasurementValidator.IsValid(m))
                return Results.BadRequest("invalid measurement");

            await store.AddAsync(m);

            return Results.Accepted($"/api/v1/measurements/{m.MeasurementId}", m);
        });

        app.MapGet("/api/v1/measurements", async (string? type, DateTimeOffset? since, IMeasurementStore store, IConfiguration config) =>
        {
            var defaultMinutes = config.GetValue<int>("Storage:DefaultQueryMinutes", 5);
            var results = await store.QueryAsync(type, since ?? DateTimeOffset.UtcNow.AddMinutes(-defaultMinutes));
            return Results.Ok(results);
        });

        await app.RunAsync();
    }

    private static bool ValidateApiKey(HttpContext ctx, IConfiguration config)
    {
        return ctx.Request.Headers.TryGetValue("x-api-key", out var v) && v == config["Security:ApiKey"];
    }
}
