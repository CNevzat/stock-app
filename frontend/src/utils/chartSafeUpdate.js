/**
 * Chart.js resize/update can run getComputedStyle on the canvas. After React removes
 * the canvas (unmount, fast remount, Strict Mode), the instance may still receive
 * updates and throw. Skip updates when the canvas is not connected to the document.
 */
export function safeChartUpdate(chart, mode) {
  if (!chart?.canvas?.isConnected) return;
  try {
    chart.update(mode);
  } catch {
    // instance may be mid-teardown
  }
}
