using System;
using System.Collections.Generic;
using JackpotPlot.Prediction.API.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace JackpotPlot.Prediction.API.Infrastructure.Databases;

#nullable disable

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }
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

            entity.HasIndex(e => new { e.Lotteryid, e.Userid, e.Generatedat }, "predictions_lotteryid_userid_generatedat_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Confidencescore)
                .HasPrecision(5, 2)
                .HasColumnName("confidencescore");
            entity.Property(e => e.Generatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("generatedat");
            entity.Property(e => e.Lotteryid).HasColumnName("lotteryid");
            entity.Property(e => e.Predictionnumbers).HasColumnName("predictionnumbers");
            entity.Property(e => e.Userid).HasColumnName("userid");
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
