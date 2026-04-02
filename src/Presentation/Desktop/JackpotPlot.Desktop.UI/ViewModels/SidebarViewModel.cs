using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JackpotPlot.Desktop.UI.Models.Menu;
using JackpotPlot.Desktop.UI.Services.Menu;
using JackpotPlot.Desktop.UI.Services.Navigation;

namespace JackpotPlot.Desktop.UI.ViewModels;

public sealed partial class SidebarViewModel : ViewModelBase
{
    private readonly IMenuService _menuService;
    private readonly INavigationService _navigationService;
    private readonly Dictionary<string, bool> _menuStateBeforeCollapse = new();

    [ObservableProperty]
    private bool _isCollapsed;

    [ObservableProperty]
    private MenuItemViewModel? _selectedMenuItem;

    public SidebarViewModel(IMenuService menuService, INavigationService navigationService)
    {
        _menuService = menuService;
        _navigationService = navigationService;

        MenuItems = new ObservableCollection<MenuItemViewModel>(_menuService.GetMenuItems());

        _menuService.MenuStateChanged += OnMenuStateChanged;
    }

    public ObservableCollection<MenuItemViewModel> MenuItems { get; }

    partial void OnIsCollapsedChanged(bool value)
    {
        if (value)
        {
            // When sidebar is collapsed, save current expansion state and collapse all menu items
            _menuStateBeforeCollapse.Clear();

            foreach (var menuItem in MenuItems)
            {
                _menuStateBeforeCollapse[menuItem.Id] = menuItem.IsExpanded;

                if (menuItem.IsExpanded)
                {
                    _menuService.CollapseMenuItem(menuItem.Id);
                }
            }
        }
        else
        {
            // When sidebar is expanded, restore previous expansion state
            // or ensure parent of active child is expanded
            foreach (var menuItem in MenuItems)
            {
                // Check if this menu item has an active child
                var hasActiveChild = menuItem.Children.Any(child => child.IsActive);

                // Expand if it was previously expanded OR if it has an active child
                var shouldExpand = hasActiveChild || 
                                   (_menuStateBeforeCollapse.TryGetValue(menuItem.Id, out var wasExpanded) && wasExpanded);

                if (shouldExpand && !menuItem.IsExpanded)
                {
                    _menuService.ExpandMenuItem(menuItem.Id);
                }
            }

            _menuStateBeforeCollapse.Clear();
        }
    }

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsCollapsed = !IsCollapsed;
    }

    [RelayCommand]
    private async Task NavigateAsync(MenuItemViewModel? menuItem)
    {
        if (menuItem is null || string.IsNullOrEmpty(menuItem.NavigationKey))
        {
            return;
        }

        SelectedMenuItem = menuItem;

        var navigationKey = menuItem.NavigationKey;

        if (navigationKey == NavigationKeys.Dashboard)
        {
            await _navigationService.NavigateToAsync<DashboardViewModel, DashboardNavigationRequest>(
                new DashboardNavigationRequest());
        }
        else if (navigationKey == NavigationKeys.DrawHistory)
        {
            await _navigationService.NavigateToAsync<DrawHistoryViewModel, DrawHistoryNavigationRequest>(
                new DrawHistoryNavigationRequest());
        }
        else if (navigationKey == NavigationKeys.NumberGenerator)
        {
            await _navigationService.NavigateToAsync<NumberGeneratorViewModel, NumberGeneratorNavigationRequest>(
                new NumberGeneratorNavigationRequest());
        }
    }

    [RelayCommand]
    private void ToggleMenuItem(MenuItemViewModel? menuItem)
    {
        if (menuItem is null || !menuItem.HasChildren)
        {
            return;
        }

        // Don't expand/collapse menu items when sidebar is collapsed
        if (IsCollapsed)
        {
            return;
        }

        if (menuItem.IsExpanded)
        {
            _menuService.CollapseMenuItem(menuItem.Id);
        }
        else
        {
            _menuService.ExpandMenuItem(menuItem.Id);
        }
    }

    private void OnMenuStateChanged(object? sender, MenuStateChangedEventArgs e)
    {
    }
}
