import React from 'react'
import BarChart from '../../charts/BarChart01'
import { useMosaicDashboard } from '../../context/MosaicDashboardContext'
import { buildDirectIndirectBar } from './mosaicChartBuilders'

function DashboardCard04() {
  const { data: stats } = useMosaicDashboard()
  if (!stats) return null
  const chartData = buildDirectIndirectBar(stats)
  return (
    <div className="flex flex-col col-span-full sm:col-span-6 bg-white dark:bg-gray-800 shadow-xs rounded-xl">
      <header className="px-5 py-4 border-b border-gray-100 dark:border-gray-700/60">
        <h2 className="font-semibold text-gray-800 dark:text-gray-100">Direct VS Indirect</h2>
      </header>
      <BarChart data={chartData} width={595} height={248} />
    </div>
  )
}

export default DashboardCard04
