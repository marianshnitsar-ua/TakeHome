using Domain;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace DesktopApp;

public partial class MainWindow : Window
{
    private readonly System.Timers.Timer _timer;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly string _baseUrl;
    private readonly string _measurementsEndpoint;
    private readonly string _healthEndpoint;

    // dependencies are now injected via constructor
    public MainWindow(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        InitializeComponent();

        _config = config;
        _httpClient = httpClientFactory.CreateClient();

        _baseUrl = _config["ApiSettings:BaseUrl"]?.TrimEnd('/');
        _measurementsEndpoint = _config["ApiSettings:Endpoints:Measurements"];
        _healthEndpoint = _config["ApiSettings:Endpoints:Health"];
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
            var url = $"{_baseUrl}/{_healthEndpoint}";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                MessageBox.Show("OK: " + content);
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
