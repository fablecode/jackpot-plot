using JackpotPlot.Lottery.API.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace JackpotPlot.Lottery.API.Infrastructure.Databases;

public partial class LotteryDbContext : DbContext
{
    public LotteryDbContext()
    {
    }

    public LotteryDbContext(DbContextOptions<LotteryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Continent> Continents { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Draw> Draws { get; set; }

    public virtual DbSet<DrawResult> DrawResults { get; set; }

    public virtual DbSet<LotteriesCountry> LotteriesCountries { get; set; }

    public virtual DbSet<Models.Lottery> Lotteries { get; set; }

    public virtual DbSet<LotteryNumber> LotteryNumbers { get; set; }

    public virtual DbSet<Schemaversion> Schemaversions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Continent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("continents_pkey");

            entity.ToTable("continents");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("countries_pkey");

            entity.ToTable("countries");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContinentId).HasColumnName("continent_id");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("country_code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Continent).WithMany(p => p.Countries)
                .HasForeignKey(d => d.ContinentId)
                .HasConstraintName("fk_continent");
        });

        modelBuilder.Entity<Draw>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("draws_pkey");

            entity.ToTable("draws");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DrawDate).HasColumnName("draw_date");
            entity.Property(e => e.DrawTime).HasColumnName("draw_time");
            entity.Property(e => e.JackpotAmount)
                .HasPrecision(15, 2)
                .HasColumnName("jackpot_amount");
            entity.Property(e => e.LotteryId).HasColumnName("lottery_id");
            entity.Property(e => e.RolloverCount)
                .HasDefaultValue(0)
                .HasColumnName("rollover_count");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Lottery).WithMany(p => p.Draws)
                .HasForeignKey(d => d.LotteryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("draws_lottery_id_fkey");
        });

        modelBuilder.Entity<DrawResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("draw_results_pkey");

            entity.ToTable("draw_results");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BonusNumbers).HasColumnName("bonus_numbers");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DrawId).HasColumnName("draw_id");
            entity.Property(e => e.Numbers).HasColumnName("numbers");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Draw).WithMany(p => p.DrawResults)
                .HasForeignKey(d => d.DrawId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("draw_results_draw_id_fkey");
        });

        modelBuilder.Entity<LotteriesCountry>(entity =>
        {
            entity.HasKey(e => new { e.LotteryId, e.CountryId }).HasName("lotteries_countries_pkey");

            entity.ToTable("lotteries_countries");

            entity.Property(e => e.LotteryId).HasColumnName("lottery_id");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Country).WithMany(p => p.LotteriesCountries)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("lotteries_countries_country_id_fkey");

            entity.HasOne(d => d.Lottery).WithMany(p => p.LotteriesCountries)
                .HasForeignKey(d => d.LotteryId)
                .HasConstraintName("lotteries_countries_lottery_id_fkey");
        });

        modelBuilder.Entity<Models.Lottery>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lotteries_pkey");

            entity.ToTable("lotteries");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DrawFrequency)
                .HasMaxLength(50)
                .HasColumnName("draw_frequency");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<LotteryNumber>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lottery_numbers_pkey");

            entity.ToTable("lottery_numbers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BonusNumbers).HasColumnName("bonus_numbers");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DrawId).HasColumnName("draw_id");
            entity.Property(e => e.Numbers).HasColumnName("numbers");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Draw).WithMany(p => p.LotteryNumbers)
                .HasForeignKey(d => d.DrawId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("lottery_numbers_draw_id_fkey");
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
