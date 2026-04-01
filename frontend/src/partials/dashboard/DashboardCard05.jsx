import React, { useMemo } from 'react'
import Tooltip from '../../components/Tooltip'
import RealtimeChart from '../../charts/RealtimeChart'
import { useMosaicDashboard } from '../../context/MosaicDashboardContext'
import { buildRealtimeLineData } from './mosaicChartBuilders'

function DashboardCard05() {
  const { data: stats } = useMosaicDashboard()
  const chartData = useMemo(() => (stats ? buildRealtimeLineData(stats) : null), [stats])
  if (!chartData) return null
  return (
    <div className="flex flex-col col-span-full sm:col-span-6 bg-white dark:bg-gray-800 shadow-xs rounded-xl">
      <header className="px-5 py-4 border-b border-gray-100 dark:border-gray-700/60 flex items-center">
        <h2 className="font-semibold text-gray-800 dark:text-gray-100">Real Time Value</h2>
        <Tooltip className="ml-2">
          <div className="text-xs text-center whitespace-nowrap">
            Built with{' '}
            <a className="underline" href="https://www.chartjs.org/" target="_blank" rel="noreferrer">
              Chart.js
            </a>
          </div>
        </Tooltip>
      </header>
      <RealtimeChart data={chartData} width={595} height={248} />
    </div>
  )
}

export default DashboardCard05
