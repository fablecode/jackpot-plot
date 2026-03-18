namespace JackpotPlot.Desktop.UI.Services.Navigation;

public interface INavigationMenuFactory
{
    IReadOnlyList<NavigationMenuItem> CreatePrimaryMenuItems();
}