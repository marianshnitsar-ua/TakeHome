using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace DesktopApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly System.Timers.Timer _timer;

    [ObservableProperty]
    private string _status = string.Empty;

    public ObservableCollection<Measurement> Measurements { get; } = new();

    public MainViewModel(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;

        var interval = _config.GetValue<int>("ApiSettings:RefreshIntervalMs", 1000);
        
        _timer = new System.Timers.Timer(interval);
        _timer.Elapsed += async (s, e) => await RefreshDataAsync();
        _timer.Start();
    }

    private async Task RefreshDataAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _config["ApiSettings:BaseUrl"]?.TrimEnd('/');
            var endpoint = _config["ApiSettings:Endpoints:Measurements"];
            
            var url = $"{baseUrl}/{endpoint}?type=HeartRate";
            var results = await client.GetFromJsonAsync<List<Measurement>>(url);

            if (results != null)
            {
                // Update collection on UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Measurements.Clear();
                    foreach (var m in results)
                    {
                        Measurements.Add(m);
                    }
                });
            }
        }
        catch
        {
            Status = "Connection error...";
        }
    }

    [RelayCommand]
    private async Task CheckHealthAsync()
    {
        Status = "Checking health...";
        try
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _config["ApiSettings:BaseUrl"]?.TrimEnd('/');
            var endpoint = _config["ApiSettings:Endpoints:Health"];

            var response = await client.GetAsync($"{baseUrl}/{endpoint}");
            var content = await response.Content.ReadAsStringAsync();

            MessageBox.Show($"Status: {response.StatusCode}\nContent: {content}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Health check failed: {ex.Message}");
        }
        finally
        {
            Status = string.Empty;
        }
    }
}
