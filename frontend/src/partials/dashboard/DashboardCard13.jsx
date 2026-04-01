import React from 'react'
import { isToday, parseISO } from 'date-fns'
import { useMosaicDashboard } from '../../context/MosaicDashboardContext'

function bucket(d) {
  try {
    const dt = typeof d === 'string' ? parseISO(d) : d
    if (isToday(dt)) return 'Today'
    if (isYesterday(dt)) return 'Yesterday'
    return 'Earlier'
  } catch {
    return 'Earlier'
  }
}

function DashboardCard13() {
  const { data: stats } = useMosaicDashboard()
  if (!stats) return null

  const movements = [...(stats.recentStockMovements ?? [])].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
  )
  const today = movements.filter((m) => bucket(m.createdAt) === 'Today')

  return (
    <div className="col-span-full xl:col-span-6 bg-white dark:bg-gray-800 shadow-xs rounded-xl">
      <header className="px-5 py-4 border-b border-gray-100 dark:border-gray-700/60">
        <h2 className="font-semibold text-gray-800 dark:text-gray-100">Income/Expenses</h2>
      </header>
      <div className="p-3">
        {today.length === 0 ? (
          <p className="text-sm text-gray-500 p-4 text-center">Bugün hareket yok.</p>
        ) : (
          <div>
            <header className="text-xs uppercase text-gray-400 dark:text-gray-500 bg-gray-50 dark:bg-gray-700/50 rounded-xs font-semibold p-2">
              Today
            </header>
            <ul className="my-1">
              {today.map((m) => {
                const isIn = m.type === 1
                return (
                  <li key={m.id} className="flex px-2">
                    <div
                      className={`w-9 h-9 rounded-full shrink-0 my-2 mr-3 flex items-center justify-center ${
                        isIn ? 'bg-green-500' : 'bg-red-500'
                      }`}
                    >
                      <svg className="w-5 h-5 fill-current text-white" viewBox="0 0 24 24">
                        {isIn ? <path d="M12 4l8 8h-5v8h-6v-8H4l8-8z" /> : <path d="M12 20l-8-8h5V4h6v8h5l-8 8z" />}
                      </svg>
                    </div>
                    <div className="grow flex items-center border-b border-gray-100 dark:border-gray-700/60 text-sm py-2">
                      <div className="grow flex justify-between">
                        <div className="self-center">
                          <span className="font-medium text-gray-800 dark:text-gray-100">{m.productName}</span>
                          <span className="text-gray-500 dark:text-gray-400"> · {isIn ? 'Giriş' : 'Çıkış'}</span>
                        </div>
                        <div className="shrink-0 self-start ml-2">
                          <span className={`font-medium ${isIn ? 'text-green-600' : 'text-gray-800 dark:text-gray-100'}`}>
                            {isIn ? '+' : '-'}
                            {m.quantity}
                          </span>
                        </div>
                      </div>
                    </div>
                  </li>
                )
              })}
            </ul>
          </div>
        )}
      </div>
    </div>
  )
}

export default DashboardCard13
