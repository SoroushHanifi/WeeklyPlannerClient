using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeeklyPlannerAPI.Models.DTOs;
using WeeklyPlannerAPI.Services.Interfaces;

namespace WeeklyPlannerAPI.Controllers;

// ─── Base helper ─────────────────────────────────────────────────────────────
[ApiController]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

// ─── AuthController ───────────────────────────────────────────────────────────
[AllowAnonymous]
[Route("api/auth")]
public class AuthController(IAuthService authService, IConfiguration config) : ControllerBase
{
    private const string CookieName = "wp_token";

    /// <summary>ورود به سیستم — Token در HttpOnly Cookie ذخیره می‌شود</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (result, token) = await authService.LoginAsync(request);
        if (!result.Success || token is null)
            return Unauthorized(result);

        var expireDays = double.Parse(config["JwtSettings:ExpireDays"] ?? "7");

        Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly  = true,
            Secure    = bool.Parse(config["CookieSettings:Secure"] ?? "true"),
            SameSite  = Enum.Parse<SameSiteMode>(config["CookieSettings:SameSite"] ?? "Strict"),
            Expires   = DateTimeOffset.UtcNow.AddDays(expireDays),
            Path      = "/",
        });

        return Ok(result);
    }

    /// <summary>ثبت‌نام کاربر جدید</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>خروج از سیستم — Cookie پاک می‌شود</summary>
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Strict,
            Path     = "/",
        });
        return Ok(new ApiResponse<bool>(true, "با موفقیت خارج شدید", true));
    }

    /// <summary>بررسی وضعیت لاگین (برای فرانت‌اند)</summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult CheckAuth()
    {
        var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue(ClaimTypes.Name);
        var fullName = User.FindFirstValue("fullName");
        var lang     = User.FindFirstValue("lang");
        return Ok(new ApiResponse<object>(true, null, new { userId, username, fullName, lang }));
    }
}

// ─── UserController ──────────────────────────────────────────────────────────
[Route("api/users")]
public class UserController(IUserService userService) : BaseController
{
    /// <summary>دریافت پروفایل کاربر جاری</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await userService.GetProfileAsync(CurrentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>بروزرسانی پروفایل</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var result = await userService.UpdateProfileAsync(CurrentUserId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>تغییر رمز عبور</summary>
    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await userService.ChangePasswordAsync(CurrentUserId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>حذف حساب کاربری</summary>
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount()
    {
        var result = await userService.DeleteAccountAsync(CurrentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

// ─── WeeksController ─────────────────────────────────────────────────────────
[Route("api/weeks")]
public class WeeksController(IWeekService weekService) : BaseController
{
    /// <summary>لیست تمام هفته‌های کاربر</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeTemplates = false)
    {
        var result = await weekService.GetUserWeeksAsync(CurrentUserId, includeTemplates);
        return Ok(result);
    }

    /// <summary>خلاصه آمار هفته‌ها</summary>
    [HttpGet("summaries")]
    public async Task<IActionResult> GetSummaries()
    {
        var result = await weekService.GetWeekSummariesAsync(CurrentUserId);
        return Ok(result);
    }

    /// <summary>هفته جاری</summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent()
    {
        var result = await weekService.GetCurrentWeekAsync(CurrentUserId);
        return Ok(result);
    }

    /// <summary>دریافت یک هفته</summary>
    [HttpGet("{weekId:int}")]
    public async Task<IActionResult> GetById(int weekId)
    {
        var result = await weekService.GetWeekAsync(weekId, CurrentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>دریافت هفته کامل با تمام جزئیات</summary>
    [HttpGet("{weekId:int}/full")]
    public async Task<IActionResult> GetFull(int weekId)
    {
        var result = await weekService.GetFullWeekAsync(weekId, CurrentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>ایجاد هفته جدید</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWeekRequest request)
    {
        var result = await weekService.CreateWeekAsync(CurrentUserId, request);
        return result.Success ? CreatedAtAction(nameof(GetById), new { weekId = result.Data!.WeekId }, result) : BadRequest(result);
    }

    /// <summary>بروزرسانی هفته</summary>
    [HttpPut("{weekId:int}")]
    public async Task<IActionResult> Update(int weekId, [FromBody] UpdateWeekRequest request)
    {
        var result = await weekService.UpdateWeekAsync(weekId, CurrentUserId, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>حذف هفته</summary>
    [HttpDelete("{weekId:int}")]
    public async Task<IActionResult> Delete(int weekId)
    {
        var result = await weekService.DeleteWeekAsync(weekId, CurrentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>کپی هفته به عنوان template</summary>
    [HttpPost("copy")]
    public async Task<IActionResult> CopyWeek([FromBody] CopyWeekRequest request)
    {
        var result = await weekService.CopyWeekAsync(CurrentUserId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// ─── TimeBlocksController ─────────────────────────────────────────────────────
[Route("api/timeblocks")]
public class TimeBlocksController(ITimeBlockService service) : BaseController
{
    [HttpGet("week/{weekId:int}")]
    public async Task<IActionResult> GetByWeek(int weekId)
        => Ok(await service.GetByWeekAsync(weekId, CurrentUserId));

    [HttpGet("week/{weekId:int}/day/{dayId:int}")]
    public async Task<IActionResult> GetByDay(int weekId, byte dayId)
        => Ok(await service.GetByDayAsync(weekId, CurrentUserId, dayId));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await service.GetByIdAsync(id, CurrentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTimeBlockRequest request)
    {
        var result = await service.CreateAsync(CurrentUserId, request);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.TimeBlockId }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTimeBlockRequest request)
    {
        var result = await service.UpdateAsync(id, CurrentUserId, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeleteAsync(id, CurrentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPatch("{id:int}/complete")]
    public async Task<IActionResult> ToggleComplete(int id, [FromBody] CompleteTimeBlockRequest request)
    {
        var result = await service.ToggleCompleteAsync(id, CurrentUserId, request.IsCompleted);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

// ─── WeekTasksController ──────────────────────────────────────────────────────
[Route("api/tasks")]
public class WeekTasksController(IWeekTaskService service) : BaseController
{
    [HttpGet("week/{weekId:int}")]
    public async Task<IActionResult> GetByWeek(int weekId)
        => Ok(await service.GetByWeekAsync(weekId, CurrentUserId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWeekTaskRequest request)
    {
        var result = await service.CreateAsync(CurrentUserId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWeekTaskRequest request)
    {
        var result = await service.UpdateAsync(id, CurrentUserId, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeleteAsync(id, CurrentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id, [FromBody] ToggleTaskRequest request)
    {
        var result = await service.ToggleDoneAsync(id, CurrentUserId, request.IsDone);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

// ─── WeekGoalsController ──────────────────────────────────────────────────────
[Route("api/goals")]
public class WeekGoalsController(IWeekGoalService service) : BaseController
{
    [HttpGet("week/{weekId:int}")]
    public async Task<IActionResult> GetByWeek(int weekId)
        => Ok(await service.GetByWeekAsync(weekId, CurrentUserId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWeekGoalRequest request)
    {
        var result = await service.CreateAsync(CurrentUserId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWeekGoalRequest request)
    {
        var result = await service.UpdateAsync(id, CurrentUserId, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeleteAsync(id, CurrentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id, [FromBody] ToggleGoalRequest request)
    {
        var result = await service.ToggleAchievedAsync(id, CurrentUserId, request.IsAchieved);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

// ─── WeekNotesController ──────────────────────────────────────────────────────
[Route("api/notes")]
public class WeekNotesController(IWeekNoteService service) : BaseController
{
    [HttpGet("week/{weekId:int}")]
    public async Task<IActionResult> GetByWeek(int weekId)
        => Ok(await service.GetByWeekAsync(weekId, CurrentUserId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWeekNoteRequest request)
    {
        var result = await service.CreateAsync(CurrentUserId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWeekNoteRequest request)
    {
        var result = await service.UpdateAsync(id, CurrentUserId, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeleteAsync(id, CurrentUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

// ─── LookupController ─────────────────────────────────────────────────────────
[AllowAnonymous]
[Route("api/lookup")]
public class LookupController(ILookupService service) : ControllerBase
{
    /// <summary>دریافت دسته‌بندی‌ها (type: TIMEBLOCK | NOTE | BOTH)</summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories([FromQuery] string? type = null)
        => Ok(await service.GetCategoriesAsync(type));

    [HttpGet("days")]
    public async Task<IActionResult> GetDays()
        => Ok(await service.GetDaysOfWeekAsync());

    [HttpGet("timeslots")]
    public async Task<IActionResult> GetTimeSlots()
        => Ok(await service.GetTimeSlotsAsync());
}
