using JackpotPlot.Desktop.UI.Models.Menu;

namespace JackpotPlot.Desktop.UI.Services.Menu;

public interface IMenuService
{
    IReadOnlyList<MenuItemViewModel> GetMenuItems();

    void ExpandMenuItem(string menuId);

    void CollapseMenuItem(string menuId);

    void SetActiveMenuItem(string? navigationKey);

    event EventHandler<MenuStateChangedEventArgs>? MenuStateChanged;
}
