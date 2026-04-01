import React from 'react'
import { Link } from 'react-router-dom'
import LineChart from '../../charts/LineChart01'
import { chartAreaGradient } from '../../charts/ChartjsConfig'
import EditMenu from '../../components/DropdownEditMenu'
import { adjustColorOpacity, getCssVariable } from '../../utils/Utils'
import { useMosaicDashboard } from '../../context/MosaicDashboardContext'
import { labelsFromTrend, fmtTryFull, pctChangeSeries } from './mosaicChartBuilders'

function DashboardCard03() {
  const { data: stats } = useMosaicDashboard()
  if (!stats) return null

  const trend = stats.stockMovementTrend ?? []
  const labels = labelsFromTrend(trend)
  const net = trend.length ? trend.map((x) => x.stockIn - x.stockOut) : [0]
  const ref = trend.length ? net.map((n) => Math.round(n * 0.8)) : [0]
  const pct = pctChangeSeries(net)

  const chartData = {
    labels: trend.length ? labels : ['01-01-2024'],
    datasets: [
      {
        data: net,
        fill: true,
        backgroundColor(context) {
          const chart = context.chart
          const { ctx, chartArea } = chart
          return chartAreaGradient(ctx, chartArea, [
            { stop: 0, color: adjustColorOpacity(getCssVariable('--color-violet-500'), 0) },
            { stop: 1, color: adjustColorOpacity(getCssVariable('--color-violet-500'), 0.2) },
          ])
        },
        borderColor: getCssVariable('--color-violet-500'),
        borderWidth: 2,
        pointRadius: 0,
        pointHoverRadius: 3,
        pointBackgroundColor: getCssVariable('--color-violet-500'),
        pointHoverBackgroundColor: getCssVariable('--color-violet-500'),
        pointBorderWidth: 0,
        pointHoverBorderWidth: 0,
        clip: 20,
        tension: 0.2,
      },
      {
        data: ref,
        borderColor: adjustColorOpacity(getCssVariable('--color-gray-500'), 0.25),
        borderWidth: 2,
        pointRadius: 0,
        pointHoverRadius: 3,
        pointBackgroundColor: adjustColorOpacity(getCssVariable('--color-gray-500'), 0.25),
        pointHoverBackgroundColor: adjustColorOpacity(getCssVariable('--color-gray-500'), 0.25),
        pointBorderWidth: 0,
        pointHoverBorderWidth: 0,
        clip: 20,
        tension: 0.2,
      },
    ],
  }

  return (
    <div className="flex flex-col col-span-full sm:col-span-6 xl:col-span-4 bg-white dark:bg-gray-800 shadow-xs rounded-xl">
      <div className="px-5 pt-5">
        <header className="flex justify-between items-start mb-2">
          <h2 className="text-lg font-semibold text-gray-800 dark:text-gray-100 mb-2">Acme Professional</h2>
          <EditMenu align="right" className="relative inline-flex">
            <li>
              <Link className="font-medium text-sm text-gray-600 dark:text-gray-300 hover:text-gray-800 dark:hover:text-gray-200 flex py-1 px-3" to="#0">
                Option 1
              </Link>
            </li>
            <li>
              <Link className="font-medium text-sm text-gray-600 dark:text-gray-300 hover:text-gray-800 dark:hover:text-gray-200 flex py-1 px-3" to="#0">
                Option 2
              </Link>
            </li>
            <li>
              <Link className="font-medium text-sm text-red-500 hover:text-red-600 flex py-1 px-3" to="#0">
                Remove
              </Link>
            </li>
          </EditMenu>
        </header>
        <div className="text-xs font-semibold text-gray-400 dark:text-gray-500 uppercase mb-1">Sales</div>
        <div className="flex items-start">
          <div className="text-3xl font-bold text-gray-800 dark:text-gray-100 mr-2">{fmtTryFull(stats.totalPotentialProfit)}</div>
          <div
            className={`text-sm font-medium px-1.5 rounded-full ${
              pct >= 0 ? 'text-green-700 bg-green-500/20' : 'text-red-700 bg-red-500/20'
            }`}
          >
            {pct >= 0 ? '+' : ''}
            {pct}%
          </div>
        </div>
      </div>
      <div className="grow max-sm:max-h-[128px] xl:max-h-[128px]">
        <LineChart data={chartData} width={389} height={128} />
      </div>
    </div>
  )
}

export default DashboardCard03
