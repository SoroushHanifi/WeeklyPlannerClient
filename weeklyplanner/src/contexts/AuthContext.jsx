import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { authAPI, deleteCookieToken } from '../api/client';

const AuthContext = createContext(null);

/**
 * Backend response structure: { success, message, data: { userId, username, fullName, lang } }
 * این تابع کاربر رو از هر ساختاری که API برگردونه استخراج می‌کنه
 */
function extractUser(apiResponseData) {
  if (!apiResponseData) return null;

  // ساختار مستقیم: { userId, username, ... }
  if (apiResponseData.userId || apiResponseData.username || apiResponseData.id) {
    return apiResponseData;
  }

  // ساختار wrapper: { success, message, data: { userId, username, ... } }
  const nested = apiResponseData.data;
  if (nested && typeof nested === 'object' && !Array.isArray(nested)) {
    if (nested.userId || nested.username || nested.id) {
      return nested;
    }
  }

  // فیلدهای احتمالی دیگه
  for (const field of ['user', 'userData', 'userInfo', 'account', 'profile']) {
    if (apiResponseData[field] && typeof apiResponseData[field] === 'object') {
      return apiResponseData[field];
    }
  }

  return null;
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  /**
   * همیشه سعی می‌کنیم /me رو صدا بزنیم — اگه کوکی backend معتبر باشه کار می‌کنه،
   * اگه نباشه 401 می‌گیریم و user رو null می‌ذاریم.
   * نیازی نیست hasToken() چک کنیم چون کوکی HttpOnly هست و JS نمی‌تونه بخوندش.
   */
  const fetchMe = useCallback(async () => {
    try {
      const res = await authAPI.me();
      const u = extractUser(res.data);
      if (u) {
        setUser(u);
        return u;
      }
      setUser(null);
    } catch (error) {
      if (error.response?.status === 401) {
        deleteCookieToken();
      }
      setUser(null);
    } finally {
      setLoading(false);
    }
    return null;
  }, []);

  useEffect(() => {
    // همیشه بررسی می‌کنیم — backend کوکی HttpOnly مدیریت می‌کنه
    fetchMe();
  }, [fetchMe]);

  const login = async (username, password) => {
    const res = await authAPI.login({ username, password });

    // بعد از لاگین موفق، اطلاعات کاربر رو از /me می‌گیریم
    // چون backend توکن رو در HttpOnly Cookie ذخیره کرده و حالا معتبره
    await fetchMe();

    return res.data;
  };

  const register = async (data) => {
    const res = await authAPI.register(data);
    await fetchMe();
    return res.data;
  };

  const logout = async () => {
    try {
      await authAPI.logout();
    } catch {}
    deleteCookieToken();
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout, refetchUser: fetchMe }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}