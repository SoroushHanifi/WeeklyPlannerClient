import { useState, useMemo } from 'react'
import { timeBlocksAPI } from '../../api/client'
import { Edit2, CheckCircle2, Circle, X, Save } from 'lucide-react'
import toast from 'react-hot-toast'
import styles from './TimeBlocksView.module.css'

const HOURS = Array.from({ length: 18 }, (_, i) => i + 6) // 6..23

const DAYS = [
  { id: 1, name: 'شنبه' },
  { id: 2, name: 'یکشنبه' },
  { id: 3, name: 'دوشنبه' },
  { id: 4, name: 'سه‌شنبه' },
  { id: 5, name: 'چهارشنبه' },
  { id: 6, name: 'پنج‌شنبه' },
  { id: 7, name: 'جمعه' },
]

const PRIORITY_COLOR = {
  1: '#f87171',
  2: '#f5c842',
  3: '#60a5fa',
}

// عنوان پیش‌فرض که backend ساخته
const isDefaultTitle = (t) => !t || /^\d{2}:\d{2} - \d{2}:\d{2}$/.test(t)

export default function TimeBlocksView({ weekId, blocks, days, categories, onRefresh }) {
  const [editing, setEditing] = useState(null) // { id, title, description, priority, categoryId }
  const [saving, setSaving] = useState(false)

  // ایندکس سریع: map[dayId][hour] = block
  const blockMap = useMemo(() => {
    const map = {}
    DAYS.forEach(d => { map[d.id] = {} })
    ;(blocks || []).forEach(b => {
      const hour = parseInt((b.startTime || '00:00').slice(0, 2))
      if (map[b.dayId]) map[b.dayId][hour] = b
    })
    return map
  }, [blocks])

  // نام روزها از API یا fallback
  const activeDays = useMemo(() => {
    if (!days || days.length === 0) return DAYS
    return DAYS.map(d => {
      const a = days.find(x => (x.id ?? x.dayId) === d.id)
      return { id: d.id, name: a?.dayNameFa ?? a?.nameFa ?? d.name }
    })
  }, [days])

  const handleToggle = async (block) => {
    try {
      await timeBlocksAPI.complete(block.timeBlockId, !block.isCompleted)
      onRefresh()
    } catch { toast.error('خطا') }
  }

  const startEdit = (block) => {
    setEditing({
      id: block.timeBlockId,
      title: isDefaultTitle(block.activityTitle) ? '' : (block.activityTitle || ''),
      description: block.description || '',
      priority: block.priority ?? 2,
      categoryId: block.categoryId ?? '',
    })
  }

  const saveEdit = async (block) => {
    if (!editing) return
    setSaving(true)
    try {
      await timeBlocksAPI.update(block.timeBlockId, {
        activityTitle: editing.title || `${block.startTime?.slice(0,5)} - ${block.endTime?.slice(0,5)}`,
        description: editing.description || null,
        priority: parseInt(editing.priority),
        categoryId: editing.categoryId ? parseInt(editing.categoryId) : null,
      })
      toast.success('ذخیره شد')
      setEditing(null)
      onRefresh()
    } catch { toast.error('خطا در ذخیره') }
    finally { setSaving(false) }
  }

  return (
    <div className={styles.outer}>
      <div className={styles.grid} style={{ '--cols': activeDays.length }}>

        {/* ── Corner ── */}
        <div className={styles.corner} />

        {/* ── Day headers ── */}
        {activeDays.map(d => (
          <div key={d.id} className={styles.dayHead}>{d.name}</div>
        ))}

        {/* ── Hour rows ── */}
        {HOURS.map(hour => (
          <>
            <div key={`lbl-${hour}`} className={styles.hourLbl}>
              {String(hour).padStart(2, '0')}:۰۰
            </div>

            {activeDays.map(day => {
              const block = blockMap[day.id]?.[hour]
              if (!block) return <div key={`e-${day.id}-${hour}`} className={styles.cellEmpty} />

              const isEdit = editing?.id === block.timeBlockId
              const accent = block.displayColor
              const filled = !isDefaultTitle(block.activityTitle)
              const cat = (categories || []).find(c => (c.id ?? c.categoryId) === block.categoryId)

              return (
                <div
                  key={`b-${block.timeBlockId}`}
                  className={`${styles.cell} ${filled ? styles.filled : styles.unfilled} ${block.isCompleted ? styles.done : ''}`}
                  style={{ '--accent': accent }}
                >
                  {isEdit ? (
                    <div className={styles.editBox} onClick={e => e.stopPropagation()}>
                      <input
                        className={styles.inp}
                        placeholder="عنوان فعالیت..."
                        value={editing.title}
                        autoFocus
                        onChange={e => setEditing(p => ({ ...p, title: e.target.value }))}
                        onKeyDown={e => {
                          if (e.key === 'Enter') saveEdit(block)
                          if (e.key === 'Escape') setEditing(null)
                        }}
                      />
                      <input
                        className={styles.inp}
                        placeholder="توضیحات (اختیاری)"
                        value={editing.description}
                        onChange={e => setEditing(p => ({ ...p, description: e.target.value }))}
                      />
                      <div className={styles.editRow}>
                        <select className={styles.sel} value={editing.priority}
                          onChange={e => setEditing(p => ({ ...p, priority: e.target.value }))}>
                          <option value={1}>🔴 بالا</option>
                          <option value={2}>🟡 متوسط</option>
                          <option value={3}>🔵 پایین</option>
                        </select>
                        {categories?.length > 0 && (
                          <select className={styles.sel} value={editing.categoryId}
                            onChange={e => setEditing(p => ({ ...p, categoryId: e.target.value }))}>
                            <option value="">دسته‌بندی</option>
                            {categories.map(c => (
                              <option key={c.id ?? c.categoryId} value={c.id ?? c.categoryId}>
                                {c.name ?? c.categoryName}
                              </option>
                            ))}
                          </select>
                        )}
                      </div>
                      <div className={styles.editBtns}>
                        <button className={styles.btnSave} onClick={() => saveEdit(block)} disabled={saving}>
                          {saving ? '...' : <><Save size={11} /> ذخیره</>}
                        </button>
                        <button className={styles.btnCancel} onClick={() => setEditing(null)}>
                          <X size={11} /> لغو
                        </button>
                      </div>
                    </div>
                  ) : (
                    <div className={styles.view} onClick={() => startEdit(block)}>
                      <button className={styles.check} onClick={e => { e.stopPropagation(); handleToggle(block) }}>
                        {block.isCompleted
                          ? <CheckCircle2 size={13} color="#4ade80" />
                          : <Circle size={13} color="var(--text-muted)" />
                        }
                      </button>
                      <div className={styles.viewText}>
                        {filled ? (
                          <>
                            <span className={styles.title}>{block.activityTitle}</span>
                            {block.description && <span className={styles.desc}>{block.description}</span>}
                            {cat && <span className={styles.catTag}>{cat.name ?? cat.categoryName}</span>}
                          </>
                        ) : (
                          <span className={styles.hint}>+ اضافه کن</span>
                        )}
                      </div>
                      <Edit2 size={10} className={styles.editIcon} />
                    </div>
                  )}
                </div>
              )
            })}
          </>
        ))}
      </div>
    </div>
  )
}
