# Weekly Planner API — راهنمای راه‌اندازی

## پیش‌نیازها
- .NET 8 SDK
- SQL Server (هر نسخه‌ای، از جمله Express)
- IDE: Visual Studio 2022 / Rider / VS Code

---

## ساختار پروژه

```
WeeklyPlannerAPI/
├── WeeklyPlannerAPI.sln
└── WeeklyPlannerAPI/
    ├── Controllers/
    │   └── Controllers.cs          ← تمام کنترلرها
    ├── Data/
    │   └── AppDbContext.cs         ← EF Core DbContext
    ├── Middleware/
    │   └── ExceptionMiddleware.cs  ← مدیریت خطاهای سراسری
    ├── Models/
    │   ├── Entities/Entities.cs    ← موجودیت‌های EF Core
    │   └── DTOs/DTOs.cs            ← مدل‌های ورودی/خروجی API
    ├── Repositories/
    │   ├── Interfaces/IRepositories.cs
    │   └── Repositories.cs
    ├── Services/
    │   ├── Interfaces/IServices.cs
    │   └── Services.cs
    ├── Program.cs                  ← تنظیمات DI و middleware
    ├── appsettings.json
    └── appsettings.Development.json
```

---

## مراحل راه‌اندازی

### ۱. دیتابیس
ابتدا اسکریپت SQL دیتابیس را اجرا کنید تا دیتابیس `WeeklyPlannerDB` ساخته شود.

### ۲. Connection String
در `appsettings.json` مقدار را تنظیم کنید:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=WeeklyPlannerDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### ۳. JWT Secret Key
در `appsettings.json` یک کلید مخفی قوی (حداقل ۳۲ کاراکتر) تنظیم کنید:
```json
"JwtSettings": {
  "SecretKey": "YOUR_SUPER_SECRET_KEY_MINIMUM_32_CHARACTERS_LONG!!"
}
```

### ۴. نصب پکیج‌ها و اجرا
```bash
cd WeeklyPlannerAPI
dotnet restore
dotnet run
```

### ۵. دسترسی به Swagger
پس از اجرا به آدرس زیر بروید:
```
http://localhost:5000
```

---

## Endpoints

### 🔓 Auth (بدون نیاز به Token)
| Method | URL | توضیح |
|--------|-----|-------|
| POST | `/api/auth/register` | ثبت‌نام |
| POST | `/api/auth/login` | ورود و دریافت JWT Token |

### 👤 User (نیاز به Token)
| Method | URL | توضیح |
|--------|-----|-------|
| GET | `/api/users/me` | دریافت پروفایل |
| PUT | `/api/users/me` | بروزرسانی پروفایل |
| POST | `/api/users/me/change-password` | تغییر رمز عبور |
| DELETE | `/api/users/me` | حذف حساب |

### 📅 Weeks (نیاز به Token)
| Method | URL | توضیح |
|--------|-----|-------|
| GET | `/api/weeks` | لیست هفته‌ها |
| GET | `/api/weeks/current` | هفته جاری |
| GET | `/api/weeks/summaries` | خلاصه آمار (از VW_WeekSummary) |
| GET | `/api/weeks/{id}` | جزئیات یک هفته |
| GET | `/api/weeks/{id}/full` | هفته کامل با تمام بلوک‌ها و وظایف |
| POST | `/api/weeks` | ایجاد هفته جدید |
| PUT | `/api/weeks/{id}` | بروزرسانی |
| DELETE | `/api/weeks/{id}` | حذف |
| POST | `/api/weeks/copy` | کپی هفته (SP_CopyWeekAsTemplate) |

### ⏱ TimeBlocks (نیاز به Token)
| Method | URL | توضیح |
|--------|-----|-------|
| GET | `/api/timeblocks/week/{weekId}` | بلوک‌های یک هفته |
| GET | `/api/timeblocks/week/{weekId}/day/{dayId}` | بلوک‌های یک روز |
| GET | `/api/timeblocks/{id}` | جزئیات |
| POST | `/api/timeblocks` | ایجاد |
| PUT | `/api/timeblocks/{id}` | ویرایش |
| DELETE | `/api/timeblocks/{id}` | حذف |
| PATCH | `/api/timeblocks/{id}/complete` | تغییر وضعیت تکمیل |

### ✅ Tasks, Goals, Notes
مشابه TimeBlocks — endpoint پایه:
- `/api/tasks/week/{weekId}` (GET, POST), `/api/tasks/{id}` (PUT, DELETE)
- `/api/tasks/{id}/toggle` (PATCH) — mark done/undone
- `/api/goals/...` — همین ساختار
- `/api/notes/...` — همین ساختار

### 🔍 Lookup (بدون Token)
| Method | URL | توضیح |
|--------|-----|-------|
| GET | `/api/lookup/categories?type=TIMEBLOCK` | دسته‌بندی‌ها |
| GET | `/api/lookup/days` | روزهای هفته |
| GET | `/api/lookup/timeslots` | اسلات‌های زمانی |

---

## نحوه استفاده از Token در Swagger
1. POST `/api/auth/login` را صدا بزنید
2. توکن را از پاسخ کپی کنید
3. روی دکمه Authorize در بالای صفحه کلیک کنید
4. توکن را paste کنید

---

## نکات امنیتی برای Production
- `SecretKey` را در Environment Variable قرار دهید نه در appsettings.json
- `detail` را از ExceptionMiddleware حذف کنید
- HTTPS را اجباری کنید
- Rate Limiting اضافه کنید
