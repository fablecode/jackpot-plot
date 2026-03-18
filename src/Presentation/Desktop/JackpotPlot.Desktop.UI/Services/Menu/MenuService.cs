using JackpotPlot.Desktop.UI.Configuration;
using JackpotPlot.Desktop.UI.Models.Menu;
using JackpotPlot.Desktop.UI.Services.Navigation;

namespace JackpotPlot.Desktop.UI.Services.Menu;

public sealed class MenuService : IMenuService
{
    private readonly INavigationService _navigationService;
    private readonly List<MenuItemViewModel> _menuItems;
    private readonly Dictionary<string, MenuItemViewModel> _menuItemsById;

    public MenuService(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _menuItems = new List<MenuItemViewModel>();
        _menuItemsById = new Dictionary<string, MenuItemViewModel>();

        InitializeMenuItems();

        _navigationService.Navigated += OnNavigated;
    }

    public event EventHandler<MenuStateChangedEventArgs>? MenuStateChanged;

    public IReadOnlyList<MenuItemViewModel> GetMenuItems() => _menuItems.AsReadOnly();

    public void ExpandMenuItem(string menuId)
    {
        if (_menuItemsById.TryGetValue(menuId, out var menuItem))
        {
            menuItem.IsExpanded = true;
            MenuStateChanged?.Invoke(this, new MenuStateChangedEventArgs(menuId, true));
        }
    }

    public void CollapseMenuItem(string menuId)
    {
        if (_menuItemsById.TryGetValue(menuId, out var menuItem))
        {
            menuItem.IsExpanded = false;
            MenuStateChanged?.Invoke(this, new MenuStateChangedEventArgs(menuId, false));
        }
    }

    public void SetActiveMenuItem(string? navigationKey)
    {
        foreach (var menuItem in _menuItems)
        {
            menuItem.IsActive = false;

            foreach (var child in menuItem.Children)
            {
                child.IsActive = child.NavigationKey?.Equals(navigationKey, StringComparison.OrdinalIgnoreCase) ?? false;

                if (child.IsActive)
                {
                    menuItem.IsExpanded = true;
                }
            }
        }
    }

    private void InitializeMenuItems()
    {
        var menuConfig = MenuConfiguration.GetMenuItems();

        foreach (var menuItem in menuConfig)
        {
            var menuItemViewModel = new MenuItemViewModel(menuItem);
            _menuItemsById[menuItem.Id] = menuItemViewModel;

            foreach (var child in menuItem.Children)
            {
                var childViewModel = new MenuItemViewModel(child);
                menuItemViewModel.AddChild(childViewModel);
                _menuItemsById[child.Id] = childViewModel;
            }

            _menuItems.Add(menuItemViewModel);
        }
    }

    private void OnNavigated(object? sender, NavigationChangedEventArgs e)
    {
        var navigationKey = (e.CurrentViewModel as IHasNavigationKey)?.NavigationKey;
        SetActiveMenuItem(navigationKey);
    }
}
