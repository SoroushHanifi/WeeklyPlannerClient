using Microsoft.EntityFrameworkCore;
using WeeklyPlannerAPI.Data;
using WeeklyPlannerAPI.Models.Entities;
using WeeklyPlannerAPI.Repositories.Interfaces;
using DayOfWeekEntity = WeeklyPlannerAPI.Models.Entities.DayOfWeek;

namespace WeeklyPlannerAPI.Repositories;

// ─── UserRepository ──────────────────────────────────────────────────────────
public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(int userId) =>
        db.Users.FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

    public Task<User?> GetByUsernameAsync(string username) =>
        db.Users.FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);

    public Task<User?> GetByEmailAsync(string email) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

    public async Task<User> CreateAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        db.Users.Update(user);
        await db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> SoftDeleteAsync(int userId)
    {
        var user = await GetByIdAsync(userId);
        if (user is null) return false;
        user.IsDeleted = true;
        user.IsActive  = false;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}

// ─── WeekRepository ──────────────────────────────────────────────────────────
public class WeekRepository(AppDbContext db) : IWeekRepository
{
    public Task<Week?> GetByIdAsync(int weekId, int userId) =>
        db.Weeks.Include(w => w.User)
            .FirstOrDefaultAsync(w => w.WeekId == weekId && w.UserId == userId && !w.IsDeleted);

    public Task<List<Week>> GetByUserAsync(int userId, bool includeTemplates = false) =>
        db.Weeks
            .Where(w => w.UserId == userId && !w.IsDeleted
                        && (includeTemplates || !w.IsTemplate))
            .OrderByDescending(w => w.StartDate)
            .ToListAsync();

    public Task<Week?> GetCurrentWeekAsync(int userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return db.Weeks.FirstOrDefaultAsync(w =>
            w.UserId == userId && !w.IsDeleted && !w.IsTemplate &&
            w.StartDate <= today && w.EndDate >= today);
    }

    public async Task<Week> CreateAsync(Week week)
    {
        db.Weeks.Add(week);
        await db.SaveChangesAsync();
        return week;
    }

    public async Task<Week> UpdateAsync(Week week)
    {
        week.UpdatedAt = DateTime.UtcNow;
        db.Weeks.Update(week);
        await db.SaveChangesAsync();
        return week;
    }

    public async Task<bool> SoftDeleteAsync(int weekId, int userId)
    {
        var week = await GetByIdAsync(weekId, userId);
        if (week is null) return false;
        week.IsDeleted = true;
        week.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CopyWeekAsync(int sourceWeekId, int targetWeekId,
        bool copyBlocks, bool copyTasks, bool copyGoals)
    {
        await db.Database.ExecuteSqlRawAsync(
            "EXEC planner.SP_CopyWeekAsTemplate @SourceWeekId={0}, @TargetWeekId={1}, @CopyBlocks={2}, @CopyTasks={3}, @CopyGoals={4}",
            sourceWeekId, targetWeekId, copyBlocks, copyTasks, copyGoals);
        return true;
    }
}

// ─── TimeBlockRepository ─────────────────────────────────────────────────────
public class TimeBlockRepository(AppDbContext db) : ITimeBlockRepository
{
    private IQueryable<TimeBlock> BaseQuery(int userId) =>
        db.TimeBlocks
            .Include(t => t.DayOfWeek)
            .Include(t => t.Category)
            .Include(t => t.Week)
            .Where(t => !t.IsDeleted && t.Week.UserId == userId);

    public Task<TimeBlock?> GetByIdAsync(int timeBlockId, int userId) =>
        BaseQuery(userId).FirstOrDefaultAsync(t => t.TimeBlockId == timeBlockId);

    public Task<List<TimeBlock>> GetByWeekAsync(int weekId, int userId) =>
        BaseQuery(userId)
            .Where(t => t.WeekId == weekId)
            .OrderBy(t => t.DayId).ThenBy(t => t.StartTime)
            .ToListAsync();

    public Task<List<TimeBlock>> GetByDayAsync(int weekId, int userId, byte dayId) =>
        BaseQuery(userId)
            .Where(t => t.WeekId == weekId && t.DayId == dayId)
            .OrderBy(t => t.StartTime)
            .ToListAsync();

    public async Task<TimeBlock> CreateAsync(TimeBlock timeBlock)
    {
        db.TimeBlocks.Add(timeBlock);
        await db.SaveChangesAsync();
        return timeBlock;
    }

    public async Task<TimeBlock> UpdateAsync(TimeBlock timeBlock)
    {
        timeBlock.UpdatedAt = DateTime.UtcNow;
        db.TimeBlocks.Update(timeBlock);
        await db.SaveChangesAsync();
        return timeBlock;
    }

    public async Task<bool> SoftDeleteAsync(int timeBlockId, int userId)
    {
        var tb = await GetByIdAsync(timeBlockId, userId);
        if (tb is null) return false;
        tb.IsDeleted = true;
        tb.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleCompleteAsync(int timeBlockId, int userId, bool isCompleted)
    {
        var tb = await GetByIdAsync(timeBlockId, userId);
        if (tb is null) return false;
        tb.IsCompleted = isCompleted;
        tb.CompletedAt = isCompleted ? DateTime.UtcNow : null;
        tb.UpdatedAt   = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}

// ─── WeekTaskRepository ──────────────────────────────────────────────────────
public class WeekTaskRepository(AppDbContext db) : IWeekTaskRepository
{
    private IQueryable<WeekTask> BaseQuery(int userId) =>
        db.WeekTasks
            .Include(t => t.Week)
            .Where(t => !t.IsDeleted && t.Week.UserId == userId);

    public Task<WeekTask?> GetByIdAsync(int taskId, int userId) =>
        BaseQuery(userId).FirstOrDefaultAsync(t => t.TaskId == taskId);

    public Task<List<WeekTask>> GetByWeekAsync(int weekId, int userId) =>
        BaseQuery(userId)
            .Where(t => t.WeekId == weekId)
            .OrderBy(t => t.OrderIndex).ThenBy(t => t.CreatedAt)
            .ToListAsync();

    public async Task<WeekTask> CreateAsync(WeekTask task)
    {
        db.WeekTasks.Add(task);
        await db.SaveChangesAsync();
        return task;
    }

    public async Task<WeekTask> UpdateAsync(WeekTask task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        db.WeekTasks.Update(task);
        await db.SaveChangesAsync();
        return task;
    }

    public async Task<bool> SoftDeleteAsync(int taskId, int userId)
    {
        var task = await GetByIdAsync(taskId, userId);
        if (task is null) return false;
        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleDoneAsync(int taskId, int userId, bool isDone)
    {
        var task = await GetByIdAsync(taskId, userId);
        if (task is null) return false;
        task.IsDone    = isDone;
        task.DoneAt    = isDone ? DateTime.UtcNow : null;
        task.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}

// ─── WeekGoalRepository ──────────────────────────────────────────────────────
public class WeekGoalRepository(AppDbContext db) : IWeekGoalRepository
{
    private IQueryable<WeekGoal> BaseQuery(int userId) =>
        db.WeekGoals
            .Include(g => g.Week)
            .Where(g => !g.IsDeleted && g.Week.UserId == userId);

    public Task<WeekGoal?> GetByIdAsync(int goalId, int userId) =>
        BaseQuery(userId).FirstOrDefaultAsync(g => g.GoalId == goalId);

    public Task<List<WeekGoal>> GetByWeekAsync(int weekId, int userId) =>
        BaseQuery(userId)
            .Where(g => g.WeekId == weekId)
            .OrderBy(g => g.OrderIndex)
            .ToListAsync();

    public async Task<WeekGoal> CreateAsync(WeekGoal goal)
    {
        db.WeekGoals.Add(goal);
        await db.SaveChangesAsync();
        return goal;
    }

    public async Task<WeekGoal> UpdateAsync(WeekGoal goal)
    {
        goal.UpdatedAt = DateTime.UtcNow;
        db.WeekGoals.Update(goal);
        await db.SaveChangesAsync();
        return goal;
    }

    public async Task<bool> SoftDeleteAsync(int goalId, int userId)
    {
        var goal = await GetByIdAsync(goalId, userId);
        if (goal is null) return false;
        goal.IsDeleted = true;
        goal.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleAchievedAsync(int goalId, int userId, bool isAchieved)
    {
        var goal = await GetByIdAsync(goalId, userId);
        if (goal is null) return false;
        goal.IsAchieved = isAchieved;
        goal.AchievedAt = isAchieved ? DateTime.UtcNow : null;
        goal.UpdatedAt  = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}

// ─── WeekNoteRepository ──────────────────────────────────────────────────────
public class WeekNoteRepository(AppDbContext db) : IWeekNoteRepository
{
    private IQueryable<WeekNote> BaseQuery(int userId) =>
        db.WeekNotes
            .Include(n => n.Category)
            .Include(n => n.Week)
            .Where(n => !n.IsDeleted && n.Week.UserId == userId);

    public Task<WeekNote?> GetByIdAsync(int noteId, int userId) =>
        BaseQuery(userId).FirstOrDefaultAsync(n => n.NoteId == noteId);

    public Task<List<WeekNote>> GetByWeekAsync(int weekId, int userId) =>
        BaseQuery(userId)
            .Where(n => n.WeekId == weekId)
            .OrderBy(n => n.OrderIndex)
            .ToListAsync();

    public async Task<WeekNote> CreateAsync(WeekNote note)
    {
        db.WeekNotes.Add(note);
        await db.SaveChangesAsync();
        return note;
    }

    public async Task<WeekNote> UpdateAsync(WeekNote note)
    {
        note.UpdatedAt = DateTime.UtcNow;
        db.WeekNotes.Update(note);
        await db.SaveChangesAsync();
        return note;
    }

    public async Task<bool> SoftDeleteAsync(int noteId, int userId)
    {
        var note = await GetByIdAsync(noteId, userId);
        if (note is null) return false;
        note.IsDeleted = true;
        note.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }
}

// ─── LookupRepository ────────────────────────────────────────────────────────
public class LookupRepository(AppDbContext db) : ILookupRepository
{
    public Task<List<Models.Entities.Category>> GetCategoriesAsync(string? type = null) =>
        db.Categories
            .Where(c => c.IsActive && (type == null || c.CategoryType == type || c.CategoryType == "BOTH"))
            .OrderBy(c => c.CategoryId)
            .ToListAsync();

    public Task<List<DayOfWeekEntity>> GetDaysOfWeekAsync() =>
        db.DaysOfWeek.OrderBy(d => d.DayOrder).ToListAsync();

    public Task<List<TimeSlot>> GetTimeSlotsAsync() =>
        db.TimeSlots.OrderBy(s => s.SlotId).ToListAsync();
}
