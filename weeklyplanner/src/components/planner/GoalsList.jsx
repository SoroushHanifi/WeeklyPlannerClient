import { useState } from 'react'
import { goalsAPI } from '../../api/client'
import { Plus, Trash2, Edit2, Target, CheckCircle2, Circle, Save, X } from 'lucide-react'
import toast from 'react-hot-toast'
import styles from './ListView.module.css'

export default function GoalsList({ weekId, goals, onRefresh }) {
  const [adding, setAdding] = useState(false)
  const [newText, setNewText] = useState('')
  const [editId, setEditId] = useState(null)
  const [editText, setEditText] = useState('')
  const [loading, setLoading] = useState(false)

  const handleAdd = async () => {
    if (!newText.trim()) return
    setLoading(true)
    try {
      await goalsAPI.create({ weekId, goalText: newText.trim(), orderIndex: goals.length, weight: 1 })
      setNewText('')
      setAdding(false)
      onRefresh()
      toast.success('هدف اضافه شد')
    } catch { toast.error('خطا') } finally { setLoading(false) }
  }

  const handleToggle = async (goal) => {
    try {
      await goalsAPI.toggle(goal.id, !goal.isAchieved)
      onRefresh()
    } catch { toast.error('خطا') }
  }

  const handleEdit = async (goal) => {
    if (!editText.trim()) return
    try {
      await goalsAPI.update(goal.id, { goalText: editText.trim() })
      setEditId(null)
      onRefresh()
      toast.success('ویرایش شد')
    } catch { toast.error('خطا') }
  }

  const handleDelete = async (id) => {
    if (!confirm('حذف شود؟')) return
    try {
      await goalsAPI.delete(id)
      onRefresh()
      toast.success('حذف شد')
    } catch { toast.error('خطا') }
  }

  const achieved = goals.filter(g => g.isAchieved).length
  const percent = goals.length > 0 ? Math.round((achieved / goals.length) * 100) : 0

  return (
    <div className={styles.container}>
      <div className={styles.topBar}>
        <div>
          <h3 className={styles.sectionTitle}>اهداف هفته</h3>
          {goals.length > 0 && (
            <p className={styles.progress}>{achieved} از {goals.length} هدف محقق شده ({percent}%)</p>
          )}
        </div>
        <button className="btn btn-primary" onClick={() => setAdding(true)}>
          <Plus size={15} /> هدف جدید
        </button>
      </div>

      {goals.length > 0 && (
        <div className={styles.progressBar}>
          <div className={styles.progressFill} style={{ width: `${percent}%` }} />
        </div>
      )}

      {adding && (
        <div className={styles.addRow}>
          <Target size={16} style={{ color: 'var(--accent)', flexShrink: 0 }} />
          <input
            className="form-input"
            style={{ flex: 1 }}
            placeholder="هدف جدید..."
            value={newText}
            onChange={e => setNewText(e.target.value)}
            onKeyDown={e => { if (e.key === 'Enter') handleAdd(); if (e.key === 'Escape') { setAdding(false); setNewText('') } }}
            autoFocus
          />
          <button className="btn btn-primary" style={{ padding: '6px 14px' }} onClick={handleAdd} disabled={loading}>
            <Save size={14} />
          </button>
          <button className="btn btn-ghost" style={{ padding: '6px 14px' }} onClick={() => { setAdding(false); setNewText('') }}>
            <X size={14} />
          </button>
        </div>
      )}

      <div className={styles.list}>
        {goals.length === 0 && !adding && (
          <div className="empty-state">
            <Target size={40} />
            <p>هنوز هدفی تعریف نشده</p>
          </div>
        )}
        {goals.map((goal, i) => (
          <div key={goal.id} className={`${styles.item} ${goal.isAchieved ? styles.done : ''}`} style={{ animationDelay: `${i * 0.04}s` }}>
            <button className={styles.toggleBtn} onClick={() => handleToggle(goal)}>
              {goal.isAchieved
                ? <CheckCircle2 size={18} style={{ color: 'var(--success)' }} />
                : <Circle size={18} style={{ color: 'var(--text-muted)' }} />
              }
            </button>

            {editId === goal.id ? (
              <input
                className="form-input"
                style={{ flex: 1, padding: '4px 10px' }}
                value={editText}
                onChange={e => setEditText(e.target.value)}
                onKeyDown={e => { if (e.key === 'Enter') handleEdit(goal); if (e.key === 'Escape') setEditId(null) }}
                autoFocus
              />
            ) : (
              <span className={styles.itemText}>{goal.goalText}</span>
            )}

            <div className={styles.itemActions}>
              {editId === goal.id ? (
                <>
                  <button className="btn-icon" onClick={() => handleEdit(goal)}><Save size={13} /></button>
                  <button className="btn-icon" onClick={() => setEditId(null)}><X size={13} /></button>
                </>
              ) : (
                <>
                  <button className="btn-icon" onClick={() => { setEditId(goal.id); setEditText(goal.goalText) }}><Edit2 size={13} /></button>
                  <button className="btn-icon" onClick={() => handleDelete(goal.id)}><Trash2 size={13} /></button>
                </>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
