using Avalonia.Controls;
using ID.WeatherDashboard.API.ViewModels;

namespace ID.WeatherDashboard.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public DashboardViewModel? ViewModel => DataContext as DashboardViewModel;
}
