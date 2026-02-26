using WeeklyPlannerAPI.Models.DTOs;

namespace WeeklyPlannerAPI.Services.Interfaces;

public interface IAuthService
{
    /// <summary>Token جداگانه برمی‌گردد تا کنترلر آن را در HttpOnly Cookie قرار دهد</summary>
    Task<(ApiResponse<LoginResponse> Response, string? Token)> LoginAsync(LoginRequest request);
    Task<ApiResponse<UserDto>> RegisterAsync(RegisterRequest request);
}

public interface IUserService
{
    Task<ApiResponse<UserDto>> GetProfileAsync(int userId);
    Task<ApiResponse<UserDto>> UpdateProfileAsync(int userId, UpdateUserRequest request);
    Task<ApiResponse<bool?>> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<ApiResponse<bool?>> DeleteAccountAsync(int userId);
}

public interface IWeekService
{
    Task<ApiResponse<WeekDto>> GetWeekAsync(int weekId, int userId);
    Task<ApiResponse<FullWeekResponse>> GetFullWeekAsync(int weekId, int userId);
    Task<ApiResponse<WeekDto?>> GetCurrentWeekAsync(int userId);
    Task<ApiResponse<List<WeekDto>>> GetUserWeeksAsync(int userId, bool includeTemplates = false);
    Task<ApiResponse<List<WeekSummaryDto>>> GetWeekSummariesAsync(int userId);
    Task<ApiResponse<WeekDto>> CreateWeekAsync(int userId, CreateWeekRequest request);
    Task<ApiResponse<WeekDto>> UpdateWeekAsync(int weekId, int userId, UpdateWeekRequest request);
    Task<ApiResponse<bool?>> DeleteWeekAsync(int weekId, int userId);
    Task<ApiResponse<bool?>> CopyWeekAsync(int userId, CopyWeekRequest request);
}

public interface ITimeBlockService
{
    Task<ApiResponse<TimeBlockDto>> GetByIdAsync(int timeBlockId, int userId);
    Task<ApiResponse<List<TimeBlockDto>>> GetByWeekAsync(int weekId, int userId);
    Task<ApiResponse<List<TimeBlockDto>>> GetByDayAsync(int weekId, int userId, byte dayId);
    Task<ApiResponse<TimeBlockDto>> CreateAsync(int userId, CreateTimeBlockRequest request);
    Task<ApiResponse<TimeBlockDto>> UpdateAsync(int timeBlockId, int userId, UpdateTimeBlockRequest request);
    Task<ApiResponse<bool?>> DeleteAsync(int timeBlockId, int userId);
    Task<ApiResponse<bool?>> ToggleCompleteAsync(int timeBlockId, int userId, bool isCompleted);
}

public interface IWeekTaskService
{
    Task<ApiResponse<List<WeekTaskDto>>> GetByWeekAsync(int weekId, int userId);
    Task<ApiResponse<WeekTaskDto>> CreateAsync(int userId, CreateWeekTaskRequest request);
    Task<ApiResponse<WeekTaskDto>> UpdateAsync(int taskId, int userId, UpdateWeekTaskRequest request);
    Task<ApiResponse<bool?>> DeleteAsync(int taskId, int userId);
    Task<ApiResponse<bool?>> ToggleDoneAsync(int taskId, int userId, bool isDone);
}

public interface IWeekGoalService
{
    Task<ApiResponse<List<WeekGoalDto>>> GetByWeekAsync(int weekId, int userId);
    Task<ApiResponse<WeekGoalDto>> CreateAsync(int userId, CreateWeekGoalRequest request);
    Task<ApiResponse<WeekGoalDto>> UpdateAsync(int goalId, int userId, UpdateWeekGoalRequest request);
    Task<ApiResponse<bool?>> DeleteAsync(int goalId, int userId);
    Task<ApiResponse<bool?>> ToggleAchievedAsync(int goalId, int userId, bool isAchieved);
}

public interface IWeekNoteService
{
    Task<ApiResponse<List<WeekNoteDto>>> GetByWeekAsync(int weekId, int userId);
    Task<ApiResponse<WeekNoteDto>> CreateAsync(int userId, CreateWeekNoteRequest request);
    Task<ApiResponse<WeekNoteDto>> UpdateAsync(int noteId, int userId, UpdateWeekNoteRequest request);
    Task<ApiResponse<bool?>> DeleteAsync(int noteId, int userId);
}

public interface ILookupService
{
    Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync(string? type = null);
    Task<ApiResponse<List<DayOfWeekDto>>> GetDaysOfWeekAsync();
    Task<ApiResponse<List<TimeSlotDto>>> GetTimeSlotsAsync();
}
