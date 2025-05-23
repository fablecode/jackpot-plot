﻿namespace JackpotPlot.Domain.Domain;

public sealed class LotteryConfigurationDomain
{
    public int Id { get; set; }

    public int LotteryId { get; set; }

    public string? DrawType { get; set; }

    public int MainNumbersCount { get; set; }

    public int MainNumbersRange { get; set; }

    public int BonusNumbersCount { get; set; }

    public int BonusNumbersRange { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? DrawFrequency { get; set; }
    public int? IntervalDays { get; set; }
    public List<string>? DrawDays { get; set; }
}
