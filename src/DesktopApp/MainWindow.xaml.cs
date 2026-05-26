using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace DesktopApp;

public partial class MainWindow : Window
{
    private readonly System.Timers.Timer _timer;
    private readonly HttpClient _httpClient = new();
    private readonly IConfiguration _config;
    private readonly string _baseUrl;
    private readonly string _measurementsEndpoint;
    private readonly string _healthEndpoint;

    public MainWindow()
    {
        InitializeComponent();

        // Load configuration
        _config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _baseUrl = _config["ApiSettings:BaseUrl"]?.TrimEnd('/');
        _measurementsEndpoint = _config["ApiSettings:Endpoints:Measurements"];
        _healthEndpoint = _config["ApiSettings:Endpoints:Health"] ;
        var interval = _config.GetValue<int>("ApiSettings:RefreshIntervalMs", 1000);

        _timer = new System.Timers.Timer(interval);
        _timer.Elapsed += async (s, e) => await RefreshDataAsync();
        _timer.Start();
    }

    private async Task RefreshDataAsync()
    {
        try
        {
            var url = $"{_baseUrl}/{_measurementsEndpoint}?type=HeartRate";
            
            var measurements = await _httpClient.GetFromJsonAsync<List<Measurement>>(url);

            await Dispatcher.InvokeAsync(() =>
            {
                dataGrid.ItemsSource = measurements;
            });
        }
        catch { /* swallow */ }
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Refreshing...");

        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/{_healthEndpoint}");

            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                MessageBox.Show("OK: " + json);
            }
            else
            {
                MessageBox.Show("Error: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Request failed: " + ex.Message);
        }
    }
}

public record Measurement(Guid MeasurementId, DateTimeOffset Timestamp, string DeviceId, string PatientId, string Type, object Value, string Unit);
