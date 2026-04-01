import { createContext, useContext } from 'react'
import { useQuery } from '@tanstack/react-query'
import { dashboardService } from '../services/dashboardService'

const MosaicDashboardContext = createContext(null)

export function MosaicDashboardProvider({ children }) {
  const query = useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: () => dashboardService.getStats(),
  })
  return <MosaicDashboardContext.Provider value={query}>{children}</MosaicDashboardContext.Provider>
}

export function useMosaicDashboard() {
  const ctx = useContext(MosaicDashboardContext)
  if (!ctx) {
    throw new Error('useMosaicDashboard must be used within MosaicDashboardProvider')
  }
  return ctx
}
