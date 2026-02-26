import { useState } from 'react'
import { weeksAPI } from '../../api/client'
import { X, Save, CalendarDays } from 'lucide-react'
import toast from 'react-hot-toast'

function getTodayISO() {
  return new Date().toISOString().slice(0, 10)
}

function getEndOfWeekISO() {
  const d = new Date()
  d.setDate(d.getDate() + 6)
  return d.toISOString().slice(0, 10)
}

export default function CreateWeekModal({ onClose, onCreated }) {
  const [form, setForm] = useState({
    startDate: getTodayISO(),
    endDate: getEndOfWeekISO(),
    startDateShamsi: '',
    endDateShamsi: '',
    title: '',
    isTemplate: false,
  })
  const [loading, setLoading] = useState(false)

  const set = (k) => (e) => setForm(p => ({ ...p, [k]: e.target.value }))
  const setCheck = (k) => (e) => setForm(p => ({ ...p, [k]: e.target.checked }))

  const handleSubmit = async (e) => {
    e.preventDefault()
    setLoading(true)
    try {
      const res = await weeksAPI.create(form)
      toast.success('هفته ساخته شد')
      onCreated(res.data)
    } catch {
      toast.error('خطا در ایجاد هفته')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal-box">
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 24 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <div style={{ width: 36, height: 36, background: 'var(--accent-dim)', border: '1px solid var(--border-accent)', borderRadius: 8, display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--accent)' }}>
              <CalendarDays size={18} />
            </div>
            <h2 style={{ fontSize: 17, fontWeight: 700 }}>هفته جدید</h2>
          </div>
          <button className="btn-icon" onClick={onClose}><X size={18} /></button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">عنوان هفته</label>
            <input className="form-input" placeholder="مثال: هفته اول اردیبهشت" value={form.title} onChange={set('title')} />
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div className="form-group">
              <label className="form-label">تاریخ شروع</label>
              <input className="form-input" type="date" value={form.startDate} onChange={set('startDate')} />
            </div>
            <div className="form-group">
              <label className="form-label">تاریخ پایان</label>
              <input className="form-input" type="date" value={form.endDate} onChange={set('endDate')} />
            </div>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div className="form-group">
              <label className="form-label">تاریخ شروع (شمسی)</label>
              <input className="form-input" placeholder="مثال: ۱۴۰۳/۰۱/۰۱" value={form.startDateShamsi} onChange={set('startDateShamsi')} />
            </div>
            <div className="form-group">
              <label className="form-label">تاریخ پایان (شمسی)</label>
              <input className="form-input" placeholder="مثال: ۱۴۰۳/۰۱/۰۷" value={form.endDateShamsi} onChange={set('endDateShamsi')} />
            </div>
          </div>

          <label style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 24, cursor: 'pointer', fontSize: 13, color: 'var(--text-secondary)' }}>
            <input type="checkbox" checked={form.isTemplate} onChange={setCheck('isTemplate')} />
            این هفته به عنوان قالب ذخیره شود
          </label>

          <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end' }}>
            <button type="button" className="btn btn-ghost" onClick={onClose}>انصراف</button>
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading
                ? <span className="spinner" style={{ width: 16, height: 16, borderWidth: 2 }} />
                : <><Save size={15} /> ایجاد هفته</>
              }
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
