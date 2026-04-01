import React from 'react'
import BarChart from '../../charts/BarChart03'
import { useMosaicDashboard } from '../../context/MosaicDashboardContext'
import { buildRefundReasonsBlock } from './mosaicChartBuilders'

function DashboardCard11() {
  const { data: stats } = useMosaicDashboard()
  if (!stats) return null
  const { chartData, total, pct } = buildRefundReasonsBlock(stats)

  return (
    <div className="flex flex-col col-span-full sm:col-span-6 bg-white dark:bg-gray-800 shadow-xs rounded-xl">
      <header className="px-5 py-4 border-b border-gray-100 dark:border-gray-700/60 flex items-center">
        <h2 className="font-semibold text-gray-800 dark:text-gray-100">Reason for Refunds</h2>
      </header>
      <div className="px-5 py-3">
        <div className="flex items-start">
          <div className="text-3xl font-bold text-gray-800 dark:text-gray-100 mr-2">{total.toLocaleString('tr-TR')}</div>
          <div
            className={`text-sm font-medium px-1.5 rounded-full ${
              pct <= 0 ? 'text-red-700 bg-red-500/20' : 'text-green-700 bg-green-500/20'
            }`}
          >
            {pct >= 0 ? '+' : ''}
            {pct}%
          </div>
        </div>
      </div>
      <div className="grow">
        <BarChart data={chartData} width={595} height={48} />
      </div>
    </div>
  )
}

export default DashboardCard11
