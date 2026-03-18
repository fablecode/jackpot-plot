using JackpotPlot.Desktop.UI.ViewModels;

namespace JackpotPlot.Desktop.UI.Services.Navigation;

public sealed class NavigationChangedEventArgs : EventArgs
{
    public NavigationChangedEventArgs(
        ViewModelBase? previousViewModel,
        ViewModelBase? currentViewModel)
    {
        PreviousViewModel = previousViewModel;
        CurrentViewModel = currentViewModel;
    }

    public ViewModelBase? PreviousViewModel { get; }

    public ViewModelBase? CurrentViewModel { get; }
}