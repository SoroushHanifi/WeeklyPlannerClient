using Microsoft.EntityFrameworkCore;
using WeeklyPlannerAPI.Models.Entities;
using DayOfWeekEntity = WeeklyPlannerAPI.Models.Entities.DayOfWeek;

namespace WeeklyPlannerAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // planner schema
    public DbSet<User>      Users      => Set<User>();
    public DbSet<Week>      Weeks      => Set<Week>();
    public DbSet<TimeBlock> TimeBlocks => Set<TimeBlock>();
    public DbSet<WeekTask>  WeekTasks  => Set<WeekTask>();
    public DbSet<WeekGoal>  WeekGoals  => Set<WeekGoal>();
    public DbSet<WeekNote>  WeekNotes  => Set<WeekNote>();

    // lookup schema
    public DbSet<Category>        Categories  => Set<Category>();
    public DbSet<DayOfWeekEntity> DaysOfWeek  => Set<DayOfWeekEntity>();
    public DbSet<TimeSlot>        TimeSlots   => Set<TimeSlot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── TimeBlock: DurationMinutes is computed ──────────────────────────
        modelBuilder.Entity<TimeBlock>()
            .Property(t => t.DurationMinutes)
            .HasComputedColumnSql("datediff(minute,[StartTime],[EndTime])", stored: true);

        modelBuilder.Entity<TimeBlock>()
            .ToTable(tb => tb.HasTrigger("TR_TimeBlocks_UpdatedAt")); 

        // ── Unique constraints ───────────────────────────────────────────────
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Week>()
            .HasIndex(w => new { w.UserId, w.StartDate }).IsUnique();
        modelBuilder.Entity<Category>()
            .HasIndex(c => new { c.CategoryName, c.CategoryType }).IsUnique();

        // ── Check constraints ────────────────────────────────────────────────
        modelBuilder.Entity<User>()
            .HasCheckConstraint("CK_Users_Lang", "[PreferredLang] IN ('fa','en')");
        modelBuilder.Entity<TimeBlock>()
            .HasCheckConstraint("CK_TB_Priority", "[Priority] BETWEEN 0 AND 3");
        modelBuilder.Entity<TimeBlock>()
            .HasCheckConstraint("CK_TB_Times", "[EndTime] > [StartTime]");
        modelBuilder.Entity<WeekGoal>()
            .HasCheckConstraint("CK_WG_Weight", "[Weight] BETWEEN 1 AND 5");
        modelBuilder.Entity<Week>()
            .HasCheckConstraint("CK_Weeks_Dates", "[EndDate] > [StartDate]");

        // ── Cascade deletes (matching FK definitions) ────────────────────────
        modelBuilder.Entity<TimeBlock>()
            .HasOne(t => t.Week)
            .WithMany(w => w.TimeBlocks)
            .HasForeignKey(t => t.WeekId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WeekTask>()
            .HasOne(t => t.Week)
            .WithMany(w => w.WeekTasks)
            .HasForeignKey(t => t.WeekId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WeekGoal>()
            .HasOne(g => g.Week)
            .WithMany(w => w.WeekGoals)
            .HasForeignKey(g => g.WeekId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WeekNote>()
            .HasOne(n => n.Week)
            .WithMany(w => w.WeekNotes)
            .HasForeignKey(n => n.WeekId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── WeekTask → TimeBlock (no cascade to avoid multiple paths) ────────
        modelBuilder.Entity<WeekTask>()
            .HasOne(t => t.LinkedBlock)
            .WithMany()
            .HasForeignKey(t => t.LinkedBlockId)
            .OnDelete(DeleteBehavior.SetNull);

        // ── Default values ───────────────────────────────────────────────────
        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("sysutcdatetime()");
        modelBuilder.Entity<Week>()
            .Property(w => w.CreatedAt)
            .HasDefaultValueSql("sysutcdatetime()");
        modelBuilder.Entity<TimeBlock>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql("sysutcdatetime()");
        modelBuilder.Entity<WeekTask>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql("sysutcdatetime()");
        modelBuilder.Entity<WeekGoal>()
            .Property(g => g.CreatedAt)
            .HasDefaultValueSql("sysutcdatetime()");
        modelBuilder.Entity<WeekNote>()
            .Property(n => n.CreatedAt)
            .HasDefaultValueSql("sysutcdatetime()");
    }
}
