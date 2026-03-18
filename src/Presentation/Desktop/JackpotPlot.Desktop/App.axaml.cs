using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JackpotPlot.Desktop.Hosting;
using JackpotPlot.Desktop.UI.Services.Navigation;
using JackpotPlot.Desktop.UI.Services.Theme;
using JackpotPlot.Desktop.UI.ViewModels;
using JackpotPlot.Desktop.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
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

        public override void OnFrameworkInitializationCompleted()
        {
            try
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    _services = DesktopHostBuilder.BuildServiceProvider();

                    var mainWindow = _services.GetRequiredService<MainWindow>();
                    mainWindow.DataContext = _services.GetRequiredService<MainWindowViewModel>();

                    desktop.MainWindow = mainWindow;
                }

                base.OnFrameworkInitializationCompleted();

                if (_services != null)
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        try
                        {
                            var themeService = _services.GetRequiredService<IThemeService>();
                            await themeService.LoadSavedThemeAsync();

                            var navigationService = _services.GetRequiredService<INavigationService>();
                            await navigationService.NavigateToAsync<DashboardViewModel, DashboardNavigationRequest>(new DashboardNavigationRequest());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Async initialization failed: {ex}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application initialization failed: {ex}");
                throw;
            }
        }
    }
}