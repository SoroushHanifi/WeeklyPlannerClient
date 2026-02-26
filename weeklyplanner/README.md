# 📅 برنامه‌ریز هفتگی — Weekly Planner Frontend

رابط کاربری حرفه‌ای React برای Weekly Planner API

## 🚀 راه‌اندازی سریع

### پیش‌نیازها
- Node.js 18+ (دانلود از nodejs.org)
- npm 8+

### نصب و اجرا

```bash
# ۱. نصب وابستگی‌ها
cd weekly-planner
npm install

# ۲. ساختن فایل تنظیمات محیطی
copy .env.example .env
# سپس ویرایش .env و تنظیم VITE_API_URL

# ۳. اجرا در حالت توسعه
npm run dev

# ۴. ساخت نسخه production
npm run build
```

---

## ⚙️ تنظیمات محیطی (.env)

```env
# آدرس backend API شما
VITE_API_URL=http://weekly.mblt.ir
```

> اگر frontend و backend روی یک دامنه هستند، `VITE_API_URL` را خالی بگذارید.

---

## 🔧 تنظیم CORS در Backend (.NET)

در `Program.cs` یا `Startup.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://weekly.mblt.ir",
                "https://weekly.mblt.ir",
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // مهم برای cookie/jwt
    });
});

// ...

app.UseCors("AllowFrontend");
```

---

## 🏗️ Deploy روی IIS

### ۱. Build پروژه
```bash
npm run build
# خروجی در پوشه dist/ قرار می‌گیرد
```

### ۲. نصب URL Rewrite Module
روی سرور IIS باید **URL Rewrite Module** نصب باشد:
- دانلود از: https://www.iis.net/downloads/microsoft/url-rewrite

### ۳. ساخت Website در IIS
1. IIS Manager را باز کنید
2. روی **Sites** کلیک راست → **Add Website**
3. تنظیمات:
   - **Site name**: WeeklyPlanner
   - **Physical path**: مسیر پوشه `dist/`
   - **Host name**: `weekly.mblt.ir`
   - **Port**: 80

### ۴. کپی فایل‌های dist به سرور
```
dist/
├── index.html
├── web.config  ← این فایل مهم است!
├── assets/
│   ├── index-[hash].js
│   └── index-[hash].css
└── favicon.svg
```

> ⚠️ `web.config` به صورت خودکار از پوشه `public/` به `dist/` کپی می‌شود

### ۵. تنظیم hosts
در `C:\Windows\System32\drivers\etc\hosts`:
```
127.0.0.1  weekly.mblt.ir
```

### ۶. Application Pool
- .NET CLR Version: **No Managed Code**
- Managed Pipeline Mode: **Integrated**

---

## 📁 ساختار پروژه

```
src/
├── api/
│   └── client.js          # تمام API calls + مدیریت JWT cookie
├── contexts/
│   └── AuthContext.jsx     # احراز هویت global
├── pages/
│   ├── LoginPage.jsx
│   ├── RegisterPage.jsx
│   ├── PlannerPage.jsx     # صفحه اصلی
│   └── ProfilePage.jsx
├── components/
│   ├── layout/
│   │   └── AppLayout.jsx   # Sidebar + Layout اصلی
│   └── planner/
│       ├── WeekHeader.jsx
│       ├── TimeBlocksView.jsx
│       ├── TimeBlockModal.jsx
│       ├── GoalsList.jsx
│       ├── TasksList.jsx
│       ├── NotesList.jsx
│       ├── CreateWeekModal.jsx
│       └── CopyWeekModal.jsx
└── styles/
    └── global.css
```

---

## 🔐 احراز هویت (JWT + Cookie)

توکن JWT در **cookie مرورگر** ذخیره می‌شود:
- نام cookie: `jwt_token`
- انقضا: ۷ روز
- SameSite: Strict

هر request به API به صورت خودکار هدر `Authorization: Bearer <token>` را دارد.

---

## 🌐 امکانات

| بخش | امکانات |
|-----|---------|
| احراز هویت | ورود، ثبت‌نام، خروج |
| هفته‌ها | ایجاد، ویرایش، حذف، کپی |
| برنامه زمانی | بلوک‌های روزانه با رنگ و اولویت |
| اهداف | تعریف، علامت‌گذاری، حذف |
| وظایف | مدیریت کامل با اولویت‌بندی |
| یادداشت‌ها | نوشتن و دسته‌بندی |
| پروفایل | ویرایش اطلاعات و تغییر رمز |

---

## 🎨 طراحی

- تم: **تاریک** با رنگ طلایی-کهربایی
- فونت: **Vazirmatn** (فارسی بهینه)
- RTL کامل
- واکنش‌گرا (Responsive)
