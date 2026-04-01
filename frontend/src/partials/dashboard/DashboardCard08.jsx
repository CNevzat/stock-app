import React from 'react'
import LineChart from '../../charts/LineChart02'
import { useMosaicDashboard } from '../../context/MosaicDashboardContext'
import { buildSalesOverTimeLines } from './mosaicChartBuilders'

function DashboardCard08() {
  const { data: stats } = useMosaicDashboard()
  if (!stats) return null
  const chartData = buildSalesOverTimeLines(stats)
  return (
    <div className="flex flex-col col-span-full sm:col-span-6 bg-white dark:bg-gray-800 shadow-xs rounded-xl">
      <header className="px-5 py-4 border-b border-gray-100 dark:border-gray-700/60 flex items-center">
        <h2 className="font-semibold text-gray-800 dark:text-gray-100">Sales Over Time (all stores)</h2>
      </header>
      <LineChart data={chartData} width={595} height={248} />
    </div>
  )
}

export default DashboardCard08
