using System;
using System.Collections.Generic;

namespace JackpotPlot.Lottery.API.Infrastructure.Models;

public partial class Schemaversion
{
    public int Schemaversionsid { get; set; }

    public string Scriptname { get; set; } = null!;

    public DateTime Applied { get; set; }
}
