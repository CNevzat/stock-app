/**
 * Maps Stock App dashboard API stats → Chart.js configs for Mosaic template cards.
 */
import { adjustColorOpacity, getCssVariable } from '../../utils/Utils'
import { chartAreaGradient } from '../../charts/ChartjsConfig'

export function fmtTry(n) {
  return new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency: 'TRY',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(Number(n ?? 0))
}

export function fmtTryFull(n) {
  return new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency: 'TRY',
    minimumFractionDigits: 2,
  }).format(Number(n ?? 0))
}

/** Chart.js time scale expects MM-DD-YYYY */
export function labelsFromTrend(trend) {
  const t = trend ?? []
  if (t.length === 0) return ['01-01-2024']
  return t.map((_, i) => {
    const month = (i % 12) + 1
    const day = (i % 28) + 1
    return `${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}-2024`
  })
}

export function pctChangeSeries(arr) {
  if (!arr?.length || arr.length < 2) return 0
  const a = Number(arr[0]) || 0
  const b = Number(arr[arr.length - 1]) || 0
  if (a === 0) return b > 0 ? 100 : 0
  return Math.round(((b - a) / Math.abs(a)) * 100)
}

function lineSparkDatasetPrimary(values) {
  return {
    data: values,
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
  }
}

function lineSparkDatasetSecondary(values) {
  return {
    data: values,
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
  }
}

export function buildSparklineData(stats) {
  const trend = stats?.stockMovementTrend ?? []
  const labels = labelsFromTrend(trend)
  const primary = trend.length ? trend.map((x) => x.stockIn) : [0]
  const secondary = trend.length ? trend.map((x) => Math.round(x.stockIn * 0.7 + x.stockOut * 0.2)) : [0]
  return {
    labels: trend.length ? labels : ['01-01-2024'],
    datasets: [lineSparkDatasetPrimary(primary), lineSparkDatasetSecondary(secondary)],
  }
}

export function buildDirectIndirectBar(stats) {
  const cats = (stats?.categoryStats ?? []).slice(0, 6)
  const labels =
    cats.length > 0
      ? cats.map((c) => {
          const name = c.categoryName || 'Cat'
          return `01-${String((name.length % 9) + 1).padStart(2, '0')}-2024`
        })
      : ['01-01-2024', '02-01-2024', '03-01-2024', '04-01-2024', '05-01-2024', '06-01-2024']
  const direct = cats.length > 0 ? cats.map((c) => c.productCount) : [0, 0, 0, 0, 0, 0]
  const indirect = cats.length > 0 ? cats.map((c) => c.totalStock) : [0, 0, 0, 0, 0, 0]
  return {
    labels,
    datasets: [
      {
        label: 'Direct',
        data: direct,
        backgroundColor: getCssVariable('--color-sky-500'),
        hoverBackgroundColor: getCssVariable('--color-sky-600'),
        barPercentage: 0.7,
        categoryPercentage: 0.7,
        borderRadius: 4,
      },
      {
        label: 'Indirect',
        data: indirect,
        backgroundColor: getCssVariable('--color-violet-500'),
        hoverBackgroundColor: getCssVariable('--color-violet-600'),
        barPercentage: 0.7,
        categoryPercentage: 0.7,
        borderRadius: 4,
      },
    ],
  }
}

export function buildRealtimeLineData(stats) {
  const trend = stats?.stockMovementTrend ?? []
  const vals = trend.length ? trend.map((x) => x.stockIn + x.stockOut) : [0]
  const now = Date.now()
  const stepMs = 3600000
  // { x, y } so time scale can infer range; hourly steps match stock trend buckets
  const points = vals.map((y, i) => ({
    x: now - (vals.length - 1 - i) * stepMs,
    y,
  }))
  return {
    datasets: [
      {
        data: points,
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
    ],
  }
}

export function buildDoughnutCountries(stats) {
  const dist = stats?.stockDistribution ?? []
  if (dist.length === 0) {
    return {
      labels: ['No data'],
      datasets: [
        {
          label: 'Status',
          data: [1],
          backgroundColor: [getCssVariable('--color-gray-500')],
          hoverBackgroundColor: [getCssVariable('--color-gray-600')],
          borderWidth: 0,
        },
      ],
    }
  }
  const top = dist.slice(0, 3)
  const rest = dist.slice(3)
  const labels = [...top.map((x) => x.status), ...(rest.length ? ['Other'] : [])]
  const data = [...top.map((x) => x.count), ...(rest.length ? [rest.reduce((s, x) => s + x.count, 0)] : [])]
  const colors = [
    getCssVariable('--color-violet-500'),
    getCssVariable('--color-sky-500'),
    getCssVariable('--color-violet-800'),
    getCssVariable('--color-gray-500'),
  ]
  return {
    labels,
    datasets: [
      {
        label: 'Distribution',
        data,
        backgroundColor: colors.slice(0, data.length),
        hoverBackgroundColor: [
          getCssVariable('--color-violet-600'),
          getCssVariable('--color-sky-600'),
          getCssVariable('--color-violet-900'),
          getCssVariable('--color-gray-600'),
        ].slice(0, data.length),
        borderWidth: 0,
      },
    ],
  }
}

export function buildSalesOverTimeLines(stats) {
  const trend = stats?.lastYearStockMovementTrend?.length
    ? stats.lastYearStockMovementTrend
    : stats?.stockMovementTrend ?? []
  const labels = labelsFromTrend(trend)
  const ins = trend.length ? trend.map((x) => x.stockIn) : [0]
  const outs = trend.length ? trend.map((x) => x.stockOut) : [0]
  const net = trend.length ? trend.map((x) => x.stockIn - x.stockOut) : [0]
  const line = (colorVar, data, label) => ({
    label,
    data,
    borderColor: getCssVariable(colorVar),
    fill: false,
    borderWidth: 2,
    pointRadius: 0,
    pointHoverRadius: 3,
    pointBackgroundColor: getCssVariable(colorVar),
    pointHoverBackgroundColor: getCssVariable(colorVar),
    pointBorderWidth: 0,
    pointHoverBorderWidth: 0,
    clip: 20,
    tension: 0.2,
  })
  return {
    labels: trend.length ? labels : ['01-01-2024'],
    datasets: [
      line('--color-violet-500', ins, 'Current'),
      line('--color-sky-500', outs, 'Previous'),
      line('--color-green-500', net, 'Average'),
    ],
  }
}

export function buildSalesVsRefunds(stats) {
  const trend = stats?.stockMovementTrend ?? []
  const labels =
    trend.length >= 6
      ? labelsFromTrend(trend).slice(-6)
      : ['01-01-2024', '02-01-2024', '03-01-2024', '04-01-2024', '05-01-2024', '06-01-2024']
  const ins = trend.length >= 6 ? trend.slice(-6).map((x) => x.stockIn) : [0, 0, 0, 0, 0, 0]
  const outs = trend.length >= 6 ? trend.slice(-6).map((x) => -Math.abs(x.stockOut)) : [0, 0, 0, 0, 0, 0]
  return {
    labels,
    datasets: [
      {
        label: 'Stack 1',
        data: ins,
        backgroundColor: getCssVariable('--color-violet-500'),
        hoverBackgroundColor: getCssVariable('--color-violet-600'),
        barPercentage: 0.7,
        categoryPercentage: 0.7,
        borderRadius: 4,
      },
      {
        label: 'Stack 2',
        data: outs,
        backgroundColor: getCssVariable('--color-violet-200'),
        hoverBackgroundColor: getCssVariable('--color-violet-300'),
        barPercentage: 0.7,
        categoryPercentage: 0.7,
        borderRadius: 4,
      },
    ],
  }
}

export function buildRefundReasonsBlock(stats) {
  const dist = stats?.stockDistribution ?? []
  const defaults = [
    { label: 'Having difficulties using the product', v: 0, color: '--color-violet-500', hover: '--color-violet-600' },
    { label: 'Missing features I need', v: 0, color: '--color-violet-700', hover: '--color-violet-800' },
    { label: 'Not satisfied about the quality', v: 0, color: '--color-sky-500', hover: '--color-sky-600' },
    { label: 'Does not look as advertised', v: 0, color: '--color-green-500', hover: '--color-green-600' },
    { label: 'Other', v: 0, color: '--color-gray-200', hover: '--color-gray-300' },
  ]
  dist.forEach((d, i) => {
    if (i < defaults.length) defaults[i].v = d.count
  })
  const total = dist.reduce((s, x) => s + x.count, 0)
  const chartData = {
    labels: ['Reasons'],
    datasets: defaults.map((row) => ({
      label: row.label,
      data: [row.v],
      backgroundColor: getCssVariable(row.color),
      hoverBackgroundColor: getCssVariable(row.hover),
      barPercentage: 1,
      categoryPercentage: 1,
    })),
  }
  const pct = pctChangeSeries(dist.map((x) => x.count))
  return { chartData, total, pct }
}
