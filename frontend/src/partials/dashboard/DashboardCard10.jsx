import React from 'react'
import { useMosaicDashboard } from '../../context/MosaicDashboardContext'
import { fmtTryFull } from './mosaicChartBuilders'

function initials(name) {
  const p = (name || '?').split(/\s+/)
  return ((p[0]?.[0] || '') + (p[1]?.[0] || p[0]?.[1] || '')).toUpperCase().slice(0, 2)
}

function DashboardCard10() {
  const { data: stats } = useMosaicDashboard()
  if (!stats) return null

  const rows = (stats.topValuableProducts ?? []).slice(0, 5)
  const palette = ['bg-violet-500', 'bg-sky-500', 'bg-green-500', 'bg-amber-500', 'bg-red-500']

  return (
    <div className="col-span-full xl:col-span-6 bg-white dark:bg-gray-800 shadow-xs rounded-xl">
      <header className="px-5 py-4 border-b border-gray-100 dark:border-gray-700/60">
        <h2 className="font-semibold text-gray-800 dark:text-gray-100">Customers</h2>
      </header>
      <div className="p-3">
        <div className="overflow-x-auto">
          <table className="table-auto w-full">
            <thead className="text-xs font-semibold uppercase text-gray-400 dark:text-gray-500 bg-gray-50 dark:bg-gray-700/50">
              <tr>
                <th className="p-2 whitespace-nowrap">
                  <div className="font-semibold text-left">Name</div>
                </th>
                <th className="p-2 whitespace-nowrap">
                  <div className="font-semibold text-left">Email</div>
                </th>
                <th className="p-2 whitespace-nowrap">
                  <div className="font-semibold text-left">Spent</div>
                </th>
                <th className="p-2 whitespace-nowrap">
                  <div className="font-semibold text-center">Country</div>
                </th>
              </tr>
            </thead>
            <tbody className="text-sm divide-y divide-gray-100 dark:divide-gray-700/60">
              {rows.length === 0 ? (
                <tr>
                  <td colSpan={4} className="p-4 text-center text-gray-500">
                    Veri yok
                  </td>
                </tr>
              ) : (
                rows.map((p, i) => (
                  <tr key={p.productId}>
                    <td className="p-2 whitespace-nowrap">
                      <div className="flex items-center">
                        <div
                          className={`w-10 h-10 shrink-0 mr-2 sm:mr-3 rounded-full ${palette[i % palette.length]} flex items-center justify-center text-xs font-bold text-white`}
                        >
                          {initials(p.productName)}
                        </div>
                        <div className="font-medium text-gray-800 dark:text-gray-100">{p.productName}</div>
                      </div>
                    </td>
                    <td className="p-2 whitespace-nowrap">
                      <div className="text-left text-gray-600 dark:text-gray-300 font-mono text-xs">{p.stockCode}</div>
                    </td>
                    <td className="p-2 whitespace-nowrap">
                      <div className="text-left font-medium text-green-500">{fmtTryFull(p.potentialProfit)}</div>
                    </td>
                    <td className="p-2 whitespace-nowrap">
                      <div className="text-lg text-center">—</div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

export default DashboardCard10
