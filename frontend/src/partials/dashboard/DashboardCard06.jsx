import React from 'react'
import DoughnutChart from '../../charts/DoughnutChart'
import { useMosaicDashboard } from '../../context/MosaicDashboardContext'
import { buildDoughnutCountries } from './mosaicChartBuilders'

function DashboardCard06() {
  const { data: stats } = useMosaicDashboard()
  if (!stats) return null
  const chartData = buildDoughnutCountries(stats)
  return (
    <div className="flex flex-col col-span-full sm:col-span-6 xl:col-span-4 bg-white dark:bg-gray-800 shadow-xs rounded-xl">
      <header className="px-5 py-4 border-b border-gray-100 dark:border-gray-700/60">
        <h2 className="font-semibold text-gray-800 dark:text-gray-100">Top Countries</h2>
      </header>
      <DoughnutChart data={chartData} width={389} height={260} />
    </div>
  )
}

export default DashboardCard06
