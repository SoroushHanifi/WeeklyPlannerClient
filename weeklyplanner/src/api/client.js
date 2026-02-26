import axios from 'axios';

const BASE_URL = import.meta.env.VITE_API_URL || '';

const api = axios.create({
  baseURL: BASE_URL,
  withCredentials: true, // Send cookies with every request
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - attach JWT from cookie if available
api.interceptors.request.use(
  (config) => {
    // Read JWT token from cookie
    const token = getCookieToken();
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - handle 401
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Clear cookie and redirect to login
      deleteCookieToken();
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

function getCookieToken() {
  const name = 'jwt_token=';
  const decodedCookie = decodeURIComponent(document.cookie);
  const parts = decodedCookie.split(';');
  for (let part of parts) {
    let c = part.trim();
    if (c.indexOf(name) === 0) {
      return c.substring(name.length);
    }
  }
  return null;
}

export function saveCookieToken(token) {
  const expires = new Date();
  expires.setDate(expires.getDate() + 7);
  document.cookie = `jwt_token=${token}; expires=${expires.toUTCString()}; path=/; SameSite=Strict`;
}

export function deleteCookieToken() {
  document.cookie = 'jwt_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
}

export function hasToken() {
  return !!getCookieToken();
}

// ─── Auth ────────────────────────────────────────────────────────────────────
export const authAPI = {
  login: (data) => api.post('/api/auth/login', data),
  register: (data) => api.post('/api/auth/register', data),
  logout: () => api.post('/api/auth/logout'),
  me: () => api.get('/api/auth/me'),
};

// ─── Weeks ───────────────────────────────────────────────────────────────────
export const weeksAPI = {
  getAll: (includeTemplates = false) => api.get(`/api/weeks?includeTemplates=${includeTemplates}`),
  getSummaries: () => api.get('/api/weeks/summaries'),
  getCurrent: () => api.get('/api/weeks/current'),
  getById: (id) => api.get(`/api/weeks/${id}`),
  getFull: (id) => api.get(`/api/weeks/${id}/full`),
  create: (data) => api.post('/api/weeks', data),
  update: (id, data) => api.put(`/api/weeks/${id}`, data),
  delete: (id) => api.delete(`/api/weeks/${id}`),
  copy: (data) => api.post('/api/weeks/copy', data),
};

// ─── TimeBlocks ──────────────────────────────────────────────────────────────
export const timeBlocksAPI = {
  getByWeek: (weekId) => api.get(`/api/timeblocks/week/${weekId}`),
  getByDay: (weekId, dayId) => api.get(`/api/timeblocks/week/${weekId}/day/${dayId}`),
  getById: (id) => api.get(`/api/timeblocks/${id}`),
  create: (data) => api.post('/api/timeblocks', data),
  update: (id, data) => api.put(`/api/timeblocks/${id}`, data),
  delete: (id) => api.delete(`/api/timeblocks/${id}`),
  complete: (id, isCompleted) => api.patch(`/api/timeblocks/${id}/complete`, { isCompleted }),
};

// ─── Goals ───────────────────────────────────────────────────────────────────
export const goalsAPI = {
  getByWeek: (weekId) => api.get(`/api/goals/week/${weekId}`),
  create: (data) => api.post('/api/goals', data),
  update: (id, data) => api.put(`/api/goals/${id}`, data),
  delete: (id) => api.delete(`/api/goals/${id}`),
  toggle: (id, isAchieved) => api.patch(`/api/goals/${id}/toggle`, { isAchieved }),
};

// ─── Tasks ───────────────────────────────────────────────────────────────────
export const tasksAPI = {
  getByWeek: (weekId) => api.get(`/api/tasks/week/${weekId}`),
  create: (data) => api.post('/api/tasks', data),
  update: (id, data) => api.put(`/api/tasks/${id}`, data),
  delete: (id) => api.delete(`/api/tasks/${id}`),
  toggle: (id, isDone) => api.patch(`/api/tasks/${id}/toggle`, { isDone }),
};

// ─── Notes ───────────────────────────────────────────────────────────────────
export const notesAPI = {
  getByWeek: (weekId) => api.get(`/api/notes/week/${weekId}`),
  create: (data) => api.post('/api/notes', data),
  update: (id, data) => api.put(`/api/notes/${id}`, data),
  delete: (id) => api.delete(`/api/notes/${id}`),
};

// ─── Lookup ──────────────────────────────────────────────────────────────────
export const lookupAPI = {
  getCategories: (type) => api.get(`/api/lookup/categories${type ? `?type=${type}` : ''}`),
  getDays: () => api.get('/api/lookup/days'),
  getTimeSlots: () => api.get('/api/lookup/timeslots'),
};

// ─── User ────────────────────────────────────────────────────────────────────
export const userAPI = {
  getMe: () => api.get('/api/users/me'),
  update: (data) => api.put('/api/users/me', data),
  delete: () => api.delete('/api/users/me'),
  changePassword: (data) => api.post('/api/users/me/change-password', data),
};

export default api;
