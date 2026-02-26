import { useState, useEffect } from 'react'
import { weeksAPI } from '../../api/client'
import { X, Copy } from 'lucide-react'
import toast from 'react-hot-toast'

export default function CopyWeekModal({ sourceWeekId, onClose, onCopied }) {
  const [weeks, setWeeks] = useState([])
  const [form, setForm] = useState({ targetWeekId: '', copyBlocks: true, copyTasks: true, copyGoals: true })
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    weeksAPI.getSummaries().then(r => setWeeks((r.data || []).filter(w => w.id !== sourceWeekId))).catch(() => {})
  }, [sourceWeekId])

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!form.targetWeekId) { toast.error('هفته مقصد را انتخاب کنید'); return }
    setLoading(true)
    try {
      await weeksAPI.copy({
        sourceWeekId,
        targetWeekId: parseInt(form.targetWeekId),
        copyBlocks: form.copyBlocks,
        copyTasks: form.copyTasks,
        copyGoals: form.copyGoals,
      })
      toast.success('کپی انجام شد')
      onCopied()
    } catch { toast.error('خطا در کپی') } finally { setLoading(false) }
  }

  const setCheck = (k) => (e) => setForm(p => ({ ...p, [k]: e.target.checked }))

  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="modal-box" style={{ maxWidth: 420 }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 24 }}>
          <h2 style={{ fontSize: 17, fontWeight: 700, display: 'flex', alignItems: 'center', gap: 8 }}>
            <Copy size={18} /> کپی هفته
          </h2>
          <button className="btn-icon" onClick={onClose}><X size={18} /></button>
        </div>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">هفته مقصد</label>
            <select className="form-input" value={form.targetWeekId} onChange={e => setForm(p => ({ ...p, targetWeekId: e.target.value }))}>
              <option value="">انتخاب کنید...</option>
              {weeks.map(w => <option key={w.id} value={w.id}>{w.title || `هفته ${w.id}`}</option>)}
            </select>
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 12, marginBottom: 24 }}>
            {[['copyBlocks', 'کپی بلوک‌های زمانی'], ['copyTasks', 'کپی وظایف'], ['copyGoals', 'کپی اهداف']].map(([k, label]) => (
              <label key={k} style={{ display: 'flex', alignItems: 'center', gap: 8, cursor: 'pointer', fontSize: 13, color: 'var(--text-secondary)' }}>
                <input type="checkbox" checked={form[k]} onChange={setCheck(k)} />
                {label}
              </label>
            ))}
          </div>
          <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end' }}>
            <button type="button" className="btn btn-ghost" onClick={onClose}>انصراف</button>
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? <span className="spinner" style={{ width: 16, height: 16, borderWidth: 2 }} /> : <><Copy size={15} /> کپی</>}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
