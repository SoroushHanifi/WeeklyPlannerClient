using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeeklyPlannerAPI.Models.Entities;

// ─── planner.Users ───────────────────────────────────────────────────────────
[Table("Users", Schema = "planner")]
public class User
{
    [Key] public int UserId { get; set; }
    [Required, MaxLength(100)] public string Username { get; set; } = null!;
    [MaxLength(150)] public string? FullName { get; set; }
    [MaxLength(255)] public string? Email { get; set; }
    [MaxLength(512)] public string? PasswordHash { get; set; }
    [MaxLength(60)]  public string TimeZone { get; set; } = "Iran Standard Time";
    [MaxLength(2)]   public string PreferredLang { get; set; } = "fa";
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public ICollection<Week> Weeks { get; set; } = new List<Week>();
}

// ─── planner.Weeks ───────────────────────────────────────────────────────────
[Table("Weeks", Schema = "planner")]
public class Week
{
    [Key] public int WeekId { get; set; }
    public int UserId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    [MaxLength(10)] public string? StartDateShamsi { get; set; }
    [MaxLength(10)] public string? EndDateShamsi { get; set; }
    public short? YearShamsi { get; set; }
    public byte? WeekOfYear { get; set; }
    [MaxLength(200)] public string? Title { get; set; }
    public bool IsTemplate { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(UserId))] public User User { get; set; } = null!;
    public ICollection<TimeBlock> TimeBlocks { get; set; } = new List<TimeBlock>();
    public ICollection<WeekTask> WeekTasks { get; set; } = new List<WeekTask>();
    public ICollection<WeekGoal> WeekGoals { get; set; } = new List<WeekGoal>();
    public ICollection<WeekNote> WeekNotes { get; set; } = new List<WeekNote>();
}

// ─── planner.TimeBlocks ──────────────────────────────────────────────────────
[Table("TimeBlocks", Schema = "planner")]
public class TimeBlock
{
    [Key] public int TimeBlockId { get; set; }
    public int WeekId { get; set; }
    public byte DayId { get; set; }
    public byte? SlotId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public int? DurationMinutes { get; set; }
    [Required, MaxLength(300)] public string ActivityTitle { get; set; } = null!;
    public string? Description { get; set; }
    public short? CategoryId { get; set; }
    [MaxLength(7)] public string? CustomColorHex { get; set; }
    public byte Priority { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public bool IsRecurring { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(WeekId))]     public Week Week { get; set; } = null!;
    [ForeignKey(nameof(DayId))]      public DayOfWeek DayOfWeek { get; set; } = null!;
    [ForeignKey(nameof(SlotId))]     public TimeSlot? TimeSlot { get; set; }
    [ForeignKey(nameof(CategoryId))] public Category? Category { get; set; }
}

// ─── planner.WeekTasks ───────────────────────────────────────────────────────
[Table("WeekTasks", Schema = "planner")]
public class WeekTask
{
    [Key] public int TaskId { get; set; }
    public int WeekId { get; set; }
    [Required, MaxLength(500)] public string TaskText { get; set; } = null!;
    public bool IsDone { get; set; } = false;
    public DateTime? DoneAt { get; set; }
    public byte Priority { get; set; } = 0;
    public byte OrderIndex { get; set; } = 0;
    public DateOnly? DueDate { get; set; }
    public int? LinkedBlockId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(WeekId))]       public Week Week { get; set; } = null!;
    [ForeignKey(nameof(LinkedBlockId))] public TimeBlock? LinkedBlock { get; set; }
}

// ─── planner.WeekGoals ───────────────────────────────────────────────────────
[Table("WeekGoals", Schema = "planner")]
public class WeekGoal
{
    [Key] public int GoalId { get; set; }
    public int WeekId { get; set; }
    [Required, MaxLength(500)] public string GoalText { get; set; } = null!;
    public bool IsAchieved { get; set; } = false;
    public DateTime? AchievedAt { get; set; }
    public byte OrderIndex { get; set; } = 0;
    public byte Weight { get; set; } = 1;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(WeekId))] public Week Week { get; set; } = null!;
}

// ─── planner.WeekNotes ───────────────────────────────────────────────────────
[Table("WeekNotes", Schema = "planner")]
public class WeekNote
{
    [Key] public int NoteId { get; set; }
    public int WeekId { get; set; }
    [Required] public string NoteText { get; set; } = null!;
    public short? CategoryId { get; set; }
    public byte OrderIndex { get; set; } = 0;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(WeekId))]      public Week Week { get; set; } = null!;
    [ForeignKey(nameof(CategoryId))]  public Category? Category { get; set; }
}

// ─── lookup.Categories ───────────────────────────────────────────────────────
[Table("Categories", Schema = "lookup")]
public class Category
{
    [Key] public short CategoryId { get; set; }
    [Required, MaxLength(100)] public string CategoryName { get; set; } = null!;
    [Required, MaxLength(30)]  public string CategoryType { get; set; } = null!;
    [MaxLength(7)]  public string? ColorHex { get; set; }
    [MaxLength(50)] public string? IconName { get; set; }
    public bool IsSystem { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

// ─── lookup.DaysOfWeek ───────────────────────────────────────────────────────
[Table("DaysOfWeek", Schema = "lookup")]
public class DayOfWeek
{
    [Key] public byte DayId { get; set; }
    [Required, MaxLength(20)] public string DayNameFa { get; set; } = null!;
    [Required, MaxLength(20)] public string DayNameEn { get; set; } = null!;
    public byte DayOrder { get; set; }
    public bool IsWeekend { get; set; } = false;
}

// ─── lookup.TimeSlots ────────────────────────────────────────────────────────
[Table("TimeSlots", Schema = "lookup")]
public class TimeSlot
{
    [Key] public byte SlotId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    [Required, MaxLength(30)] public string LabelFa { get; set; } = null!;
}
