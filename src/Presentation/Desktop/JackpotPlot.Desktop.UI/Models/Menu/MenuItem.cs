using System.Collections.ObjectModel;

namespace JackpotPlot.Desktop.UI.Models.Menu;

public sealed class MenuItem
{
    public MenuItem(
        string id,
        string title,
        string? icon = null,
        string? navigationKey = null,
        int order = 0)
    {
        Id = id;
        Title = title;
        Icon = icon;
        NavigationKey = navigationKey;
        Order = order;
        Children = new ObservableCollection<MenuItem>();
    }

    public string Id { get; }

    public string Title { get; }

    public string? Icon { get; }

    public string? NavigationKey { get; }

    public int Order { get; }

    public ObservableCollection<MenuItem> Children { get; }

    public bool HasChildren => Children.Count > 0;

    public void AddChild(MenuItem child)
    {
        Children.Add(child);
    }
}
