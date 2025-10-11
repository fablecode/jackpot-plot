namespace LotteryDataCollector.Service.Infrastructure.Services;

public static class EurojackpotHelper
{
    public static IEnumerable<DateTime> GetEuroJackpotDrawDates()
    {
        var startDate = new DateTime(2012, 3, 23); // First Eurojackpot draw (Friday)
        var secondDrawStartDate = new DateTime(2022, 3, 29); // Tuesday draws started around this time
        var today = DateTime.Today;

        for (var date = startDate; date <= today; date = date.AddDays(1))
        {
            if (date.DayOfWeek == DayOfWeek.Friday || (date >= secondDrawStartDate && date.DayOfWeek == DayOfWeek.Tuesday))
            {
                yield return date;
            }
        }
    }
}