import React from 'react'
import { isToday, isYesterday, parseISO } from 'date-fns'
import { useMosaicDashboard } from '../../context/MosaicDashboardContext'

function ActivityIcon({ type }) {
  const t = type === 1 ? 'in' : 'out'
  if (t === 'in') {
    return (
      <div className="w-9 h-9 rounded-full shrink-0 bg-green-500 my-2 mr-3 flex items-center justify-center">
        <svg className="w-5 h-5 fill-current text-white" viewBox="0 0 24 24">
          <path d="M12 4l8 8h-5v8h-6v-8H4l8-8z" />
        </svg>
      </div>
    )
  }
  return (
    <div className="w-9 h-9 rounded-full shrink-0 bg-red-500 my-2 mr-3 flex items-center justify-center">
      <svg className="w-5 h-5 fill-current text-white" viewBox="0 0 24 24">
        <path d="M12 20l-8-8h5V4h6v8h5l-8 8z" />
      </svg>
    </div>
  )
}

function groupLabel(d) {
  try {
    const dt = typeof d === 'string' ? parseISO(d) : d
    if (isToday(dt)) return 'Today'
    if (isYesterday(dt)) return 'Yesterday'
    return 'Earlier'
  } catch {
    return 'Earlier'
  }
}

function DashboardCard12() {
  const { data: stats } = useMosaicDashboard()
  if (!stats) return null

  const raw = [...(stats.recentStockMovements ?? [])].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
  )
  const today = raw.filter((m) => groupLabel(m.createdAt) === 'Today')
  const yesterday = raw.filter((m) => groupLabel(m.createdAt) === 'Yesterday')
  const earlier = raw.filter((m) => groupLabel(m.createdAt) === 'Earlier')

  function renderGroup(title, items) {
    if (!items.length) return null
    return (
      <div>
        <header className="text-xs uppercase text-gray-400 dark:text-gray-500 bg-gray-50 dark:bg-gray-700/50 rounded-xs font-semibold p-2">
          {title}
        </header>
        <ul className="my-1">
          {items.map((m) => (
            <li key={m.id} className="flex px-2">
              <ActivityIcon type={m.type} />
              <div className="grow flex items-center border-b border-gray-100 dark:border-gray-700/60 text-sm py-2">
                <div className="grow flex justify-between">
                  <div className="self-center text-gray-700 dark:text-gray-300">
                    <span className="font-medium text-gray-800 dark:text-gray-100">{m.productName}</span>
                    {' — '}
                    {m.type === 1 ? 'Stock in' : 'Stock out'} · {m.quantity} adet
                  </div>
                  <div className="shrink-0 self-end ml-2">
                    <span className="font-medium text-violet-500">View</span>
                    <span className="hidden sm:inline"> -&gt;</span>
                  </div>
                </div>
              </div>
            </li>
          ))}
        </ul>
      </div>
    )
  }

  return (
    <div className="col-span-full xl:col-span-6 bg-white dark:bg-gray-800 shadow-xs rounded-xl">
      <header className="px-5 py-4 border-b border-gray-100 dark:border-gray-700/60">
        <h2 className="font-semibold text-gray-800 dark:text-gray-100">Recent Activity</h2>
      </header>
      <div className="p-3">
        {raw.length === 0 ? (
          <p className="text-sm text-gray-500 p-4 text-center">Henüz stok hareketi yok.</p>
        ) : (
          <>
            {renderGroup('Today', today)}
            {renderGroup('Yesterday', yesterday)}
            {renderGroup('Earlier', earlier)}
          </>
        )}
      </div>
    </div>
  )
}

export default DashboardCard12
