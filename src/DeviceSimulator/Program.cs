using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Json;

// Set up the Host to manage DI and Configuration
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient();
    })
    .Build();

var config = host.Services.GetRequiredService<IConfiguration>();
var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();

var baseUrl = config["ApiSettings:BaseUrl"];
var apiKey = config["ApiSettings:ApiKey"];
var deviceId = config["Simulation:DeviceId"];
var patientId = config["Simulation:PatientId"];
var intervalSeconds = config.GetValue<int>("Simulation:IntervalSeconds", 2);
var hrMin = config.GetValue<int>("Simulation:HeartRateRange:Min", 60);
var hrMax = config.GetValue<int>("Simulation:HeartRateRange:Max", 100);

var http = httpClientFactory.CreateClient();
http.DefaultRequestHeaders.Add("x-api-key", apiKey);

var random = new Random();

Console.WriteLine($"Starting simulator for device {deviceId}...");
Console.WriteLine($"Targeting API: {baseUrl}");

while (true)
{
    try
    {
        var hr = new Measurement(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            deviceId!,
            patientId!,
            "HeartRate",
            random.Next(hrMin, hrMax),
            "bpm"
        );

        var response = await http.PostAsJsonAsync($"{baseUrl.TrimEnd('/')}/api/v1/measurements", hr);
        
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Data sent: {hr.Value} {hr.Unit}");
        }
        else
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error: {response.StatusCode}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] API is not available: {ex.Message}");
    }

    await Task.Delay(TimeSpan.FromSeconds(intervalSeconds));
}
