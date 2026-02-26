import { useState } from 'react'
import { weeksAPI } from '../../api/client'
import { Calendar, Edit2, Trash2, Copy, RefreshCw, Check, X } from 'lucide-react'
import toast from 'react-hot-toast'
import { useNavigate } from 'react-router-dom'
import CopyWeekModal from './CopyWeekModal'
import styles from './WeekHeader.module.css'

export default function WeekHeader({ week, onUpdate, onRefresh, onWeeksChange }) {
  const navigate = useNavigate()
  const [editing, setEditing] = useState(false)
  const [title, setTitle] = useState(week?.title || '')
  const [showCopy, setShowCopy] = useState(false)
  const [deleting, setDeleting] = useState(false)

  const saveTitle = async () => {
    try {
      const res = await weeksAPI.update(week.id, { title })
      onUpdate?.(res.data || { ...week, title })
      setEditing(false)
      toast.success('عنوان ذخیره شد')
    } catch {
      toast.error('خطا در ذخیره')
    }
  }

  const handleDelete = async () => {
    if (!confirm('آیا از حذف این هفته مطمئن هستید؟')) return
    setDeleting(true)
    try {
      await weeksAPI.delete(week.id)
      toast.success('هفته حذف شد')
      onWeeksChange?.()
      navigate('/')
    } catch {
      toast.error('خطا در حذف')
    } finally {
      setDeleting(false)
    }
  }

  const formatDate = (dateStr) => {
    if (!dateStr) return ''
    return dateStr
  }

  return (
    <div className={styles.header}>
      <div className={styles.titleRow}>
        <div className={styles.weekIcon}>
          <Calendar size={18} />
        </div>

        {editing ? (
          <div className={styles.editRow}>
            <input
              className={`form-input ${styles.titleInput}`}
              value={title}
              onChange={e => setTitle(e.target.value)}
              autoFocus
              onKeyDown={e => { if (e.key === 'Enter') saveTitle(); if (e.key === 'Escape') setEditing(false) }}
            />
            <button className="btn-icon" onClick={saveTitle}><Check size={15} /></button>
            <button className="btn-icon" onClick={() => setEditing(false)}><X size={15} /></button>
          </div>
        ) : (
          <div className={styles.titleGroup}>
            <h1 className={styles.title}>{week?.title || `هفته ${week?.id}`}</h1>
            {(week?.startDateShamsi || week?.startDate) && (
              <span className={styles.dateRange}>
                {week.startDateShamsi || week.startDate}
                {week.endDateShamsi || week.endDate ? ` تا ${week.endDateShamsi || week.endDate}` : ''}
              </span>
            )}
          </div>
        )}

        <div className={styles.actions}>
          <button className="btn-icon" onClick={() => { setTitle(week?.title || ''); setEditing(true) }} title="ویرایش عنوان">
            <Edit2 size={15} />
          </button>
          <button className="btn-icon" onClick={onRefresh} title="بارگذاری مجدد">
            <RefreshCw size={15} />
          </button>
          <button className="btn-icon" onClick={() => setShowCopy(true)} title="کپی هفته">
            <Copy size={15} />
          </button>
          <button className={`btn-icon ${styles.deleteBtn}`} onClick={handleDelete} disabled={deleting} title="حذف هفته">
            <Trash2 size={15} />
          </button>
        </div>
      </div>

      {showCopy && (
        <CopyWeekModal
          sourceWeekId={week.id}
          onClose={() => setShowCopy(false)}
          onCopied={() => { setShowCopy(false); onWeeksChange?.() }}
        />
      )}
    </div>
  )
}
