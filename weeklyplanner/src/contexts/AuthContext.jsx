import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { authAPI, saveCookieToken, deleteCookieToken, hasToken } from '../api/client';

const AuthContext = createContext(null);

function extractToken(data) {
  if (!data) return null;
  const fields = ['token', 'accessToken', 'access_token', 'jwt', 'jwtToken', 'bearerToken', 'authToken', 'idToken', 'id_token'];
  for (const f of fields) {
    if (data[f] && typeof data[f] === 'string') return data[f];
  }
  if (data.data && typeof data.data === 'object') return extractToken(data.data);
  if (data.result && typeof data.result === 'object') return extractToken(data.result);
  return null;
}

function extractUser(data) {
  if (!data) return null;
  const userFields = ['user', 'userData', 'userInfo', 'account', 'profile'];
  for (const f of userFields) {
    if (data[f] && typeof data[f] === 'object') return data[f];
  }
  if (data.id || data.username || data.email || data.userId) return data;
  return null;
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  const fetchMe = useCallback(async () => {
    try {
      const res = await authAPI.me();
      if (res.data) {
        const u = extractUser(res.data) || res.data;
        setUser(u);
        return u;
      }
    } catch {
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
    if (!hasToken()) {
      setLoading(false);
      return;
    }

    fetchMe();
  }, [fetchMe]);

    const login = async (username, password) => {
    const res = await authAPI.login({ username, password });
    console.log('[Auth] Login response:', JSON.stringify(res.data));

    const token = extractToken(res.data);
    if (token) {
      console.log('[Auth] Token found, saving...');
      saveCookieToken(token);
    }

    const userFromLogin = extractUser(res.data);

    try {
      const meRes = await authAPI.me();
      const u = extractUser(meRes.data) || meRes.data;
      console.log("SET USER:", u);
      if (u && (u.id || u.username || u.email)) {
        setUser(u);
        setLoading(false);
        return res.data;
      }
    } catch (e) {
      console.log('[Auth] /me failed:', e.message);
    }

    if (userFromLogin) {
      setUser(userFromLogin);
    } else if (token) {
      setUser({ username, loggedIn: true });
    }
    setLoading(false);
    return res.data;
  };

  const register = async (data) => {
    const res = await authAPI.register(data);
    const token = extractToken(res.data);
    if (token) saveCookieToken(token);
    try {
      const meRes = await authAPI.me();
      const u = extractUser(meRes.data) || meRes.data;
      if (u) { setUser(u); setLoading(false); return res.data; }
    } catch {}
    const u = extractUser(res.data);
    if (u) setUser(u);
    else if (token) setUser({ username: data.username, loggedIn: true });
    setLoading(false);
    return res.data;
  };

  const logout = async () => {
    try { await authAPI.logout(); } catch {}
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
