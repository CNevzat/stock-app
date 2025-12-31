import { Routes, Route, Link, useLocation, useNavigate } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { useState, useEffect } from 'react'
import DashboardPage from './pages/DashboardPage'
import CategoriesPage from './pages/CategoriesPage'
import LocationsPage from './pages/LocationsPage'
import ProductsPage from './pages/ProductsPage'
import ProductAttributesPage from './pages/ProductAttributesPage'
import StockMovementsPage from './pages/StockMovementsPage'
import TodosPage from './pages/TodosPage'
import LoginPage from './pages/LoginPage'
import UsersPage from './pages/UsersPage'
import RolesPage from './pages/RolesPage'
import ProtectedRoute from './components/ProtectedRoute'
import { ChatWidget } from './components/ChatWidget'
import { ToastContainer } from './components/Toast'
import { authService } from './services/authService'

function NavLink({ to, onClick, children }: { to: string; onClick: () => void; children: React.ReactNode }) {
  const location = useLocation()
  const isActive = location.pathname === to

  return (
    <Link
      to={to}
      onClick={onClick}
      className={`
        relative inline-flex items-center px-4 py-2 text-sm font-semibold rounded-lg transition-all duration-200
        ${isActive 
          ? 'text-indigo-600 bg-indigo-50 shadow-sm' 
          : 'text-gray-700 hover:text-indigo-600 hover:bg-gray-50'
        }
      `}
    >
      {children}
      {isActive && (
        <span className="absolute bottom-0 left-1/2 transform -translate-x-1/2 w-8 h-1 bg-indigo-600 rounded-t-full"></span>
      )}
    </Link>
  )
}

function App() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const [isAuthenticated, setIsAuthenticated] = useState(authService.isAuthenticated())
  const [user, setUser] = useState(authService.getUser())

  useEffect(() => {
    // Check authentication on mount and on location change
    const checkAuth = () => {
      const authenticated = authService.isAuthenticated()
      const currentUser = authService.getUser()
      setIsAuthenticated(authenticated)
      setUser(currentUser)
      
      // Check if password change is required
      if (authenticated && currentUser?.mustChangePassword) {
        // Redirect to password change (or show modal)
        // For now, we'll handle this in the login flow
      }
    }
    
    // Initial check
    checkAuth()
    
    // Listen for storage changes (when login happens in another tab)
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === 'token' || e.key === 'user') {
        checkAuth()
      }
    }
    
    window.addEventListener('storage', handleStorageChange)
    
    return () => {
      window.removeEventListener('storage', handleStorageChange)
    }
  }, [])
  
  // Also check auth when location changes (e.g., after login/logout)
  const location = useLocation()
  useEffect(() => {
    const authenticated = authService.isAuthenticated()
    const currentUser = authService.getUser()
    setIsAuthenticated(authenticated)
    setUser(currentUser)
  }, [location.pathname])

  const handleNavClick = () => {
    // Tüm query'leri invalidate et (sayfalar arası geçişte fresh data için)
    queryClient.invalidateQueries()
  }

  const handleLogout = () => {
    authService.clearTokens()
    setIsAuthenticated(false)
    setUser(null)
    navigate('/login')
  }

  // Menu groups with dropdowns
  const menuGroups = [
    {
      label: 'Dashboard',
      to: '/',
      icon: (
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
        </svg>
      ),
      singleItem: true,
    },
    {
      label: 'Stok Yönetimi',
      icon: (
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
        </svg>
      ),
      items: [
        { to: '/products', label: 'Ürünler', icon: (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
          </svg>
        )},
        { to: '/categories', label: 'Kategoriler', icon: (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
          </svg>
        )},
        { to: '/attributes', label: 'Öznitelikler', icon: (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 21a4 4 0 01-4-4V5a2 2 0 012-2h4a2 2 0 012 2v12a4 4 0 01-4 4zm0 0h12a2 2 0 002-2v-4a2 2 0 00-2-2h-2.343M11 7.343l1.657-1.657a2 2 0 012.828 0l2.829 2.829a2 2 0 010 2.828l-8.486 8.485M7 17h.01" />
          </svg>
        )},
        { to: '/stock-movements', label: 'Stok Hareketleri', icon: (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
          </svg>
        )},
        { to: '/locations', label: 'Lokasyonlar', icon: (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
        )},
      ],
    },
    {
      label: 'Görevler',
      icon: (
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
        </svg>
      ),
      items: [
        { to: '/todos', label: 'Yapılacaklar', icon: (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
          </svg>
        )},
      ],
    },
    {
      label: 'Yönetim',
      icon: (
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
        </svg>
      ),
      roles: ['Admin', 'Manager'],
      items: [
        { to: '/users', label: 'Kullanıcı Yönetimi', icon: (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
          </svg>
        ), roles: ['Admin', 'Manager'] },
        { to: '/roles', label: 'Rol Yönetimi', icon: (
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
          </svg>
        ), roles: ['Admin', 'Manager'] },
      ],
    },
  ]

  return (
    <div className="relative min-h-screen bg-gray-50">
      <div className="relative z-10">
      {/* Navigation */}
      <nav className="bg-white shadow-sm border-b border-gray-200 relative z-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-20">
            {/* Logo */}
            <div className="flex-shrink-0 flex items-center">
              <Link to="/" onClick={handleNavClick} className="flex items-center space-x-3 group">
                <div className="flex items-center justify-center w-10 h-10 bg-gradient-to-br from-indigo-600 to-indigo-700 rounded-xl shadow-lg transform group-hover:scale-105 transition-transform duration-200">
                  <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                  </svg>
                </div>
                <div>
                  <h1 className="text-2xl font-bold bg-gradient-to-r from-indigo-600 to-indigo-800 bg-clip-text text-transparent">
                    Stock App
                  </h1>
                  <p className="text-xs text-gray-500 hidden sm:block">Stok Yönetim Sistemi</p>
                </div>
              </Link>
            </div>

            {/* Navigation Links */}
            <div className="hidden lg:flex lg:items-center lg:space-x-1">
              {menuGroups
                .filter(group => {
                  // Filter by role if specified
                  if (group.roles && user) {
                    return group.roles.some(role => user.roles.includes(role));
                  }
                  return true;
                })
                .map((group) => {
                  // Single item (no dropdown)
                  if (group.singleItem) {
                    return (
                      <NavLink key={group.to} to={group.to} onClick={handleNavClick}>
                        {group.icon}
                        {group.label}
                      </NavLink>
                    );
                  }
                  
                  // Dropdown menu
                  return (
                    <div key={group.label} className="relative group">
                      <button className="inline-flex items-center px-4 py-2 text-sm font-semibold rounded-lg transition-all duration-200 text-gray-700 hover:text-indigo-600 hover:bg-gray-50">
                        {group.icon}
                        <span className="ml-2">{group.label}</span>
                        <svg className="w-4 h-4 ml-1 transition-transform group-hover:rotate-180" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                        </svg>
                      </button>
                      
                      {/* Dropdown Content */}
                      <div className="absolute left-0 mt-2 w-56 rounded-md shadow-lg bg-white ring-1 ring-black ring-opacity-5 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 z-50">
                        <div className="py-1">
                          {group.items
                            ?.filter((item: any) => {
                              if (item.roles && user) {
                                return item.roles.some((role: string) => user.roles.includes(role));
                              }
                              return true;
                            })
                            .map((item) => (
                              <Link
                                key={item.to}
                                to={item.to}
                                onClick={handleNavClick}
                                className="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-indigo-50 hover:text-indigo-600 transition-colors"
                              >
                                {item.icon}
                                <span className="ml-3">{item.label}</span>
                              </Link>
                            ))}
                        </div>
                      </div>
                    </div>
                  );
                })}
            </div>

            {/* User Menu */}
            <div className="flex items-center space-x-4">
              {isAuthenticated && user ? (
                <div className="flex items-center space-x-3">
                  <div className="hidden sm:block text-right">
                    <p className="text-sm font-medium text-gray-900">{user.firstName} {user.lastName}</p>
                    <p className="text-xs text-gray-500">{user.roles.join(', ')}</p>
                  </div>
                  <button
                    onClick={handleLogout}
                    className="inline-flex items-center px-3 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
                  >
                    <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                    </svg>
                    Çıkış
                  </button>
                </div>
              ) : (
                <Link
                  to="/login"
                  className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700"
                >
                  Giriş Yap
                </Link>
              )}
            </div>

            {/* Mobile Menu Button */}
            <div className="lg:hidden ml-4">
              <button
                type="button"
                className="inline-flex items-center justify-center p-2 rounded-md text-gray-700 hover:text-indigo-600 hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-indigo-500"
                aria-label="Toggle menu"
              >
                <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                </svg>
              </button>
            </div>
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8 relative z-10">
        <Routes>
          {/* Public Routes */}
          <Route path="/login" element={<LoginPage />} />

          {/* Protected Routes */}
          <Route path="/" element={
            <ProtectedRoute>
              <DashboardPage />
            </ProtectedRoute>
          } />
          <Route path="/categories" element={
            <ProtectedRoute>
              <CategoriesPage />
            </ProtectedRoute>
          } />
          <Route path="/locations" element={
            <ProtectedRoute>
              <LocationsPage />
            </ProtectedRoute>
          } />
          <Route path="/products" element={
            <ProtectedRoute>
              <ProductsPage />
            </ProtectedRoute>
          } />
          <Route path="/attributes" element={
            <ProtectedRoute>
              <ProductAttributesPage />
            </ProtectedRoute>
          } />
          <Route path="/stock-movements" element={
            <ProtectedRoute>
              <StockMovementsPage />
            </ProtectedRoute>
          } />
          <Route path="/todos" element={
            <ProtectedRoute>
              <TodosPage />
            </ProtectedRoute>
          } />
          <Route path="/users" element={
            <ProtectedRoute requiredRoles={['Admin', 'Manager']}>
              <UsersPage />
            </ProtectedRoute>
          } />
          <Route path="/roles" element={
            <ProtectedRoute>
              <RolesPage />
            </ProtectedRoute>
          } />
        </Routes>
      </main>
      </div>

      <ChatWidget />
      <ToastContainer />
    </div>
  )
}

export default App
