using Avalonia.Controls;
using Avalonia.Controls.Templates;
using JackpotPlot.Desktop.UI.ViewModels;
using JackpotPlot.Desktop.UI.Views;

namespace JackpotPlot.Desktop.UI.Resources.Templates;

public sealed class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        return data switch
        {
            DashboardViewModel => new DashboardView(),
            _ => new TextBlock { Text = $"No view found for {data?.GetType().Name ?? "null"}" }
        };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}