using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JackpotPlot.Desktop.UI.ViewModels;

namespace JackpotPlot.Desktop.UI.Models.Menu;

public sealed partial class MenuItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isActive;

    public MenuItemViewModel(MenuItem menuItem)
    {
        MenuItem = menuItem;
        Children = new ObservableCollection<MenuItemViewModel>();
    }

    public MenuItem MenuItem { get; }

    public string Id => MenuItem.Id;

    public string Title => MenuItem.Title;

    public string? Icon => MenuItem.Icon;

    public string? NavigationKey => MenuItem.NavigationKey;

    public int Order => MenuItem.Order;

    public bool HasChildren => Children.Count > 0;

    public ObservableCollection<MenuItemViewModel> Children { get; }

    [RelayCommand]
    private void ToggleExpansion()
    {
        if (HasChildren)
        {
            IsExpanded = !IsExpanded;
        }
    }

    public void AddChild(MenuItemViewModel child)
    {
        Children.Add(child);
    }
}
