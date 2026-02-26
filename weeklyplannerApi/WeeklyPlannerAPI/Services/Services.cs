using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WeeklyPlannerAPI.Data;
using WeeklyPlannerAPI.Models.DTOs;
using WeeklyPlannerAPI.Models.Entities;
using WeeklyPlannerAPI.Repositories.Interfaces;
using WeeklyPlannerAPI.Services.Interfaces;

namespace WeeklyPlannerAPI.Services;

// ─── Mappers (قرار می‌گیرند در همین فایل) ─────────────────────────────────────
internal static class Mapper
{
    public static UserDto ToDto(User u) => new(
        u.UserId, u.Username, u.FullName, u.Email,
        u.TimeZone, u.PreferredLang, u.IsActive, u.CreatedAt);

    public static WeekDto ToDto(Week w) => new(
        w.WeekId, w.UserId, w.User?.FullName,
        w.StartDate, w.EndDate,
        w.StartDateShamsi, w.EndDateShamsi,
        w.YearShamsi, w.WeekOfYear,
        w.Title, w.IsTemplate, w.CreatedAt);

    public static TimeBlockDto ToDto(TimeBlock t) => new(
        t.TimeBlockId, t.WeekId,
        t.DayId, t.DayOfWeek?.DayNameFa ?? string.Empty,
        t.SlotId, t.StartTime, t.EndTime, t.DurationMinutes,
        t.ActivityTitle, t.Description,
        t.CategoryId, t.Category?.CategoryName,
        t.CustomColorHex ?? t.Category?.ColorHex,
        t.Priority, t.IsCompleted, t.CompletedAt, t.IsRecurring,
        t.CreatedAt);

    public static WeekTaskDto ToDto(WeekTask t) => new(
        t.TaskId, t.WeekId,
        t.TaskText, t.IsDone, t.DoneAt,
        t.Priority, t.OrderIndex, t.DueDate,
        t.LinkedBlockId, t.CreatedAt);

    public static WeekGoalDto ToDto(WeekGoal g) => new(
        g.GoalId, g.WeekId,
        g.GoalText, g.IsAchieved, g.AchievedAt,
        g.OrderIndex, g.Weight, g.CreatedAt);

    public static WeekNoteDto ToDto(WeekNote n) => new(
        n.NoteId, n.WeekId,
        n.NoteText,
        n.CategoryId, n.Category?.CategoryName,
        n.OrderIndex, n.CreatedAt);

    public static CategoryDto ToDto(Category c) => new(
        c.CategoryId, c.CategoryName, c.CategoryType,
        c.ColorHex, c.IconName, c.IsSystem, c.IsActive);

    public static DayOfWeekDto ToDto(Models.Entities.DayOfWeek d) => new(
        d.DayId, d.DayNameFa, d.DayNameEn, d.DayOrder, d.IsWeekend);

    public static TimeSlotDto ToDto(TimeSlot s) => new(
        s.SlotId, s.StartTime, s.EndTime, s.LabelFa);
}

// ─── AuthService ──────────────────────────────────────────────────────────────
public class AuthService(
    IUserRepository userRepo,
    IConfiguration config) : IAuthService
{
    /// <summary>
    /// برمی‌گردد (response, jwtToken) — کنترلر token را در HttpOnly Cookie قرار می‌دهد
    /// </summary>
    public async Task<(ApiResponse<LoginResponse> Response, string? Token)> LoginAsync(LoginRequest request)
    {
        var user = await userRepo.GetByUsernameAsync(request.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return (new(false, "نام کاربری یا رمز عبور اشتباه است", null), null);

        if (!user.IsActive)
            return (new(false, "حساب کاربری غیرفعال است", null), null);

        user.LastLoginAt = DateTime.UtcNow;
        await userRepo.UpdateAsync(user);

        var token = GenerateJwt(user);
        var response = new LoginResponse(user.FullName ?? user.Username, user.UserId, user.PreferredLang);
        return (new(true, "ورود موفق", response), token);
    }

    public async Task<ApiResponse<UserDto>> RegisterAsync(RegisterRequest request)
    {
        if (await userRepo.GetByUsernameAsync(request.Username) is not null)
            return new(false, "این نام کاربری قبلاً استفاده شده است", null);

        if (await userRepo.GetByEmailAsync(request.Email) is not null)
            return new(false, "این ایمیل قبلاً ثبت شده است", null);

        var user = new User
        {
            Username     = request.Username,
            FullName     = request.FullName,
            Email        = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            TimeZone     = request.TimeZone,
            PreferredLang = request.PreferredLang,
        };

        await userRepo.CreateAsync(user);
        return new(true, "ثبت‌نام با موفقیت انجام شد", Mapper.ToDto(user));
    }

    private string GenerateJwt(User user)
    {
        var jwtSettings = config.GetSection("JwtSettings");
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("fullName", user.FullName ?? user.Username),
            new Claim("lang", user.PreferredLang),
        };

        var token = new JwtSecurityToken(
            issuer:   jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims:   claims,
            expires:  DateTime.UtcNow.AddDays(double.Parse(jwtSettings["ExpireDays"] ?? "7")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// ─── UserService ──────────────────────────────────────────────────────────────
public class UserService(IUserRepository userRepo) : IUserService
{
    public async Task<ApiResponse<UserDto>> GetProfileAsync(int userId)
    {
        var user = await userRepo.GetByIdAsync(userId);
        return user is null
            ? new(false, "کاربر یافت نشد", null)
            : new(true, null, Mapper.ToDto(user));
    }

    public async Task<ApiResponse<UserDto>> UpdateProfileAsync(int userId, UpdateUserRequest request)
    {
        var user = await userRepo.GetByIdAsync(userId);
        if (user is null) return new(false, "کاربر یافت نشد", null);

        if (request.FullName   is not null) user.FullName     = request.FullName;
        if (request.Email      is not null) user.Email        = request.Email;
        if (request.TimeZone   is not null) user.TimeZone     = request.TimeZone;
        if (request.PreferredLang is not null) user.PreferredLang = request.PreferredLang;

        await userRepo.UpdateAsync(user);
        return new(true, "پروفایل بروزرسانی شد", Mapper.ToDto(user));
    }

    public async Task<ApiResponse<bool?>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await userRepo.GetByIdAsync(userId);
        if (user is null) return new(false, "کاربر یافت نشد", null);

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            return new(false, "رمز عبور فعلی اشتباه است", null);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await userRepo.UpdateAsync(user);
        return new(true, "رمز عبور تغییر کرد", true);
    }

    public async Task<ApiResponse<bool?>> DeleteAccountAsync(int userId)
    {
        var ok = await userRepo.SoftDeleteAsync(userId);
        return ok ? new(true, "حساب حذف شد", true) : new(false, "کاربر یافت نشد", null);
    }
}

// ─── WeekService ──────────────────────────────────────────────────────────────
public class WeekService(IWeekRepository weekRepo, AppDbContext db) : IWeekService
{
    // نگاشت DayOfWeek دات‌نت به DayId جدول (شنبه=1 ... جمعه=7)
    private static readonly Dictionary<System.DayOfWeek, byte> DotNetToPersianDayId = new()
    {
        { System.DayOfWeek.Saturday,  1 },
        { System.DayOfWeek.Sunday,    2 },
        { System.DayOfWeek.Monday,    3 },
        { System.DayOfWeek.Tuesday,   4 },
        { System.DayOfWeek.Wednesday, 5 },
        { System.DayOfWeek.Thursday,  6 },
        { System.DayOfWeek.Friday,    7 },
    };

    public async Task<ApiResponse<WeekDto>> GetWeekAsync(int weekId, int userId)
    {
        var week = await weekRepo.GetByIdAsync(weekId, userId);
        return week is null
            ? new(false, "هفته یافت نشد", null)
            : new(true, null, Mapper.ToDto(week));
    }

    public async Task<ApiResponse<FullWeekResponse>> GetFullWeekAsync(int weekId, int userId)
    {
        var week = await weekRepo.GetByIdAsync(weekId, userId);
        if (week is null) return new(false, "هفته یافت نشد", null);

        var timeBlocks = await db.TimeBlocks
            .Include(t => t.DayOfWeek).Include(t => t.Category)
            .Where(t => t.WeekId == weekId && !t.IsDeleted)
            .OrderBy(t => t.DayId).ThenBy(t => t.StartTime).ToListAsync();

        var tasks = await db.WeekTasks
            .Where(t => t.WeekId == weekId && !t.IsDeleted)
            .OrderBy(t => t.OrderIndex).ToListAsync();

        var goals = await db.WeekGoals
            .Where(g => g.WeekId == weekId && !g.IsDeleted)
            .OrderBy(g => g.OrderIndex).ToListAsync();

        var notes = await db.WeekNotes
            .Include(n => n.Category)
            .Where(n => n.WeekId == weekId && !n.IsDeleted)
            .OrderBy(n => n.OrderIndex).ToListAsync();

        return new(true, null, new(
            Mapper.ToDto(week),
            timeBlocks.Select(Mapper.ToDto).ToList(),
            tasks.Select(Mapper.ToDto).ToList(),
            goals.Select(Mapper.ToDto).ToList(),
            notes.Select(Mapper.ToDto).ToList()));
    }

    public async Task<ApiResponse<WeekDto?>> GetCurrentWeekAsync(int userId)
    {
        var week = await weekRepo.GetCurrentWeekAsync(userId);
        return new(true, null, week is null ? null : Mapper.ToDto(week));
    }

    public async Task<ApiResponse<List<WeekDto>>> GetUserWeeksAsync(int userId, bool includeTemplates = false)
    {
        var weeks = await weekRepo.GetByUserAsync(userId, includeTemplates);
        return new(true, null, weeks.Select(Mapper.ToDto).ToList());
    }

    public async Task<ApiResponse<List<WeekSummaryDto>>> GetWeekSummariesAsync(int userId)
    {
        var summaries = await db.Database
            .SqlQueryRaw<WeekSummaryDto>("""
                SELECT WeekId, UserId, FullName,
                       StartDateShamsi, EndDateShamsi, WeekTitle,
                       TotalBlocks, CompletedBlocks,
                       TotalPlannedMinutes, CompletedMinutes,
                       TotalTasks, DoneTasks,
                       TotalGoals, AchievedGoals,
                       OverallProgressPct
                FROM planner.VW_WeekSummary
                WHERE UserId = {0}
                ORDER BY WeekId DESC
                """, userId)
            .ToListAsync();

        return new(true, null, summaries);
    }

    /// <summary>
    /// هفته را می‌سازد و برای هر روز از 06:00 تا 23:00 (یک ساعته) بلوک‌های خالی ایجاد می‌کند.
    /// </summary>
    public async Task<ApiResponse<WeekDto>> CreateWeekAsync(int userId, CreateWeekRequest request)
    {
        // ── ۱. ساختن هفته ────────────────────────────────────────────────────
        var week = new Week
        {
            UserId = userId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            StartDateShamsi = request.StartDateShamsi,
            EndDateShamsi = request.EndDateShamsi,
            YearShamsi = request.YearShamsi,
            WeekOfYear = request.WeekOfYear,
            Title = request.Title,
            IsTemplate = request.IsTemplate,
        };
        await weekRepo.CreateAsync(week);

        // ── ۲. ساختن بلوک‌های زمانی خودکار ──────────────────────────────────
        // فقط اگر هفته template نباشد، بلوک‌های خالی می‌سازیم
        if (!request.IsTemplate)
        {
            var blocks = BuildEmptyTimeBlocks(week.WeekId, request.StartDate, request.EndDate);
            if (blocks.Count > 0)
            {
                await db.TimeBlocks.AddRangeAsync(blocks);
                await db.SaveChangesAsync();
            }
        }

        return new(true, "هفته ایجاد شد", Mapper.ToDto(week));
    }

    /// <summary>
    /// برای هر روز بین startDate و endDate (شامل هر دو طرف)،
    /// بلوک‌های یک‌ساعته از 06:00 تا 23:00 می‌سازد (۱۸ بلوک در روز).
    /// </summary>
    // فقط متد BuildEmptyTimeBlocks رو با این جایگزین کن:

    private static List<TimeBlock> BuildEmptyTimeBlocks(int weekId, DateOnly startDate, DateOnly endDate)
    {
        var blocks = new List<TimeBlock>();

        if (endDate < startDate) return blocks;

        var totalDays = Math.Min(endDate.DayNumber - startDate.DayNumber + 1, 7);

        for (int d = 0; d < totalDays; d++)
        {
            var date = startDate.AddDays(d);
            var dotNet = date.DayOfWeek;

            if (!DotNetToPersianDayId.TryGetValue(dotNet, out var dayId))
                continue;

            for (int h = 6; h < 24; h++)
            {
                var start = new TimeOnly(h, 0);
                var end = new TimeOnly(h == 23 ? 23 : h + 1, h == 23 ? 59 : 0);

                blocks.Add(new TimeBlock
                {
                    WeekId = weekId,
                    DayId = dayId,
                    StartTime = start,
                    EndTime = end,
                    ActivityTitle = $"{start:HH:mm} - {end:HH:mm}",  // ← NOT NULL رو پر می‌کنه
                    Description = null,
                    CategoryId = null,
                    Priority = 2,
                    IsRecurring = false,
                    IsCompleted = false,
                });
            }
        }

        return blocks;
    }

    public async Task<ApiResponse<WeekDto>> UpdateWeekAsync(int weekId, int userId, UpdateWeekRequest request)
    {
        var week = await weekRepo.GetByIdAsync(weekId, userId);
        if (week is null) return new(false, "هفته یافت نشد", null);
        if (request.Title is not null) week.Title = request.Title;
        await weekRepo.UpdateAsync(week);
        return new(true, "هفته بروزرسانی شد", Mapper.ToDto(week));
    }

    public async Task<ApiResponse<bool?>> DeleteWeekAsync(int weekId, int userId)
    {
        var ok = await weekRepo.SoftDeleteAsync(weekId, userId);
        return ok ? new(true, "هفته حذف شد", true) : new(false, "هفته یافت نشد", null);
    }

    public async Task<ApiResponse<bool?>> CopyWeekAsync(int userId, CopyWeekRequest request)
    {
        var source = await weekRepo.GetByIdAsync(request.SourceWeekId, userId);
        var target = await weekRepo.GetByIdAsync(request.TargetWeekId, userId);
        if (source is null || target is null)
            return new(false, "هفته مبدا یا مقصد یافت نشد", null);

        await weekRepo.CopyWeekAsync(request.SourceWeekId, request.TargetWeekId,
            request.CopyBlocks, request.CopyTasks, request.CopyGoals);
        return new(true, "هفته با موفقیت کپی شد", true);
    }
}


// ─── TimeBlockService ─────────────────────────────────────────────────────────
public class TimeBlockService(ITimeBlockRepository repo, IWeekRepository weekRepo) : ITimeBlockService
{
    public async Task<ApiResponse<TimeBlockDto>> GetByIdAsync(int timeBlockId, int userId)
    {
        var tb = await repo.GetByIdAsync(timeBlockId, userId);
        return tb is null ? new(false, "بلوک زمانی یافت نشد", null) : new(true, null, Mapper.ToDto(tb));
    }

    public async Task<ApiResponse<List<TimeBlockDto>>> GetByWeekAsync(int weekId, int userId)
    {
        var items = await repo.GetByWeekAsync(weekId, userId);
        return new(true, null, items.Select(Mapper.ToDto).ToList());
    }

    public async Task<ApiResponse<List<TimeBlockDto>>> GetByDayAsync(int weekId, int userId, byte dayId)
    {
        var items = await repo.GetByDayAsync(weekId, userId, dayId);
        return new(true, null, items.Select(Mapper.ToDto).ToList());
    }

    public async Task<ApiResponse<TimeBlockDto>> CreateAsync(int userId, CreateTimeBlockRequest request)
    {
        var week = await weekRepo.GetByIdAsync(request.WeekId, userId);
        if (week is null) return new(false, "هفته یافت نشد", null);

        var tb = new TimeBlock
        {
            WeekId        = request.WeekId,
            DayId         = request.DayId,
            SlotId        = request.SlotId,
            StartTime     = request.StartTime,
            EndTime       = request.EndTime,
            ActivityTitle = request.ActivityTitle,
            Description   = request.Description,
            CategoryId    = request.CategoryId,
            CustomColorHex = request.CustomColorHex,
            Priority      = request.Priority,
            IsRecurring   = request.IsRecurring,
        };
        await repo.CreateAsync(tb);
        return new(true, "بلوک زمانی ایجاد شد", Mapper.ToDto(tb));
    }

    public async Task<ApiResponse<TimeBlockDto>> UpdateAsync(int timeBlockId, int userId, UpdateTimeBlockRequest request)
    {
        var tb = await repo.GetByIdAsync(timeBlockId, userId);
        if (tb is null) return new(false, "بلوک زمانی یافت نشد", null);

        if (request.ActivityTitle is not null) tb.ActivityTitle  = request.ActivityTitle;
        if (request.Description   is not null) tb.Description    = request.Description;
        if (request.StartTime     is not null) tb.StartTime      = request.StartTime.Value;
        if (request.EndTime       is not null) tb.EndTime        = request.EndTime.Value;
        if (request.CategoryId    is not null) tb.CategoryId     = request.CategoryId;
        if (request.CustomColorHex is not null) tb.CustomColorHex = request.CustomColorHex;
        if (request.Priority      is not null) tb.Priority       = request.Priority.Value;
        if (request.IsRecurring   is not null) tb.IsRecurring    = request.IsRecurring.Value;

        await repo.UpdateAsync(tb);
        return new(true, "بلوک زمانی بروزرسانی شد", Mapper.ToDto(tb));
    }

    public async Task<ApiResponse<bool?>> DeleteAsync(int timeBlockId, int userId)
    {
        var ok = await repo.SoftDeleteAsync(timeBlockId, userId);
        return ok ? new(true, "بلوک زمانی حذف شد", true) : new(false, "بلوک زمانی یافت نشد", null);
    }

    public async Task<ApiResponse<bool?>> ToggleCompleteAsync(int timeBlockId, int userId, bool isCompleted)
    {
        var ok = await repo.ToggleCompleteAsync(timeBlockId, userId, isCompleted);
        return ok ? new(true, null, true) : new(false, "بلوک زمانی یافت نشد", null);
    }
}

// ─── WeekTaskService ──────────────────────────────────────────────────────────
public class WeekTaskService(IWeekTaskRepository repo, IWeekRepository weekRepo) : IWeekTaskService
{
    public async Task<ApiResponse<List<WeekTaskDto>>> GetByWeekAsync(int weekId, int userId)
    {
        var items = await repo.GetByWeekAsync(weekId, userId);
        return new(true, null, items.Select(Mapper.ToDto).ToList());
    }

    public async Task<ApiResponse<WeekTaskDto>> CreateAsync(int userId, CreateWeekTaskRequest request)
    {
        if (await weekRepo.GetByIdAsync(request.WeekId, userId) is null)
            return new(false, "هفته یافت نشد", null);

        var task = new WeekTask
        {
            WeekId       = request.WeekId,
            TaskText     = request.TaskText,
            Priority     = request.Priority,
            OrderIndex   = request.OrderIndex,
            DueDate      = request.DueDate,
            LinkedBlockId = request.LinkedBlockId,
        };
        await repo.CreateAsync(task);
        return new(true, "وظیفه ایجاد شد", Mapper.ToDto(task));
    }

    public async Task<ApiResponse<WeekTaskDto>> UpdateAsync(int taskId, int userId, UpdateWeekTaskRequest request)
    {
        var task = await repo.GetByIdAsync(taskId, userId);
        if (task is null) return new(false, "وظیفه یافت نشد", null);

        if (request.TaskText     is not null) task.TaskText     = request.TaskText;
        if (request.Priority     is not null) task.Priority     = request.Priority.Value;
        if (request.OrderIndex   is not null) task.OrderIndex   = request.OrderIndex.Value;
        if (request.DueDate      is not null) task.DueDate      = request.DueDate;
        if (request.LinkedBlockId is not null) task.LinkedBlockId = request.LinkedBlockId;

        await repo.UpdateAsync(task);
        return new(true, "وظیفه بروزرسانی شد", Mapper.ToDto(task));
    }

    public async Task<ApiResponse<bool?>> DeleteAsync(int taskId, int userId)
    {
        var ok = await repo.SoftDeleteAsync(taskId, userId);
        return ok ? new(true, "وظیفه حذف شد", true) : new(false, "وظیفه یافت نشد", null);
    }

    public async Task<ApiResponse<bool?>> ToggleDoneAsync(int taskId, int userId, bool isDone)
    {
        var ok = await repo.ToggleDoneAsync(taskId, userId, isDone);
        return ok ? new(true, null, true) : new(false, "وظیفه یافت نشد", null);
    }
}

// ─── WeekGoalService ──────────────────────────────────────────────────────────
public class WeekGoalService(IWeekGoalRepository repo, IWeekRepository weekRepo) : IWeekGoalService
{
    public async Task<ApiResponse<List<WeekGoalDto>>> GetByWeekAsync(int weekId, int userId)
    {
        var items = await repo.GetByWeekAsync(weekId, userId);
        return new(true, null, items.Select(Mapper.ToDto).ToList());
    }

    public async Task<ApiResponse<WeekGoalDto>> CreateAsync(int userId, CreateWeekGoalRequest request)
    {
        if (await weekRepo.GetByIdAsync(request.WeekId, userId) is null)
            return new(false, "هفته یافت نشد", null);

        var goal = new WeekGoal
        {
            WeekId     = request.WeekId,
            GoalText   = request.GoalText,
            OrderIndex = request.OrderIndex,
            Weight     = request.Weight,
        };
        await repo.CreateAsync(goal);
        return new(true, "هدف ایجاد شد", Mapper.ToDto(goal));
    }

    public async Task<ApiResponse<WeekGoalDto>> UpdateAsync(int goalId, int userId, UpdateWeekGoalRequest request)
    {
        var goal = await repo.GetByIdAsync(goalId, userId);
        if (goal is null) return new(false, "هدف یافت نشد", null);

        if (request.GoalText   is not null) goal.GoalText   = request.GoalText;
        if (request.OrderIndex is not null) goal.OrderIndex = request.OrderIndex.Value;
        if (request.Weight     is not null) goal.Weight     = request.Weight.Value;

        await repo.UpdateAsync(goal);
        return new(true, "هدف بروزرسانی شد", Mapper.ToDto(goal));
    }

    public async Task<ApiResponse<bool?>> DeleteAsync(int goalId, int userId)
    {
        var ok = await repo.SoftDeleteAsync(goalId, userId);
        return ok ? new(true, "هدف حذف شد", true) : new(false, "هدف یافت نشد", null);
    }

    public async Task<ApiResponse<bool?>> ToggleAchievedAsync(int goalId, int userId, bool isAchieved)
    {
        var ok = await repo.ToggleAchievedAsync(goalId, userId, isAchieved);
        return ok ? new(true, null, true) : new(false, "هدف یافت نشد", null);
    }
}

// ─── WeekNoteService ──────────────────────────────────────────────────────────
public class WeekNoteService(IWeekNoteRepository repo, IWeekRepository weekRepo) : IWeekNoteService
{
    public async Task<ApiResponse<List<WeekNoteDto>>> GetByWeekAsync(int weekId, int userId)
    {
        var items = await repo.GetByWeekAsync(weekId, userId);
        return new(true, null, items.Select(Mapper.ToDto).ToList());
    }

    public async Task<ApiResponse<WeekNoteDto>> CreateAsync(int userId, CreateWeekNoteRequest request)
    {
        if (await weekRepo.GetByIdAsync(request.WeekId, userId) is null)
            return new(false, "هفته یافت نشد", null);

        var note = new WeekNote
        {
            WeekId     = request.WeekId,
            NoteText   = request.NoteText,
            CategoryId = request.CategoryId,
            OrderIndex = request.OrderIndex,
        };
        await repo.CreateAsync(note);
        return new(true, "یادداشت ایجاد شد", Mapper.ToDto(note));
    }

    public async Task<ApiResponse<WeekNoteDto>> UpdateAsync(int noteId, int userId, UpdateWeekNoteRequest request)
    {
        var note = await repo.GetByIdAsync(noteId, userId);
        if (note is null) return new(false, "یادداشت یافت نشد", null);

        if (request.NoteText   is not null) note.NoteText   = request.NoteText;
        if (request.CategoryId is not null) note.CategoryId = request.CategoryId;
        if (request.OrderIndex is not null) note.OrderIndex = request.OrderIndex.Value;

        await repo.UpdateAsync(note);
        return new(true, "یادداشت بروزرسانی شد", Mapper.ToDto(note));
    }

    public async Task<ApiResponse<bool?>> DeleteAsync(int noteId, int userId)
    {
        var ok = await repo.SoftDeleteAsync(noteId, userId);
        return ok ? new(true, "یادداشت حذف شد", true) : new(false, "یادداشت یافت نشد", null);
    }
}

// ─── LookupService ────────────────────────────────────────────────────────────
public class LookupService(ILookupRepository repo) : ILookupService
{
    public async Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync(string? type = null)
    {
        var items = await repo.GetCategoriesAsync(type);
        return new(true, null, items.Select(Mapper.ToDto).ToList());
    }

    public async Task<ApiResponse<List<DayOfWeekDto>>> GetDaysOfWeekAsync()
    {
        var items = await repo.GetDaysOfWeekAsync();
        return new(true, null, items.Select(Mapper.ToDto).ToList());
    }

    public async Task<ApiResponse<List<TimeSlotDto>>> GetTimeSlotsAsync()
    {
        var items = await repo.GetTimeSlotsAsync();
        return new(true, null, items.Select(Mapper.ToDto).ToList());
    }
}
