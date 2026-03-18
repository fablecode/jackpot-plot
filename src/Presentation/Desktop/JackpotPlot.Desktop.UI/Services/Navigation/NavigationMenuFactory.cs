namespace JackpotPlot.Desktop.UI.Services.Navigation;

public sealed class NavigationMenuFactory : INavigationMenuFactory
{
    private readonly IEnumerable<INavigationTarget> _navigationTargets;

    public NavigationMenuFactory(IEnumerable<INavigationTarget> navigationTargets)
    {
        _navigationTargets = navigationTargets;
    }

    public IReadOnlyList<NavigationMenuItem> CreatePrimaryMenuItems()
    {
        return _navigationTargets
            .Where(x => x.ShowInPrimaryMenu)
            .OrderBy(x => x.Order)
            .Select(x => new NavigationMenuItem
            {
                Key = x.Key,
                Title = x.Title,
                IconKey = x.IconKey,
                Target = x
            })
            .ToList();
    }
}