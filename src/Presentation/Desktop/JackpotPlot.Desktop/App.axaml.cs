using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JackpotPlot.Desktop.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using JackpotPlot.Desktop.UI.Services.Navigation;
using JackpotPlot.Desktop.UI.ViewModels;
using DashboardNavigationRequest = JackpotPlot.Desktop.UI.Services.Navigation.DashboardNavigationRequest;

namespace JackpotPlot.Desktop
{
    public partial class App : Application
    {
        private IServiceProvider? _services;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            _services = DesktopHostBuilder.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var navigationService = _services.GetRequiredService<INavigationService>();

                await navigationService.NavigateToAsync<DashboardViewModel, DashboardNavigationRequest>(new DashboardNavigationRequest());

                var mainWindow = _services.GetRequiredService<MainWindow>();
                mainWindow.DataContext = _services.GetRequiredService<MainWindowViewModel>();

                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}