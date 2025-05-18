namespace JackpotPlot.Domain.Scheduling.Strategies;

public interface IDrawScheduleStrategy
{
    DateTime GetNextDraw(DateTime fromDate, LotteryScheduleConfig config);

    bool Handles(DrawScheduleType drawScheduleType);
}