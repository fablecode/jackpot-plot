namespace JackpotPlot.Domain.Scheduling.Strategies;

public class WeekdayDrawStrategy : IDrawScheduleStrategy
{
    public DateTime GetNextDraw(DateTime fromDate, LotteryScheduleConfig config)
    {
        var weekdays = config.Days ?? throw new ArgumentException("Missing weekday list");
        for (var i = 0; i < 7; i++)
        {
            var next = fromDate.Date.AddDays(i);
            if (weekdays.Contains(next.DayOfWeek))
                return next;
        }
        throw new InvalidOperationException("No next weekday match found");
    }

    public bool Handles(DrawScheduleType drawTypeSchedule) => drawTypeSchedule == DrawScheduleType.Weekday;
}