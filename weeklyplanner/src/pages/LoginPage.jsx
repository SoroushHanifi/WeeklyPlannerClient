import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { Eye, EyeOff, CalendarDays, LogIn, Bug } from 'lucide-react'
import toast from 'react-hot-toast'
import styles from './AuthPage.module.css'

export default function LoginPage() {
  const { login } = useAuth()
  const [form, setForm] = useState({ username: '', password: '' })
  const [showPass, setShowPass] = useState(false)
  const [loading, setLoading] = useState(false)
  const [debugInfo, setDebugInfo] = useState(null)

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!form.username || !form.password) {
      toast.error('لطفاً تمام فیلدها را پر کنید')
      return
    }
    setLoading(true)
    setDebugInfo(null)
    try {
      const data = await login(form.username, form.password)
      console.log("USER AFTER LOGIN:", data);
      setDebugInfo({ success: true, response: data })
      toast.success('خوش آمدید!')
    } catch (err) {
      const errData = {
        message: err.message,
        status: err.response?.status,
        response: err.response?.data,
      }
      setDebugInfo({ success: false, error: errData })
      toast.error(err.response?.data?.message || err.response?.data?.title || 'خطا در ورود')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className={styles.container}>
      <div className={styles.bg} />
      <div style={{ display: 'flex', flexDirection: 'column', gap: 16, width: '100%', maxWidth: 480, position: 'relative', zIndex: 1 }}>
        <div className={styles.box}>
          <div className={styles.logo}><CalendarDays size={28} /></div>
          <h1 className={styles.title}>ورود به حساب</h1>
          <p className={styles.subtitle}>برنامه‌ریزی هفتگی حرفه‌ای</p>

          <form onSubmit={handleSubmit} className={styles.form}>
            <div className="form-group">
              <label className="form-label">نام کاربری</label>
              <input
                className="form-input"
                type="text"
                placeholder="نام کاربری خود را وارد کنید"
                value={form.username}
                onChange={e => setForm(p => ({ ...p, username: e.target.value }))}
                autoComplete="username"
              />
            </div>
            <div className="form-group">
              <label className="form-label">رمز عبور</label>
              <div style={{ position: 'relative' }}>
                <input
                  className="form-input"
                  type={showPass ? 'text' : 'password'}
                  placeholder="رمز عبور خود را وارد کنید"
                  value={form.password}
                  onChange={e => setForm(p => ({ ...p, password: e.target.value }))}
                  autoComplete="current-password"
                  style={{ paddingLeft: '42px' }}
                />
                <button type="button" className={styles.eyeBtn} onClick={() => setShowPass(p => !p)}>
                  {showPass ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>
            <button type="submit" className={`btn btn-primary ${styles.submitBtn}`} disabled={loading}>
              {loading
                ? <span className="spinner" style={{ width: 20, height: 20, borderWidth: 2 }} />
                : <><LogIn size={16} /> ورود</>
              }
            </button>
          </form>
          <p className={styles.footer}>
            حساب ندارید؟{' '}
            <Link to="/register" className={styles.link}>ثبت‌نام کنید</Link>
          </p>
        </div>

        {/* Debug Panel */}
        {debugInfo && (
          <div style={{
            background: debugInfo.success ? 'rgba(74,222,128,0.08)' : 'rgba(248,113,113,0.08)',
            border: `1px solid ${debugInfo.success ? 'rgba(74,222,128,0.3)' : 'rgba(248,113,113,0.3)'}`,
            borderRadius: 12,
            padding: 16,
            fontSize: 12,
            direction: 'ltr',
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginBottom: 8, color: debugInfo.success ? '#4ade80' : '#f87171', fontWeight: 700 }}>
              <Bug size={14} />
              {debugInfo.success ? 'Login Successful — Response:' : 'Login Failed:'}
            </div>
            <pre style={{ color: '#8b90a8', overflow: 'auto', maxHeight: 200, margin: 0, fontSize: 11 }}>
              {JSON.stringify(debugInfo.success ? debugInfo.response : debugInfo.error, null, 2)}
            </pre>
            {debugInfo.success && (
              <p style={{ color: '#f5c842', marginTop: 8, textAlign: 'center', direction: 'rtl', fontSize: 13 }}>
                ✓ این اطلاعات رو به ما بگو تا توکن رو درست شناسایی کنیم
              </p>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
