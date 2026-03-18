namespace JackpotPlot.Desktop.UI.Services.Navigation;

public interface INavigationAware
{
    Task OnNavigatedToAsync(CancellationToken cancellationToken = default);

    Task OnNavigatedFromAsync(CancellationToken cancellationToken = default);
}

public interface INavigationAware<in TRequest>
{
    Task OnNavigatedToAsync(TRequest request, CancellationToken cancellationToken = default);
}