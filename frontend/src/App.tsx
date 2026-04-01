import { Routes, Route, Navigate, useLocation } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { useState, useEffect } from 'react'
import HomePage from './pages/HomePage'
import MosaicDashboardPage from './pages/MosaicDashboardPage'
import CategoriesPage from './pages/CategoriesPage'
import LocationsPage from './pages/LocationsPage'
import ProductsPage from './pages/ProductsPage'
import ProductAttributesPage from './pages/ProductAttributesPage'
import StockMovementsPage from './pages/StockMovementsPage'
import TodosPage from './pages/TodosPage'
import LoginPage from './pages/LoginPage'
import UsersPage from './pages/UsersPage'
import RolesPage from './pages/RolesPage'
import SupportRequestsPage from './pages/SupportRequestsPage'
import ProtectedRoute from './components/ProtectedRoute'
import MosaicShellLayout from './components/MosaicShellLayout'
import { ChatWidget } from './components/ChatWidget'
import { ToastContainer } from './components/Toast'
import { authService } from './services/authService'

function App() {
  const queryClient = useQueryClient()
  const [isAuthenticated, setIsAuthenticated] = useState(authService.isAuthenticated())

  const location = useLocation()

  useEffect(() => {
    setIsAuthenticated(authService.isAuthenticated())
  }, [location.pathname])

  useEffect(() => {
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === 'token' || e.key === 'user') {
        setIsAuthenticated(authService.isAuthenticated())
      }
    }
    window.addEventListener('storage', handleStorageChange)
    return () => window.removeEventListener('storage', handleStorageChange)
  }, [])

  useEffect(() => {
    if (isAuthenticated) queryClient.invalidateQueries()
  }, [location.pathname])

  useEffect(() => {
    document.documentElement.style.scrollBehavior = 'auto'
    window.scroll({ top: 0 })
    document.documentElement.style.scrollBehavior = ''
  }, [location.pathname])

  return (
    <>
      <Routes>
        {/* ── Public routes ── */}
        <Route path="/" element={
          isAuthenticated ? <Navigate to="/dashboard" replace /> : <HomePage />
        } />
        <Route path="/login" element={
          isAuthenticated ? <Navigate to="/dashboard" replace /> : <LoginPage />
        } />

        <Route
          element={
            <ProtectedRoute>
              <MosaicShellLayout />
            </ProtectedRoute>
          }
        >
          <Route path="/dashboard" element={<MosaicDashboardPage />} />
          <Route path="/products" element={<ProductsPage />} />
          <Route path="/categories" element={<CategoriesPage />} />
          <Route path="/attributes" element={<ProductAttributesPage />} />
          <Route path="/stock-movements" element={<StockMovementsPage />} />
          <Route path="/locations" element={<LocationsPage />} />
          <Route path="/todos" element={<TodosPage />} />
          <Route
            path="/users"
            element={
              <ProtectedRoute requiredRoles={['Admin', 'Manager']}>
                <UsersPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/roles"
            element={
              <ProtectedRoute requiredRoles={['Admin', 'Manager']}>
                <RolesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/support-requests"
            element={
              <ProtectedRoute requiredRoles={['Admin']}>
                <SupportRequestsPage />
              </ProtectedRoute>
            }
          />
        </Route>
      </Routes>

      <ChatWidget />
      <ToastContainer />
    </>
  )
}

export default App
