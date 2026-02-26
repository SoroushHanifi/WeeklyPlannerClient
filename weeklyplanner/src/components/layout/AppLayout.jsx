import { useState, useEffect } from 'react'
import { Outlet, NavLink, useNavigate, useLocation } from 'react-router-dom'
import { useAuth } from '../../contexts/AuthContext'
import { weeksAPI } from '../../api/client'
import { CalendarDays, ChevronLeft, ChevronRight, Plus, LogOut, User, LayoutGrid } from 'lucide-react'
import toast from 'react-hot-toast'
import CreateWeekModal from '../planner/CreateWeekModal'
import styles from './AppLayout.module.css'

function toArray(data) {
  if (!data) return []
  if (Array.isArray(data)) return data
  const keys = ['items', 'data', 'weeks', 'results', 'list', 'value']
  for (const k of keys) {
    if (Array.isArray(data[k])) return data[k]
  }
  return []
}

// استخراج ID عددی از هر ساختار ممکن
function extractId(data) {
  if (!data) return null
  const raw = data?.id ?? data?.weekId ?? data?.data?.id ?? data?.data?.weekId
  const num = parseInt(raw)
  return isNaN(num) ? null : num
}

export default function AppLayout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [weeks, setWeeks] = useState([])
  const [currentWeekId, setCurrentWeekId] = useState(null)
  const [sidebarOpen, setSidebarOpen] = useState(true)
  const [showCreateWeek, setShowCreateWeek] = useState(false)

  useEffect(() => {
    if (!user) return;
    loadWeeks()
  }, [])

  const loadWeeks = async () => {
    // بارگذاری لیست هفته‌ها
    try {
      const res = await weeksAPI.getSummaries()
      const arr = toArray(res.data)
      setWeeks(arr)
    } catch {
      try {
        const res2 = await weeksAPI.getAll()
        setWeeks(toArray(res2.data))
      } catch {}
    }

    // هفته جاری رو فقط اگه روی صفحه اصلی هستیم load کن
    // یعنی نه /profile و نه /week/xxx از قبل
    const isRootPath = location.pathname === '/'
    if (!isRootPath) return

    try {
      const cur = await weeksAPI.getCurrent()
      const id = extractId(cur.data)
      if (id) {
        setCurrentWeekId(id)
        navigate(`/week/${id}`, { replace: true })
      }
    } catch {
      // هفته جاری نیست، اشکالی نداره
    }
  }

  const handleLogout = async () => {
    await logout()
    toast.success('خارج شدید')
    navigate('/login')
  }

  const handleWeekCreated = (weekData) => {
    const w = weekData?.data || weekData
    const id = extractId(w)
    if (!id) return
    setWeeks(p => [w, ...p])
    setCurrentWeekId(id)
    navigate(`/week/${id}`)
    setShowCreateWeek(false)
  }

  // هایلایت کردن هفته فعال از URL
  const activeWeekIdFromUrl = (() => {
    const m = location.pathname.match(/\/week\/(\d+)/)
    return m ? parseInt(m[1]) : null
  })()

  return (
    <div className={styles.layout}>
      <aside className={`${styles.sidebar} ${sidebarOpen ? styles.open : styles.closed}`}>
        <div className={styles.sidebarHeader}>
          <div className={styles.brandIcon}><CalendarDays size={20} /></div>
          {sidebarOpen && <span className={styles.brandName}>برنامه‌ریز</span>}
          <button className={`btn-icon ${styles.toggleBtn}`} onClick={() => setSidebarOpen(p => !p)}>
            {sidebarOpen ? <ChevronRight size={16} /> : <ChevronLeft size={16} />}
          </button>
        </div>

        {sidebarOpen && (
          <>
            <nav className={styles.nav}>
              <NavLink to="/" end className={({ isActive }) => `${styles.navItem} ${isActive ? styles.active : ''}`}>
                <LayoutGrid size={16} /><span>داشبورد</span>
              </NavLink>
              <NavLink to="/profile" className={({ isActive }) => `${styles.navItem} ${isActive ? styles.active : ''}`}>
                <User size={16} /><span>پروفایل</span>
              </NavLink>
            </nav>

            <div className={styles.weeksSection}>
              <div className={styles.weeksSectionHeader}>
                <span>هفته‌ها</span>
                <button className="btn-icon" onClick={() => setShowCreateWeek(true)} title="هفته جدید">
                  <Plus size={16} />
                </button>
              </div>
              <div className={styles.weeksList}>
                {weeks.length === 0 && (
                  <p className={styles.emptyWeeks}>هنوز هفته‌ای نیست</p>
                )}
                {weeks.map(w => {
                  const wid = extractId(w)
                  if (!wid) return null
                  return (
                    <button
                      key={wid}
                      className={`${styles.weekItem} ${wid === activeWeekIdFromUrl ? styles.weekActive : ''}`}
                      onClick={() => { setCurrentWeekId(wid); navigate(`/week/${wid}`) }}
                    >
                      <span className={styles.weekTitle}>{w.title || `هفته ${wid}`}</span>
                      {w.startDateShamsi && <span className={styles.weekDate}>{w.startDateShamsi}</span>}
                    </button>
                  )
                })}
              </div>
            </div>
          </>
        )}

        <div className={styles.sidebarFooter}>
          {sidebarOpen ? (
            <div className={styles.userRow}>
              <div className={styles.avatar}>
                {user?.fullName?.[0] || user?.username?.[0] || 'U'}
              </div>
              <div className={styles.userInfo}>
                <span className={styles.userName}>{user?.fullName || user?.username}</span>
                <span className={styles.userSub}>{user?.email || ''}</span>
              </div>
              <button className="btn-icon" onClick={handleLogout} title="خروج">
                <LogOut size={16} />
              </button>
            </div>
          ) : (
            <button className="btn-icon" onClick={handleLogout} style={{ width: '100%' }}>
              <LogOut size={16} />
            </button>
          )}
        </div>
      </aside>

      <main className={styles.main}>
        <Outlet context={{ weeks, loadWeeks, currentWeekId, setCurrentWeekId }} />
      </main>

      {showCreateWeek && (
        <CreateWeekModal
          onClose={() => setShowCreateWeek(false)}
          onCreated={handleWeekCreated}
        />
      )}
    </div>
  )
}
