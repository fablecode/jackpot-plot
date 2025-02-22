﻿namespace JackpotPlot.Lottery.API.Infrastructure.Models;

public partial class DrawResult
{
    public int Id { get; set; }

    public int? DrawId { get; set; }

    public List<int> Numbers { get; set; } = null!;

    public List<int>? BonusNumbers { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Draw? Draw { get; set; }
}
