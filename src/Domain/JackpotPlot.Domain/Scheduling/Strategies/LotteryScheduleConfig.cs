namespace JackpotPlot.Domain.Scheduling.Strategies;

public class LotteryScheduleConfig
{
    public DrawScheduleType DrawType { get; set; } = DrawScheduleType.Daily; // "weekday", "interval", "daily"
    public List<DayOfWeek>? Days { get; set; }      // for "weekday"
    public int? IntervalDays { get; set; }          // for "interval"
    public DateTime? StartDate { get; set; }        // for "interval"
}