namespace JackpotPlot.Domain.Scheduling.Strategies;

public class IntervalDrawStrategy : IDrawScheduleStrategy
{
    public DateTime GetNextDraw(DateTime fromDate, LotteryScheduleConfig config)
    {
        var start = config.StartDate?.Date ?? fromDate.Date;
        var interval = config.IntervalDays ?? 7;
        var daysSinceStart = (fromDate.Date - start).Days;
        var daysToNext = interval - (daysSinceStart % interval);
        return fromDate.Date.AddDays(daysToNext);
    }

    public bool Handles(DrawScheduleType drawTypeSchedule) => drawTypeSchedule == DrawScheduleType.Interval;
}