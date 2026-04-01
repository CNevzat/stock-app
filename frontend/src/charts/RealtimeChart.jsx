import React, { useRef, useEffect, useState } from 'react';
import { useThemeProvider } from '../utils/ThemeContext';

import { chartColors } from './ChartjsConfig';
import {
  Chart, LineController, LineElement, Filler, PointElement, LinearScale, TimeScale, Tooltip,
} from 'chart.js';
import 'chartjs-adapter-moment';

// Import utilities
import { adjustColorOpacity, getCssVariable, formatValue } from '../utils/Utils';
import { safeChartUpdate } from '../utils/chartSafeUpdate';

Chart.register(LineController, LineElement, Filler, PointElement, LinearScale, TimeScale, Tooltip);

function RealtimeChart({
  data,
  width,
  height
}) {

  const [chart, setChart] = useState(null)
  const canvas = useRef(null);
  const chartValue = useRef(null);
  const chartDeviation = useRef(null);
  const { currentTheme } = useThemeProvider();
  const darkMode = currentTheme === 'dark';  
  const { textColor, gridColor, tooltipTitleColor, tooltipBodyColor, tooltipBgColor, tooltipBorderColor } = chartColors;

  useEffect(() => {
    const canvasEl = canvas.current;
    if (!canvasEl || !data?.datasets?.length) return;
    const existing = Chart.getChart(canvasEl);
    if (existing) existing.destroy();
    // eslint-disable-next-line no-unused-vars
    const newChart = new Chart(canvasEl, {
      type: 'line',
      data: data,
      options: {
        layout: {
          padding: 20,
        },
        scales: {
          y: {
            border: {
              display: false,
            },
            beginAtZero: true,
            ticks: {
              maxTicksLimit: 5,
              callback: (value) => formatValue(value),
              color: darkMode ? textColor.dark : textColor.light,
            },
            grid: {
              color: darkMode ? gridColor.dark : gridColor.light,
            },
          },
          x: {
            type: 'time',
            time: {
              tooltipFormat: 'MMM D, HH:mm',
              displayFormats: {
                minute: 'HH:mm',
                hour: 'MMM D, HH:mm',
                day: 'MMM D',
                week: 'MMM D',
                month: 'MMM YYYY',
              },
            },
            border: {
              display: false,
            },
            grid: {
              display: false,
            },
            ticks: {
              autoSkipPadding: 48,
              maxRotation: 0,
              color: darkMode ? textColor.dark : textColor.light,
            },
          },
        },
        plugins: {
          legend: {
            display: false,
          },
          tooltip: {
            titleFont: {
              weight: 600,
            },
            callbacks: {
              label: (context) => formatValue(context.parsed.y),
            },
            titleColor: darkMode ? tooltipTitleColor.dark : tooltipTitleColor.light,
            bodyColor: darkMode ? tooltipBodyColor.dark : tooltipBodyColor.light,
            backgroundColor: darkMode ? tooltipBgColor.dark : tooltipBgColor.light,
            borderColor: darkMode ? tooltipBorderColor.dark : tooltipBorderColor.light,
          },
        },
        interaction: {
          intersect: false,
          mode: 'nearest',
        },
        animation: false,
        maintainAspectRatio: false,
      },
    });
    setChart(newChart);
    return () => newChart.destroy();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [data]);

  // Update header values
  useEffect(() => {
    if (!chartValue.current || !chartDeviation.current) return;
    const series = data?.datasets?.[0]?.data;
    if (!series?.length) return;
    const yOf = (pt) => (pt != null && typeof pt === 'object' && 'y' in pt ? Number(pt.y) : Number(pt)) || 0;
    const currentValue = yOf(series[series.length - 1]);
    const previousValue = series.length > 1 ? yOf(series[series.length - 2]) : currentValue;
    const safePrev = previousValue === 0 ? (currentValue === 0 ? 1 : currentValue) : previousValue;
    const diff = ((currentValue - previousValue) / safePrev) * 100;
    const safeDiff = Number.isFinite(diff) ? diff : 0;
    chartValue.current.textContent = String(currentValue);
    if (safeDiff < 0) {
      chartDeviation.current.style.backgroundColor = adjustColorOpacity(getCssVariable('--color-red-500'), 0.2);
      chartDeviation.current.style.color = getCssVariable('--color-red-700');
    } else {
      chartDeviation.current.style.backgroundColor = adjustColorOpacity(getCssVariable('--color-green-500'), 0.2);
      chartDeviation.current.style.color = getCssVariable('--color-green-700');
    }
    chartDeviation.current.textContent = `${safeDiff > 0 ? '+' : ''}${safeDiff.toFixed(2)}%`;
  }, [data]);

  useEffect(() => {
    if (!chart) return

    if (darkMode) {
      chart.options.scales.x.ticks.color = textColor.dark;
      chart.options.scales.y.ticks.color = textColor.dark;
      chart.options.scales.y.grid.color = gridColor.dark;
      chart.options.plugins.tooltip.titleColor = tooltipTitleColor.dark;
      chart.options.plugins.tooltip.bodyColor = tooltipBodyColor.dark;
      chart.options.plugins.tooltip.backgroundColor = tooltipBgColor.dark;
      chart.options.plugins.tooltip.borderColor = tooltipBorderColor.dark;      
    } else {
      chart.options.scales.x.ticks.color = textColor.light;
      chart.options.scales.y.ticks.color = textColor.light;
      chart.options.scales.y.grid.color = gridColor.light;
      chart.options.plugins.tooltip.titleColor = tooltipTitleColor.light;
      chart.options.plugins.tooltip.bodyColor = tooltipBodyColor.light;
      chart.options.plugins.tooltip.backgroundColor = tooltipBgColor.light;
      chart.options.plugins.tooltip.borderColor = tooltipBorderColor.light; 
    }
    safeChartUpdate(chart, 'none');
  }, [currentTheme])    


  return (
    <React.Fragment>
      <div className="px-5 py-3">
        <div className="flex items-start">
          <div className="text-3xl font-bold text-gray-800 dark:text-gray-100 mr-2 tabular-nums">$<span ref={chartValue}>57.81</span></div>
          <div ref={chartDeviation} className="text-sm font-medium px-1.5 rounded-full"></div>
        </div>
      </div>
      <div className="grow">
        <canvas ref={canvas} width={width} height={height}></canvas>
      </div>
    </React.Fragment>
  );
}

export default RealtimeChart;