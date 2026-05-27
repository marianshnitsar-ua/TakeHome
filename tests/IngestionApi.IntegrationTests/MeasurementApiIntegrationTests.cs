using Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IngestionApi.IntegrationTests;

public class MeasurementApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MeasurementApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // factory.CreateClient() automatically handles the in-memory server setup
        _client = factory.CreateClient();
        
        // We must add the API key because our API now requires it!
        _client.DefaultRequestHeaders.Add("x-api-key", "local-dev");
    }

    [Fact]
    public async Task Post_And_Query_Measurement_Succeeds()
    {
        // 1. Arrange: Prepare a measurement to send
        var measurement = new Measurement(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "test-device",
            "patient-456",
            "HeartRate",
            75,
            "bpm"
        );

        // 2. Act: POST the measurement to the API
        var postResponse = await _client.PostAsJsonAsync("/api/v1/measurements", measurement);
        
        // 3. Assert: Verify the POST was successful (202 Accepted)
        Assert.Equal(HttpStatusCode.Accepted, postResponse.StatusCode);

        // 4. Act: Query measurements to see if our data is there
        var getResponse = await _client.GetAsync("/api/v1/measurements?type=HeartRate");
        var measurements = await getResponse.Content.ReadFromJsonAsync<List<Measurement>>();

        // 5. Assert: Verify the data
        Assert.NotNull(measurements);
        Assert.NotEmpty(measurements);
        Assert.Contains(measurements, m => m.MeasurementId == measurement.MeasurementId);
    }

    [Fact]
    public async Task Get_Healthz_Returns_Ok()
    {
        var response = await _client.GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_Without_ApiKey_Returns_Unauthorized()
    {
        // Arrange: Create a client without the API key header
        using var noAuthClient = _client.DefaultRequestHeaders.Contains("x-api-key") 
            ? new HttpClient() { BaseAddress = _client.BaseAddress } // Clone for test
            : _client; 
        
        // Simpler way for this test: just remove the header from current client for a moment
        _client.DefaultRequestHeaders.Remove("x-api-key");

        var measurement = new Measurement(Guid.NewGuid(), DateTimeOffset.UtcNow, "dev", "p", "Type", 1, "u");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/measurements", measurement);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        
        // Cleanup: add it back for other tests
        _client.DefaultRequestHeaders.Add("x-api-key", "local-dev");
    }

    [Fact]
    public async Task Post_Invalid_Measurement_Returns_BadRequest()
    {
        // Arrange: Measurement with empty DeviceId (fails validation)
        var invalidMeasurement = new Measurement(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            string.Empty, // Invalid!
            "patient-456",
            "HeartRate",
            75,
            "bpm"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/measurements", invalidMeasurement);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
