import { useState } from 'react'
import { tasksAPI } from '../../api/client'
import { Plus, Trash2, Edit2, CheckSquare, CheckCircle2, Circle, Save, X, Link as LinkIcon } from 'lucide-react'
import toast from 'react-hot-toast'
import styles from './ListView.module.css'

const PRIORITY_COLORS = { 1: '#f87171', 2: '#fb923c', 3: '#60a5fa' }
const PRIORITY_LABELS = { 1: 'بالا', 2: 'متوسط', 3: 'پایین' }

export default function TasksList({ weekId, tasks, blocks, onRefresh }) {
  const [adding, setAdding] = useState(false)
  const [newForm, setNewForm] = useState({ taskText: '', priority: 2 })
  const [editId, setEditId] = useState(null)
  const [editText, setEditText] = useState('')
  const [loading, setLoading] = useState(false)

  const handleAdd = async () => {
    if (!newForm.taskText.trim()) return
    setLoading(true)
    try {
      await tasksAPI.create({ weekId, taskText: newForm.taskText.trim(), priority: parseInt(newForm.priority), orderIndex: tasks.length })
      setNewForm({ taskText: '', priority: 2 })
      setAdding(false)
      onRefresh()
      toast.success('وظیفه اضافه شد')
    } catch { toast.error('خطا') } finally { setLoading(false) }
  }

  const handleToggle = async (task) => {
    try {
      await tasksAPI.toggle(task.id, !task.isDone)
      onRefresh()
    } catch { toast.error('خطا') }
  }

  const handleEdit = async (task) => {
    if (!editText.trim()) return
    try {
      await tasksAPI.update(task.id, { taskText: editText.trim() })
      setEditId(null)
      onRefresh()
    } catch { toast.error('خطا') }
  }

  const handleDelete = async (id) => {
    if (!confirm('حذف شود؟')) return
    try {
      await tasksAPI.delete(id)
      onRefresh()
      toast.success('حذف شد')
    } catch { toast.error('خطا') }
  }

  const done = tasks.filter(t => t.isDone).length

  return (
    <div className={styles.container}>
      <div className={styles.topBar}>
        <div>
          <h3 className={styles.sectionTitle}>وظایف هفته</h3>
          {tasks.length > 0 && (
            <p className={styles.progress}>{done} از {tasks.length} وظیفه انجام شده</p>
          )}
        </div>
        <button className="btn btn-primary" onClick={() => setAdding(true)}>
          <Plus size={15} /> وظیفه جدید
        </button>
      </div>

      {tasks.length > 0 && (
        <div className={styles.progressBar}>
          <div className={styles.progressFill} style={{ width: `${Math.round((done / tasks.length) * 100)}%` }} />
        </div>
      )}

      {adding && (
        <div className={styles.addRow}>
          <CheckSquare size={16} style={{ color: 'var(--accent)', flexShrink: 0 }} />
          <input
            className="form-input"
            style={{ flex: 1 }}
            placeholder="وظیفه جدید..."
            value={newForm.taskText}
            onChange={e => setNewForm(p => ({ ...p, taskText: e.target.value }))}
            onKeyDown={e => { if (e.key === 'Enter') handleAdd(); if (e.key === 'Escape') { setAdding(false) } }}
            autoFocus
          />
          <select className="form-input" style={{ width: 100 }} value={newForm.priority} onChange={e => setNewForm(p => ({ ...p, priority: e.target.value }))}>
            <option value={1}>بالا</option>
            <option value={2}>متوسط</option>
            <option value={3}>پایین</option>
          </select>
          <button className="btn btn-primary" style={{ padding: '6px 14px' }} onClick={handleAdd} disabled={loading}><Save size={14} /></button>
          <button className="btn btn-ghost" style={{ padding: '6px 14px' }} onClick={() => setAdding(false)}><X size={14} /></button>
        </div>
      )}

      <div className={styles.list}>
        {tasks.length === 0 && !adding && (
          <div className="empty-state">
            <CheckSquare size={40} />
            <p>هنوز وظیفه‌ای تعریف نشده</p>
          </div>
        )}
        {tasks.map((task, i) => {
          const linkedBlock = blocks.find(b => b.id === task.linkedBlockId)
          return (
            <div key={task.id} className={`${styles.item} ${task.isDone ? styles.done : ''}`} style={{ animationDelay: `${i * 0.03}s` }}>
              <div className={styles.taskPriority} style={{ background: PRIORITY_COLORS[task.priority] || '#888' }} />
              <button className={styles.toggleBtn} onClick={() => handleToggle(task)}>
                {task.isDone
                  ? <CheckCircle2 size={18} style={{ color: 'var(--success)' }} />
                  : <Circle size={18} style={{ color: 'var(--text-muted)' }} />
                }
              </button>

              {editId === task.id ? (
                <input
                  className="form-input"
                  style={{ flex: 1, padding: '4px 10px' }}
                  value={editText}
                  onChange={e => setEditText(e.target.value)}
                  onKeyDown={e => { if (e.key === 'Enter') handleEdit(task); if (e.key === 'Escape') setEditId(null) }}
                  autoFocus
                />
              ) : (
                <div style={{ flex: 1 }}>
                  <span className={styles.itemText}>{task.taskText}</span>
                  <div className={styles.itemMeta}>
                    <span className={styles.taskBadge} style={{ color: PRIORITY_COLORS[task.priority] }}>
                      {PRIORITY_LABELS[task.priority]}
                    </span>
                    {task.dueDate && <span>{task.dueDate}</span>}
                    {linkedBlock && (
                      <span style={{ display: 'flex', alignItems: 'center', gap: 3 }}>
                        <LinkIcon size={10} /> {linkedBlock.activityTitle}
                      </span>
                    )}
                  </div>
                </div>
              )}

              <div className={styles.itemActions}>
                {editId === task.id ? (
                  <>
                    <button className="btn-icon" onClick={() => handleEdit(task)}><Save size={13} /></button>
                    <button className="btn-icon" onClick={() => setEditId(null)}><X size={13} /></button>
                  </>
                ) : (
                  <>
                    <button className="btn-icon" onClick={() => { setEditId(task.id); setEditText(task.taskText) }}><Edit2 size={13} /></button>
                    <button className="btn-icon" onClick={() => handleDelete(task.id)}><Trash2 size={13} /></button>
                  </>
                )}
              </div>
            </div>
          )
        })}
      </div>
    </div>
  )
}
