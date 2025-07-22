using Avalonia.Controls;
using ID.WeatherDashboard.API.ViewModels;
using Location = ID.WeatherDashboard.API.Data.Location;

namespace ID.WeatherDashboard.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    public DashboardViewModel? ViewModel => DataContext as DashboardViewModel;
}
