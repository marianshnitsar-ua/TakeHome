using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Threading;

namespace DesktopApp;

public partial class MainWindow : Window
{
    private readonly System.Timers.Timer _timer;
    private readonly WebClient _client = new WebClient(); // sync, no DI
    private readonly IConfiguration _config;

    public MainWindow()
    {
        InitializeComponent();

        // Load configuration
        _config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var baseUrl = _config["ApiSettings:BaseUrl"]?.TrimEnd('/');
        var interval = _config.GetValue<int>("ApiSettings:RefreshIntervalMs", 1000);
        var measurementsEndpoint = _config["ApiSettings:Endpoints:Measurements"];

        _timer = new System.Timers.Timer(interval);
        _timer.Elapsed += (s, e) =>
        {
            try
            {
                var url = $"{baseUrl}/{measurementsEndpoint}?type=HeartRate";
                var json = _client.DownloadString(url);

                Dispatcher.Invoke(() =>
                {
                    dataGrid.ItemsSource = JsonConvert.DeserializeObject<List<dynamic>>(json);
                });
            }
            catch { /* swallow */ }
        };

        _timer.Start();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Refreshing...");

        var baseUrl = _config["ApiSettings:BaseUrl"]?.TrimEnd('/');
        var healthEndpoint = _config["ApiSettings:Endpoints:Health"];
            
        var json = _client.DownloadString($"{baseUrl}/{healthEndpoint}");

        MessageBox.Show("OK: " + json);
    }
}