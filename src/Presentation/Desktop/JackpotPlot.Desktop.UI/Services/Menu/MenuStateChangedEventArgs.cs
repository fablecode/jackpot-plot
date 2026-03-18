namespace JackpotPlot.Desktop.UI.Services.Menu;

public sealed class MenuStateChangedEventArgs : EventArgs
{
    public MenuStateChangedEventArgs(string menuId, bool isExpanded)
    {
        MenuId = menuId;
        IsExpanded = isExpanded;
    }

    public string MenuId { get; }

    public bool IsExpanded { get; }
}
