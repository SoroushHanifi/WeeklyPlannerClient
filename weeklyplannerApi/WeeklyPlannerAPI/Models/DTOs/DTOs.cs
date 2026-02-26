namespace WeeklyPlannerAPI.Models.DTOs;

// ─── Auth ─────────────────────────────────────────────────────────────────────
public record LoginRequest(string Username, string Password);

/// <summary>
/// Token دیگر در body برنمی‌گردد — از طریق HttpOnly Cookie مدیریت می‌شود.
/// </summary>
public record LoginResponse(string FullName, int UserId, string PreferredLang);

public record RegisterRequest(
    string Username,
    string FullName,
    string Email,
    string Password,
    string TimeZone = "Iran Standard Time",
    string PreferredLang = "fa");

// ─── User ─────────────────────────────────────────────────────────────────────
public record UserDto(int UserId, string Username, string? FullName, string? Email,
    string TimeZone, string PreferredLang, bool IsActive, DateTime CreatedAt);

public record UpdateUserRequest(string? FullName, string? Email,
    string? TimeZone, string? PreferredLang);

public record ChangePasswordRequest(string OldPassword, string NewPassword);

// ─── Week ─────────────────────────────────────────────────────────────────────
public record WeekDto(
    int WeekId, int UserId, string? FullName,
    DateOnly StartDate, DateOnly EndDate,
    string? StartDateShamsi, string? EndDateShamsi,
    short? YearShamsi, byte? WeekOfYear,
    string? Title, bool IsTemplate, DateTime CreatedAt);

public record CreateWeekRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    string? StartDateShamsi,
    string? EndDateShamsi,
    short? YearShamsi,
    byte? WeekOfYear,
    string? Title,
    bool IsTemplate = false);

public record UpdateWeekRequest(string? Title);

public record WeekSummaryDto(
    int WeekId, int UserId, string? FullName,
    string? StartDateShamsi, string? EndDateShamsi, string? WeekTitle,
    int TotalBlocks, int CompletedBlocks,
    int TotalPlannedMinutes, int CompletedMinutes,
    int TotalTasks, int DoneTasks,
    int TotalGoals, int AchievedGoals,
    decimal OverallProgressPct);

public record FullWeekResponse(
    WeekDto Week,
    List<TimeBlockDto> TimeBlocks,
    List<WeekTaskDto> Tasks,
    List<WeekGoalDto> Goals,
    List<WeekNoteDto> Notes);

public record CopyWeekRequest(
    int SourceWeekId,
    int TargetWeekId,
    bool CopyBlocks = true,
    bool CopyTasks = false,
    bool CopyGoals = false);

// ─── TimeBlock ────────────────────────────────────────────────────────────────
public record TimeBlockDto(
    int TimeBlockId, int WeekId,
    byte DayId, string DayNameFa,
    byte? SlotId,
    TimeOnly StartTime, TimeOnly EndTime, int? DurationMinutes,
    string ActivityTitle, string? Description,
    short? CategoryId, string? CategoryName, string? DisplayColor,
    byte Priority, bool IsCompleted, DateTime? CompletedAt, bool IsRecurring,
    DateTime CreatedAt);

public record CreateTimeBlockRequest(
    int WeekId,
    byte DayId,
    byte? SlotId,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string ActivityTitle,
    string? Description,
    short? CategoryId,
    string? CustomColorHex,
    byte Priority = 0,
    bool IsRecurring = false);

public record UpdateTimeBlockRequest(
    string? ActivityTitle,
    string? Description,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    short? CategoryId,
    string? CustomColorHex,
    byte? Priority,
    bool? IsRecurring);

public record CompleteTimeBlockRequest(bool IsCompleted);

// ─── WeekTask ─────────────────────────────────────────────────────────────────
public record WeekTaskDto(
    int TaskId, int WeekId,
    string TaskText, bool IsDone, DateTime? DoneAt,
    byte Priority, byte OrderIndex, DateOnly? DueDate,
    int? LinkedBlockId, DateTime CreatedAt);

public record CreateWeekTaskRequest(
    int WeekId,
    string TaskText,
    byte Priority = 0,
    byte OrderIndex = 0,
    DateOnly? DueDate = null,
    int? LinkedBlockId = null);

public record UpdateWeekTaskRequest(
    string? TaskText,
    byte? Priority,
    byte? OrderIndex,
    DateOnly? DueDate,
    int? LinkedBlockId);

public record ToggleTaskRequest(bool IsDone);

// ─── WeekGoal ─────────────────────────────────────────────────────────────────
public record WeekGoalDto(
    int GoalId, int WeekId,
    string GoalText, bool IsAchieved, DateTime? AchievedAt,
    byte OrderIndex, byte Weight, DateTime CreatedAt);

public record CreateWeekGoalRequest(
    int WeekId,
    string GoalText,
    byte OrderIndex = 0,
    byte Weight = 1);

public record UpdateWeekGoalRequest(
    string? GoalText,
    byte? OrderIndex,
    byte? Weight);

public record ToggleGoalRequest(bool IsAchieved);

// ─── WeekNote ─────────────────────────────────────────────────────────────────
public record WeekNoteDto(
    int NoteId, int WeekId,
    string NoteText,
    short? CategoryId, string? CategoryName,
    byte OrderIndex, DateTime CreatedAt);

public record CreateWeekNoteRequest(
    int WeekId,
    string NoteText,
    short? CategoryId = null,
    byte OrderIndex = 0);

public record UpdateWeekNoteRequest(
    string? NoteText,
    short? CategoryId,
    byte? OrderIndex);

// ─── Lookup ───────────────────────────────────────────────────────────────────
public record CategoryDto(
    short CategoryId, string CategoryName, string CategoryType,
    string? ColorHex, string? IconName, bool IsSystem, bool IsActive);

public record DayOfWeekDto(
    byte DayId, string DayNameFa, string DayNameEn,
    byte DayOrder, bool IsWeekend);

public record TimeSlotDto(
    byte SlotId, TimeOnly StartTime, TimeOnly EndTime, string LabelFa);

// ─── Shared ───────────────────────────────────────────────────────────────────
public record ApiResponse<T>(bool Success, string? Message, T? Data);
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
