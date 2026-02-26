import { useState } from 'react'
import { notesAPI } from '../../api/client'
import { Plus, Trash2, Edit2, FileText, Save, X } from 'lucide-react'
import toast from 'react-hot-toast'
import styles from './ListView.module.css'
import noteStyles from './NotesList.module.css'

export default function NotesList({ weekId, notes, categories, onRefresh }) {
  const [adding, setAdding] = useState(false)
  const [newForm, setNewForm] = useState({ noteText: '', categoryId: '' })
  const [editId, setEditId] = useState(null)
  const [editText, setEditText] = useState('')
  const [loading, setLoading] = useState(false)

  const handleAdd = async () => {
    if (!newForm.noteText.trim()) return
    setLoading(true)
    try {
      await notesAPI.create({
        weekId,
        noteText: newForm.noteText.trim(),
        categoryId: newForm.categoryId ? parseInt(newForm.categoryId) : null,
        orderIndex: notes.length,
      })
      setNewForm({ noteText: '', categoryId: '' })
      setAdding(false)
      onRefresh()
      toast.success('یادداشت اضافه شد')
    } catch { toast.error('خطا') } finally { setLoading(false) }
  }

  const handleEdit = async (note) => {
    try {
      await notesAPI.update(note.id, { noteText: editText.trim() })
      setEditId(null)
      onRefresh()
    } catch { toast.error('خطا') }
  }

  const handleDelete = async (id) => {
    if (!confirm('حذف شود؟')) return
    try {
      await notesAPI.delete(id)
      onRefresh()
      toast.success('حذف شد')
    } catch { toast.error('خطا') }
  }

  return (
    <div className={styles.container}>
      <div className={styles.topBar}>
        <h3 className={styles.sectionTitle}>یادداشت‌های هفته</h3>
        <button className="btn btn-primary" onClick={() => setAdding(true)}>
          <Plus size={15} /> یادداشت جدید
        </button>
      </div>

      {adding && (
        <div className={noteStyles.addBox}>
          <div style={{ display: 'flex', gap: 10, marginBottom: 10 }}>
            <FileText size={16} style={{ color: 'var(--accent)', flexShrink: 0, marginTop: 2 }} />
            <select className="form-input" style={{ width: 160 }} value={newForm.categoryId} onChange={e => setNewForm(p => ({ ...p, categoryId: e.target.value }))}>
              <option value="">بدون دسته</option>
              {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
          <textarea
            className="form-input"
            rows={4}
            placeholder="متن یادداشت..."
            value={newForm.noteText}
            onChange={e => setNewForm(p => ({ ...p, noteText: e.target.value }))}
            autoFocus
            style={{ resize: 'vertical', marginBottom: 10 }}
          />
          <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
            <button className="btn btn-ghost" style={{ padding: '6px 14px' }} onClick={() => setAdding(false)}><X size={14} /> انصراف</button>
            <button className="btn btn-primary" style={{ padding: '6px 14px' }} onClick={handleAdd} disabled={loading}><Save size={14} /> ذخیره</button>
          </div>
        </div>
      )}

      <div className={noteStyles.notesGrid}>
        {notes.length === 0 && !adding && (
          <div className="empty-state" style={{ gridColumn: '1/-1' }}>
            <FileText size={40} />
            <p>هنوز یادداشتی ثبت نشده</p>
          </div>
        )}
        {notes.map((note, i) => {
          const cat = categories.find(c => c.id === note.categoryId)
          return (
            <div key={note.id} className={noteStyles.noteCard} style={{ animationDelay: `${i * 0.04}s` }}>
              {cat && (
                <span className={noteStyles.noteCat}>{cat.name}</span>
              )}
              {editId === note.id ? (
                <div>
                  <textarea
                    className="form-input"
                    rows={4}
                    value={editText}
                    onChange={e => setEditText(e.target.value)}
                    autoFocus
                    style={{ resize: 'vertical', marginBottom: 8, width: '100%' }}
                  />
                  <div style={{ display: 'flex', gap: 6 }}>
                    <button className="btn btn-primary" style={{ padding: '4px 12px', fontSize: 12 }} onClick={() => handleEdit(note)}><Save size={12} /></button>
                    <button className="btn btn-ghost" style={{ padding: '4px 12px', fontSize: 12 }} onClick={() => setEditId(null)}><X size={12} /></button>
                  </div>
                </div>
              ) : (
                <p className={noteStyles.noteText}>{note.noteText}</p>
              )}
              <div className={noteStyles.noteActions}>
                <button className="btn-icon" onClick={() => { setEditId(note.id); setEditText(note.noteText) }}><Edit2 size={13} /></button>
                <button className="btn-icon" onClick={() => handleDelete(note.id)}><Trash2 size={13} /></button>
              </div>
            </div>
          )
        })}
      </div>
    </div>
  )
}
