using JackpotPlot.Prediction.API.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace JackpotPlot.Prediction.API.Infrastructure.Databases;

public partial class PredictionDbContext : DbContext
{
    public PredictionDbContext()
    {
    }

    public PredictionDbContext(DbContextOptions<PredictionDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Lotteryhistory> Lotteryhistories { get; set; }

    public virtual DbSet<Models.Prediction> Predictions { get; set; }

    public virtual DbSet<Schemaversion> Schemaversions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder){}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lotteryhistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lotteryhistory_pkey");

            entity.ToTable("lotteryhistory");

            entity.HasIndex(e => new { e.Lotteryid, e.Drawdate }, "lotteryhistory_lotteryid_drawdate_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bonusnumbers).HasColumnName("bonusnumbers");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Drawdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("drawdate");
            entity.Property(e => e.Lotteryid).HasColumnName("lotteryid");
            entity.Property(e => e.Winningnumbers).HasColumnName("winningnumbers");
        });

        modelBuilder.Entity<Models.Prediction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("predictions_pkey");

            entity.ToTable("predictions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BonusNumbers).HasColumnName("bonus_numbers");
            entity.Property(e => e.ConfidenceScore)
                .HasPrecision(5, 2)
                .HasColumnName("confidence_score");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.LotteryId).HasColumnName("lottery_id");
            entity.Property(e => e.PredictedNumbers).HasColumnName("predicted_numbers");
            entity.Property(e => e.Strategy)
                .HasMaxLength(50)
                .HasColumnName("strategy");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Schemaversion>(entity =>
        {
            entity.HasKey(e => e.Schemaversionsid).HasName("PK_schemaversions_Id");

            entity.ToTable("schemaversions");

            entity.Property(e => e.Schemaversionsid).HasColumnName("schemaversionsid");
            entity.Property(e => e.Applied)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("applied");
            entity.Property(e => e.Scriptname)
                .HasMaxLength(255)
                .HasColumnName("scriptname");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
