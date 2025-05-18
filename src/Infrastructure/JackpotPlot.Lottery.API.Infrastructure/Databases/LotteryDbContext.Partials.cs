using JackpotPlot.Domain.Enums;
using JackpotPlot.Lottery.API.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace JackpotPlot.Lottery.API.Infrastructure.Databases;

public partial class LotteryDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // 🧾 PostgreSQL Enum Mappings
        modelBuilder.HasPostgresEnum<TicketStatus>("ticket_status");
        modelBuilder.HasPostgresEnum<ConfidenceLevel>("confidence_level");

        // ✅ View: ticket_overview (read-only)
        modelBuilder.Entity<TicketOverview>(entity =>
        {
            entity.HasNoKey();                   // read-only view
            entity.ToView("ticket_overview");    // exact SQL view name

            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.TicketName).HasColumnName("ticket_name");
            entity.Property(e => e.LotteryName).HasColumnName("lottery_name");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Entries).HasColumnName("entries");
            entity.Property(e => e.Confidence).HasColumnName("confidence");
        });
    }
}

