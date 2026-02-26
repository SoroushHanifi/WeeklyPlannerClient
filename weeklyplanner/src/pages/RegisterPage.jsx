import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { Eye, EyeOff, CalendarDays, UserPlus } from 'lucide-react'
import toast from 'react-hot-toast'
import styles from './AuthPage.module.css'

export default function RegisterPage() {
  const { register } = useAuth()
  const [form, setForm] = useState({
    username: '', fullName: '', email: '', password: '',
    timeZone: 'Asia/Tehran', preferredLang: 'fa'
  })
  const [showPass, setShowPass] = useState(false)
  const [loading, setLoading] = useState(false)

  const set = (key) => (e) => setForm(p => ({ ...p, [key]: e.target.value }))

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!form.username || !form.password || !form.fullName) {
      toast.error('لطفاً فیلدهای اجباری را پر کنید')
      return
    }
    setLoading(true)
    try {
      await register(form)
      toast.success('ثبت‌نام با موفقیت انجام شد!')
    } catch (err) {
      toast.error(err.response?.data?.message || 'خطا در ثبت‌نام')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className={styles.container}>
      <div className={styles.bg} />
      <div className={styles.box} style={{ maxWidth: 460 }}>
        <div className={styles.logo}><CalendarDays size={28} /></div>
        <h1 className={styles.title}>ثبت‌نام</h1>
        <p className={styles.subtitle}>ساخت حساب جدید</p>

        <form onSubmit={handleSubmit} className={styles.form}>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div className="form-group" style={{ marginBottom: 0 }}>
              <label className="form-label">نام کاربری *</label>
              <input className="form-input" placeholder="username" value={form.username} onChange={set('username')} />
            </div>
            <div className="form-group" style={{ marginBottom: 0 }}>
              <label className="form-label">نام کامل *</label>
              <input className="form-input" placeholder="نام و نام‌خانوادگی" value={form.fullName} onChange={set('fullName')} />
            </div>
          </div>
          <div className="form-group" style={{ marginTop: 12 }}>
            <label className="form-label">ایمیل</label>
            <input className="form-input" type="email" placeholder="example@mail.com" value={form.email} onChange={set('email')} />
          </div>
          <div className="form-group">
            <label className="form-label">رمز عبور *</label>
            <div style={{ position: 'relative' }}>
              <input
                className="form-input"
                type={showPass ? 'text' : 'password'}
                placeholder="حداقل ۸ کاراکتر"
                value={form.password}
                onChange={set('password')}
                style={{ paddingLeft: '42px' }}
              />
              <button type="button" className={styles.eyeBtn} onClick={() => setShowPass(p => !p)}>
                {showPass ? <EyeOff size={16} /> : <Eye size={16} />}
              </button>
            </div>
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div className="form-group" style={{ marginBottom: 0 }}>
              <label className="form-label">منطقه زمانی</label>
              <select className="form-input" value={form.timeZone} onChange={set('timeZone')}>
                <option value="Asia/Tehran">تهران</option>
                <option value="Asia/Dubai">دبی</option>
                <option value="UTC">UTC</option>
              </select>
            </div>
            <div className="form-group" style={{ marginBottom: 0 }}>
              <label className="form-label">زبان</label>
              <select className="form-input" value={form.preferredLang} onChange={set('preferredLang')}>
                <option value="fa">فارسی</option>
                <option value="en">English</option>
              </select>
            </div>
          </div>

          <button type="submit" className={`btn btn-primary ${styles.submitBtn}`} disabled={loading} style={{ marginTop: 16 }}>
            {loading
              ? <span className="spinner" style={{ width: 20, height: 20, borderWidth: 2 }} />
              : <><UserPlus size={16} /> ثبت‌نام</>
            }
          </button>
        </form>

        <p className={styles.footer}>
          قبلاً ثبت‌نام کرده‌اید؟{' '}
          <Link to="/login" className={styles.link}>ورود</Link>
        </p>
      </div>
    </div>
  )
}
