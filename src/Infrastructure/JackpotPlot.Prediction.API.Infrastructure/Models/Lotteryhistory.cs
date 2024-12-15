using System;
using System.Collections.Generic;

namespace JackpotPlot.Prediction.API.Infrastructure.Models;

public partial class Lotteryhistory
{
    public int Id { get; set; }

    public int Lotteryid { get; set; }

    public DateTime Drawdate { get; set; }

    public List<int> Winningnumbers { get; set; } = null!;

    public List<int>? Bonusnumbers { get; set; }

    public DateTime? Createdat { get; set; }
}
