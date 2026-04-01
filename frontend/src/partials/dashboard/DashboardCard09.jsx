import React from 'react'
import Tooltip from '../../components/Tooltip'
import BarChart from '../../charts/BarChart02'
import { useMosaicDashboard } from '../../context/MosaicDashboardContext'
import { buildSalesVsRefunds, fmtTryFull, pctChangeSeries } from './mosaicChartBuilders'

function DashboardCard09() {
  const { data: stats } = useMosaicDashboard()
  if (!stats) return null

  const trend = stats.stockMovementTrend ?? []
  const last6 = trend.length >= 6 ? trend.slice(-6) : trend
  const sumIn = last6.reduce((s, x) => s + x.stockIn, 0)
  const sumOut = last6.reduce((s, x) => s + x.stockOut, 0)
  const net = sumIn - sumOut
  const pct = pctChangeSeries(last6.map((x) => x.stockIn - x.stockOut))
  const chartData = buildSalesVsRefunds(stats)

  return (
    <div className="flex flex-col col-span-full sm:col-span-6 bg-white dark:bg-gray-800 shadow-xs rounded-xl">
      <header className="px-5 py-4 border-b border-gray-100 dark:border-gray-700/60 flex items-center">
        <h2 className="font-semibold text-gray-800 dark:text-gray-100">Sales VS Refunds</h2>
        <Tooltip className="ml-2" size="lg">
          <div className="text-sm">Giriş ve çıkış hareketleri (son dönem, API).</div>
        </Tooltip>
      </header>
      <div className="px-5 py-3">
        <div className="flex items-start">
          <div className="text-3xl font-bold text-gray-800 dark:text-gray-100 mr-2">{fmtTryFull(net)}</div>
          <div
            className={`text-sm font-medium px-1.5 rounded-full ${
              pct >= 0 ? 'text-red-700 bg-red-500/20' : 'text-green-700 bg-green-500/20'
            }`}
          >
            {pct >= 0 ? '+' : ''}
            {pct}%
          </div>
        </div>
      </div>
      <div className="grow">
        <BarChart data={chartData} width={595} height={248} />
      </div>
    </div>
  )
}

export default DashboardCard09
