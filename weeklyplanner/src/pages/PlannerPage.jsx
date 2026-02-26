import { useState, useEffect } from 'react'
import { useParams, useOutletContext } from 'react-router-dom'
import { weeksAPI, timeBlocksAPI, goalsAPI, tasksAPI, notesAPI, lookupAPI } from '../api/client'
import { CalendarDays, Target, CheckSquare, FileText } from 'lucide-react'
import toast from 'react-hot-toast'
import TimeBlocksView from '../components/planner/TimeBlocksView'
import GoalsList from '../components/planner/GoalsList'
import TasksList from '../components/planner/TasksList'
import NotesList from '../components/planner/NotesList'
import WeekHeader from '../components/planner/WeekHeader'
import styles from './PlannerPage.module.css'

// پاسخ API همیشه { data: [...] } یا مستقیم [...] برمی‌گردونه
function toArray(res) {
  const d = res?.data
  if (!d) return []
  if (Array.isArray(d)) return d
  // { data: [...] }
  if (Array.isArray(d.data)) return d.data
  // { items: [...] } etc
  for (const k of ['items', 'results', 'list', 'value']) {
    if (Array.isArray(d[k])) return d[k]
  }
  return []
}

function toObj(res) {
  const d = res?.data
  if (!d) return null
  // { data: {...} }
  if (d.data && typeof d.data === 'object' && !Array.isArray(d.data)) return d.data
  // { id: ... } مستقیم
  if (d.id || d.weekId) return d
  return d
}

const TABS = [
  { id: 'schedule', label: 'برنامه زمانی', icon: CalendarDays },
  { id: 'goals', label: 'اهداف', icon: Target },
  { id: 'tasks', label: 'وظایف', icon: CheckSquare },
  { id: 'notes', label: 'یادداشت‌ها', icon: FileText },
]

export default function PlannerPage() {
  const { weekId } = useParams()
  const ctx = useOutletContext()

  const [week, setWeek] = useState(null)
  const [blocks, setBlocks] = useState([])
  const [goals, setGoals] = useState([])
  const [tasks, setTasks] = useState([])
  const [notes, setNotes] = useState([])
  const [days, setDays] = useState([])
  const [categories, setCategories] = useState([])
  const [loading, setLoading] = useState(false)
  const [tab, setTab] = useState('schedule')

  useEffect(() => {
    if (weekId) loadAll(parseInt(weekId))
  }, [weekId])

  useEffect(() => {
    loadLookups()
  }, [])

  const loadLookups = async () => {
    try {
      const [dr, cr] = await Promise.all([lookupAPI.getDays(), lookupAPI.getCategories()])
      setDays(toArray(dr))
      setCategories(toArray(cr))
    } catch {}
  }

  const loadAll = async (id) => {
    setLoading(true)
    try {
      const [wr, br, gr, tr, nr] = await Promise.all([
        weeksAPI.getById(id),
        timeBlocksAPI.getByWeek(id),
        goalsAPI.getByWeek(id),
        tasksAPI.getByWeek(id),
        notesAPI.getByWeek(id),
      ])
      console.log('[blocks raw]', br.data)
      setWeek(toObj(wr))
      setBlocks(toArray(br))
      setGoals(toArray(gr))
      setTasks(toArray(tr))
      setNotes(toArray(nr))
    } catch (e) {
      console.error(e)
      toast.error('خطا در بارگذاری')
    } finally {
      setLoading(false)
    }
  }

  const refresh = () => weekId && loadAll(parseInt(weekId))

  if (!weekId) return (
    <div style={{ display:'flex', flexDirection:'column', alignItems:'center', justifyContent:'center', height:'100%', gap:12, color:'var(--text-muted)' }}>
      <CalendarDays size={48} opacity={0.3} />
      <p>یک هفته از منوی کناری انتخاب کنید</p>
    </div>
  )

  if (loading) return (
    <div style={{ display:'flex', alignItems:'center', justifyContent:'center', height:'100%' }}>
      <div className="spinner" />
    </div>
  )

  return (
    <div className={styles.page}>
      {week && (
        <WeekHeader
          week={week}
          onUpdate={setWeek}
          onRefresh={refresh}
          onWeeksChange={ctx?.loadWeeks}
        />
      )}

      <div className={styles.tabs}>
        {TABS.map(t => (
          <button
            key={t.id}
            className={`${styles.tab} ${tab === t.id ? styles.tabActive : ''}`}
            onClick={() => setTab(t.id)}
          >
            <t.icon size={15} />
            <span>{t.label}</span>
            {t.id === 'tasks' && tasks.filter(x => !x.isDone).length > 0 && (
              <span className={styles.badge}>{tasks.filter(x => !x.isDone).length}</span>
            )}
            {t.id === 'goals' && goals.length > 0 && (
              <span className={styles.badge}>{goals.length}</span>
            )}
          </button>
        ))}
      </div>

      <div className={styles.body}>
        {tab === 'schedule' && (
          <TimeBlocksView
            weekId={parseInt(weekId)}
            blocks={blocks}
            days={days}
            categories={categories}
            onRefresh={refresh}
          />
        )}
        {tab === 'goals' && <GoalsList weekId={parseInt(weekId)} goals={goals} onRefresh={refresh} />}
        {tab === 'tasks' && <TasksList weekId={parseInt(weekId)} tasks={tasks} blocks={blocks} onRefresh={refresh} />}
        {tab === 'notes' && <NotesList weekId={parseInt(weekId)} notes={notes} categories={categories} onRefresh={refresh} />}
      </div>
    </div>
  )
}
