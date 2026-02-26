using WeeklyPlannerAPI.Models.Entities;

namespace WeeklyPlannerAPI.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> SoftDeleteAsync(int userId);
}

public interface IWeekRepository
{
    Task<Week?> GetByIdAsync(int weekId, int userId);
    Task<List<Week>> GetByUserAsync(int userId, bool includeTemplates = false);
    Task<Week?> GetCurrentWeekAsync(int userId);
    Task<Week> CreateAsync(Week week);
    Task<Week> UpdateAsync(Week week);
    Task<bool> SoftDeleteAsync(int weekId, int userId);
    Task<bool> CopyWeekAsync(int sourceWeekId, int targetWeekId, bool copyBlocks, bool copyTasks, bool copyGoals);
}

public interface ITimeBlockRepository
{
    Task<TimeBlock?> GetByIdAsync(int timeBlockId, int userId);
    Task<List<TimeBlock>> GetByWeekAsync(int weekId, int userId);
    Task<List<TimeBlock>> GetByDayAsync(int weekId, int userId, byte dayId);
    Task<TimeBlock> CreateAsync(TimeBlock timeBlock);
    Task<TimeBlock> UpdateAsync(TimeBlock timeBlock);
    Task<bool> SoftDeleteAsync(int timeBlockId, int userId);
    Task<bool> ToggleCompleteAsync(int timeBlockId, int userId, bool isCompleted);
}

public interface IWeekTaskRepository
{
    Task<WeekTask?> GetByIdAsync(int taskId, int userId);
    Task<List<WeekTask>> GetByWeekAsync(int weekId, int userId);
    Task<WeekTask> CreateAsync(WeekTask task);
    Task<WeekTask> UpdateAsync(WeekTask task);
    Task<bool> SoftDeleteAsync(int taskId, int userId);
    Task<bool> ToggleDoneAsync(int taskId, int userId, bool isDone);
}

public interface IWeekGoalRepository
{
    Task<WeekGoal?> GetByIdAsync(int goalId, int userId);
    Task<List<WeekGoal>> GetByWeekAsync(int weekId, int userId);
    Task<WeekGoal> CreateAsync(WeekGoal goal);
    Task<WeekGoal> UpdateAsync(WeekGoal goal);
    Task<bool> SoftDeleteAsync(int goalId, int userId);
    Task<bool> ToggleAchievedAsync(int goalId, int userId, bool isAchieved);
}

public interface IWeekNoteRepository
{
    Task<WeekNote?> GetByIdAsync(int noteId, int userId);
    Task<List<WeekNote>> GetByWeekAsync(int weekId, int userId);
    Task<WeekNote> CreateAsync(WeekNote note);
    Task<WeekNote> UpdateAsync(WeekNote note);
    Task<bool> SoftDeleteAsync(int noteId, int userId);
}

public interface ILookupRepository
{
    Task<List<Models.Entities.Category>>  GetCategoriesAsync(string? type = null);
    Task<List<Models.Entities.DayOfWeek>> GetDaysOfWeekAsync();
    Task<List<TimeSlot>>                  GetTimeSlotsAsync();
}
