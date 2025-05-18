namespace JackpotPlot.Domain.Scheduling.Strategies;

public class DailyDrawStrategy : IDrawScheduleStrategy
{
    public DateTime GetNextDraw(DateTime fromDate, LotteryScheduleConfig config) => fromDate.Date.AddDays(1);

    public bool Handles(DrawScheduleType drawScheduleType) => drawScheduleType == DrawScheduleType.Daily;
}