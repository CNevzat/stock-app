import { Routes, Route, Link, useLocation } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import DashboardPage from './pages/DashboardPage'
import CategoriesPage from './pages/CategoriesPage'
import LocationsPage from './pages/LocationsPage'
import ProductsPage from './pages/ProductsPage'
import ProductAttributesPage from './pages/ProductAttributesPage'
import StockMovementsPage from './pages/StockMovementsPage'
import TodosPage from './pages/TodosPage'

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

  const handleNavClick = () => {
    // Tüm query'leri invalidate et (sayfalar arası geçişte fresh data için)
    queryClient.invalidateQueries()
  }

  const navItems = [
    { to: '/', label: 'Dashboard', icon: (
      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
      </svg>
    )},
    { to: '/categories', label: 'Kategoriler', icon: (
      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z" />
      </svg>
    )},
    { to: '/locations', label: 'Lokasyonlar', icon: (
      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
      </svg>
    )},
    { to: '/products', label: 'Ürünler', icon: (
      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
      </svg>
    )},
    { to: '/attributes', label: 'Öznitelikler', icon: (
      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 21a4 4 0 01-4-4V5a2 2 0 012-2h4a2 2 0 012 2v12a4 4 0 01-4 4zm0 0h12a2 2 0 002-2v-4a2 2 0 00-2-2h-2.343M11 7.343l1.657-1.657a2 2 0 012.828 0l2.829 2.829a2 2 0 010 2.828l-8.486 8.485M7 17h.01" />
      </svg>
    )},
    { to: '/stock-movements', label: 'Stok Hareketleri', icon: (
      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
      </svg>
    )},
    { to: '/todos', label: 'Yapılacaklar', icon: (
      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
      </svg>
    )},
  ]

  return (
    <div className="min-h-screen bg-gray-50 relative">
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
            <div className="hidden lg:flex lg:items-center lg:space-x-2">
              {navItems.map((item) => (
                <NavLink key={item.to} to={item.to} onClick={handleNavClick}>
                  {item.icon}
                  {item.label}
                </NavLink>
              ))}
            </div>

            {/* Mobile Menu Button */}
            <div className="lg:hidden">
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
          <Route path="/" element={<DashboardPage />} />
          <Route path="/categories" element={<CategoriesPage />} />
          <Route path="/locations" element={<LocationsPage />} />
          <Route path="/products" element={<ProductsPage />} />
          <Route path="/attributes" element={<ProductAttributesPage />} />
          <Route path="/stock-movements" element={<StockMovementsPage />} />
          <Route path="/todos" element={<TodosPage />} />
        </Routes>
      </main>
      </div>
    </div>
  )
}

export default App
