import { useState } from 'react'
import { timeBlocksAPI } from '../../api/client'
import { X, Save } from 'lucide-react'
import toast from 'react-hot-toast'

const DAY_NAMES_FA = {
  Saturday: 'شنبه', Sunday: 'یک‌شنبه', Monday: 'دوشنبه',
  Tuesday: 'سه‌شنبه', Wednesday: 'چهارشنبه', Thursday: 'پنج‌شنبه', Friday: 'جمعه',
}

export default function TimeBlockModal({ weekId, dayId, block, days, categories, onClose, onSaved }) {
  const isEdit = !!block
  const [form, setForm] = useState({
    weekId,
    dayId: block?.dayId ?? dayId ?? (days[0]?.id || 1),
    startTime: block?.startTime?.slice(0, 5) || '08:00',
    endTime: block?.endTime?.slice(0, 5) || '09:00',
    activityTitle: block?.activityTitle || '',
    description: block?.description || '',
    categoryId: block?.categoryId || '',
    customColorHex: block?.customColorHex || '',
    priority: block?.priority || 2,
    isRecurring: block?.isRecurring || false,
  })
  const [loading, setLoading] = useState(false)

  const set = (k) => (e) => setForm(p => ({ ...p, [k]: e.target.value }))
  const setCheck = (k) => (e) => setForm(p => ({ ...p, [k]: e.target.checked }))

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!form.activityTitle) { toast.error('عنوان الزامی است'); return }
    setLoading(true)
    try {
      const payload = {
        ...form,
        dayId: parseInt(form.dayId),
        categoryId: form.categoryId ? parseInt(form.categoryId) : null,
        priority: parseInt(form.priority),
        startTime: form.startTime + ':00',
        endTime: form.endTime + ':00',
      }
      if (isEdit) {
        await timeBlocksAPI.update(block.id, payload)
        toast.success('بلوک ویرایش شد')
      } else {
        await timeBlocksAPI.create(payload)
        toast.success('بلوک اضافه شد')
      }
      onSaved()
    } catch {
      toast.error('خطا در ذخیره')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal-box">
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 24 }}>
          <h2 style={{ fontSize: 17, fontWeight: 700 }}>{isEdit ? 'ویرایش بلوک' : 'بلوک جدید'}</h2>
          <button className="btn-icon" onClick={onClose}><X size={18} /></button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">عنوان فعالیت *</label>
            <input className="form-input" placeholder="مثال: جلسه تیم" value={form.activityTitle} onChange={set('activityTitle')} />
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div className="form-group">
              <label className="form-label">روز هفته</label>
              <select className="form-input" value={form.dayId} onChange={set('dayId')}>
                {days.map(d => (
                  <option key={d.id} value={d.id}>
                    {DAY_NAMES_FA[d.name] || d.name || d.nameFa || `روز ${d.id}`}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">دسته‌بندی</label>
              <select className="form-input" value={form.categoryId} onChange={set('categoryId')}>
                <option value="">بدون دسته</option>
                {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </div>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div className="form-group">
              <label className="form-label">شروع</label>
              <input className="form-input" type="time" value={form.startTime} onChange={set('startTime')} />
            </div>
            <div className="form-group">
              <label className="form-label">پایان</label>
              <input className="form-input" type="time" value={form.endTime} onChange={set('endTime')} />
            </div>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div className="form-group">
              <label className="form-label">اولویت</label>
              <select className="form-input" value={form.priority} onChange={set('priority')}>
                <option value={1}>بالا</option>
                <option value={2}>متوسط</option>
                <option value={3}>پایین</option>
              </select>
            </div>
            <div className="form-group">
              <label className="form-label">رنگ سفارشی</label>
              <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                <input className="form-input" type="text" placeholder="#f5c842" value={form.customColorHex} onChange={set('customColorHex')} style={{ flex: 1 }} />
                {form.customColorHex && (
                  <div style={{ width: 32, height: 32, borderRadius: 6, background: form.customColorHex, border: '1px solid var(--border)', flexShrink: 0 }} />
                )}
              </div>
            </div>
          </div>

          <div className="form-group">
            <label className="form-label">توضیحات</label>
            <textarea className="form-input" rows={3} placeholder="توضیحات اختیاری..." value={form.description} onChange={set('description')} style={{ resize: 'vertical' }} />
          </div>

          <label style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 20, cursor: 'pointer', fontSize: 13, color: 'var(--text-secondary)' }}>
            <input type="checkbox" checked={form.isRecurring} onChange={setCheck('isRecurring')} />
            تکرارشونده
          </label>

          <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end' }}>
            <button type="button" className="btn btn-ghost" onClick={onClose}>انصراف</button>
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? <span className="spinner" style={{ width: 16, height: 16, borderWidth: 2 }} /> : <><Save size={15} /> ذخیره</>}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
