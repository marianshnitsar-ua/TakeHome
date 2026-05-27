using DesktopApp.ViewModels;
using System.Windows;

namespace DesktopApp.Views;

public partial class MainWindow : Window
{
    // Business logic is now in MainViewModel
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        
        // Set DataContext via Constructor Injection
        DataContext = viewModel;
    }
}
