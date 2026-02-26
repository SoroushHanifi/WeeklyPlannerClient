import { useState } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { userAPI } from '../api/client'
import { User, Lock, Trash2, Save } from 'lucide-react'
import toast from 'react-hot-toast'
import { useNavigate } from 'react-router-dom'
import styles from './ProfilePage.module.css'

export default function ProfilePage() {
  const { user, refetchUser, logout } = useAuth()
  const navigate = useNavigate()

  const [profileForm, setProfileForm] = useState({
    fullName: user?.fullName || '',
    email: user?.email || '',
    timeZone: user?.timeZone || 'Asia/Tehran',
    preferredLang: user?.preferredLang || 'fa',
  })
  const [passForm, setPassForm] = useState({ oldPassword: '', newPassword: '' })
  const [saving, setSaving] = useState(false)
  const [savingPass, setSavingPass] = useState(false)

  const setP = (k) => (e) => setProfileForm(p => ({ ...p, [k]: e.target.value }))
  const setPP = (k) => (e) => setPassForm(p => ({ ...p, [k]: e.target.value }))

  const handleSaveProfile = async (e) => {
    e.preventDefault()
    setSaving(true)
    try {
      await userAPI.update(profileForm)
      await refetchUser()
      toast.success('پروفایل به‌روز شد')
    } catch { toast.error('خطا در ذخیره') } finally { setSaving(false) }
  }

  const handleChangePassword = async (e) => {
    e.preventDefault()
    if (!passForm.oldPassword || !passForm.newPassword) { toast.error('همه فیلدها الزامی هستند'); return }
    setSavingPass(true)
    try {
      await userAPI.changePassword(passForm)
      setPassForm({ oldPassword: '', newPassword: '' })
      toast.success('رمز عبور تغییر کرد')
    } catch { toast.error('خطا در تغییر رمز') } finally { setSavingPass(false) }
  }

  const handleDelete = async () => {
    if (!confirm('آیا مطمئن هستید؟ این عمل برگشت‌ناپذیر است.')) return
    try {
      await userAPI.delete()
      await logout()
      navigate('/login')
      toast.success('حساب حذف شد')
    } catch { toast.error('خطا') }
  }

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div className={styles.avatar}>{user?.fullName?.[0] || user?.username?.[0] || 'U'}</div>
        <div>
          <h1 className={styles.title}>{user?.fullName || user?.username}</h1>
          <p className={styles.sub}>@{user?.username}</p>
        </div>
      </div>

      <div className={styles.sections}>
        {/* Profile Section */}
        <div className="card">
          <h2 className={styles.sectionTitle}><User size={16} /> اطلاعات شخصی</h2>
          <div className="divider" />
          <form onSubmit={handleSaveProfile}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
              <div className="form-group">
                <label className="form-label">نام کامل</label>
                <input className="form-input" value={profileForm.fullName} onChange={setP('fullName')} />
              </div>
              <div className="form-group">
                <label className="form-label">ایمیل</label>
                <input className="form-input" type="email" value={profileForm.email} onChange={setP('email')} />
              </div>
              <div className="form-group">
                <label className="form-label">منطقه زمانی</label>
                <select className="form-input" value={profileForm.timeZone} onChange={setP('timeZone')}>
                  <option value="Asia/Tehran">تهران</option>
                  <option value="Asia/Dubai">دبی</option>
                  <option value="UTC">UTC</option>
                </select>
              </div>
              <div className="form-group">
                <label className="form-label">زبان</label>
                <select className="form-input" value={profileForm.preferredLang} onChange={setP('preferredLang')}>
                  <option value="fa">فارسی</option>
                  <option value="en">English</option>
                </select>
              </div>
            </div>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? <span className="spinner" style={{ width: 16, height: 16, borderWidth: 2 }} /> : <><Save size={15} /> ذخیره</>}
            </button>
          </form>
        </div>

        {/* Password Section */}
        <div className="card">
          <h2 className={styles.sectionTitle}><Lock size={16} /> تغییر رمز عبور</h2>
          <div className="divider" />
          <form onSubmit={handleChangePassword}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
              <div className="form-group">
                <label className="form-label">رمز فعلی</label>
                <input className="form-input" type="password" value={passForm.oldPassword} onChange={setPP('oldPassword')} />
              </div>
              <div className="form-group">
                <label className="form-label">رمز جدید</label>
                <input className="form-input" type="password" value={passForm.newPassword} onChange={setPP('newPassword')} />
              </div>
            </div>
            <button type="submit" className="btn btn-primary" disabled={savingPass}>
              {savingPass ? <span className="spinner" style={{ width: 16, height: 16, borderWidth: 2 }} /> : <><Save size={15} /> تغییر رمز</>}
            </button>
          </form>
        </div>

        {/* Danger Zone */}
        <div className="card" style={{ borderColor: 'rgba(248,113,113,0.2)' }}>
          <h2 className={styles.sectionTitle} style={{ color: 'var(--danger)' }}><Trash2 size={16} /> ناحیه خطر</h2>
          <div className="divider" />
          <p style={{ fontSize: 13, color: 'var(--text-muted)', marginBottom: 16 }}>
            حذف حساب کاربری غیرقابل بازگشت است و تمام داده‌های شما پاک می‌شود.
          </p>
          <button className="btn btn-danger" onClick={handleDelete}>
            <Trash2 size={15} /> حذف حساب
          </button>
        </div>
      </div>
    </div>
  )
}
