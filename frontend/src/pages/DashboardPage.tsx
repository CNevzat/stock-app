import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { dashboardService } from '../services/dashboardService'
import { useSignalR } from '../hooks/useSignalR'
import { BarChart, Bar, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, AreaChart, Area, LabelList, ComposedChart, Line } from 'recharts'

export default function DashboardPage() {
  const [isDownloading, setIsDownloading] = useState(false)
  const formatCurrency = (value?: number | null) =>
    new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(
      Number(value ?? 0)
    )
  const formatCompactCurrency = (value?: number | null) =>
    new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', notation: 'compact', maximumFractionDigits: 1 }).format(
      Number(value ?? 0)
    )
  
  // SignalR bağlantısını başlat ve dashboard güncellemelerini dinle
  const { isConnected } = useSignalR()

  const handleDownloadPdf = async () => {
    setIsDownloading(true)
    try {
      // API base URL helper
      const getApiBaseUrl = () => {
        if (import.meta.env.VITE_API_BASE_URL) {
          return import.meta.env.VITE_API_BASE_URL;
        }
        if (import.meta.env.PROD) {
          return `http://${window.location.hostname}:5134`;
        }
        return 'http://localhost:5134';
      };
      const API_BASE_URL = getApiBaseUrl();
      const response = await fetch(`${API_BASE_URL}/api/reports/critical-stock/pdf`, {
        method: 'GET',
        headers: {
          'Accept': 'application/pdf',
        },
      })

      if (!response.ok) {
        throw new Error('PDF indirme hatası')
      }

      const blob = await response.blob()
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `kritik-stok-raporu-${new Date().toISOString().split('T')[0]}.pdf`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)
    } catch (error) {
      console.error('PDF indirme hatası:', error)
      alert('PDF indirilirken bir hata oluştu. Lütfen tekrar deneyin.')
    } finally {
      setIsDownloading(false)
    }
  }
  const { data: stats, isLoading } = useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: () => dashboardService.getStats(),
  })

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  if (!stats) {
    return <div className="text-center text-gray-500 py-10">Veri yüklenemedi</div>
  }

  // Stok dağılımı için renkler
  const COLORS = {
    'Stokta Var': '#10b981',
    'Düşük Stok': '#f59e0b',
    'Stokta Yok': '#ef4444',
  }



  return (
    <div className="fixed inset-0 top-[64px] overflow-y-auto bg-gray-50">
      <div className="px-12 lg:px-20 xl:px-32 py-6 w-full">
        {/* Header */}
        <div className="sm:flex sm:items-center sm:justify-between">
        <div className="sm:flex-auto">
          <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
          <p className="mt-2 text-sm text-gray-700">
            Stok yönetim sisteminizin genel durumu ve istatistikleri
          </p>
        </div>
        {/* SignalR Bağlantı Durumu */}
        <div className="mt-4 sm:mt-0 flex items-center gap-2">
          <div className={`w-2 h-2 rounded-full ${isConnected ? 'bg-green-500' : 'bg-red-500'}`}></div>
          <span className="text-sm text-gray-600">
            {isConnected ? 'Canlı' : 'Bağlantı Yok'}
          </span>
        </div>
      </div>

      {/* Önemli Metrikler */}
      <div className="mt-8 grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        {/* Toplam Ürün */}
        <div className="overflow-hidden rounded-xl bg-gradient-to-br from-blue-500 to-blue-600 shadow-lg">
          <div className="p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0 rounded-lg bg-white bg-opacity-30 p-3">
                <svg className="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                </svg>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-blue-100 truncate">Toplam Ürün</dt>
                  <dd className="flex items-baseline">
                    <div className="text-3xl font-bold text-white">{stats.totalProducts || 0}</div>
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        {/* Toplam Stok */}
        <div className="overflow-hidden rounded-xl bg-gradient-to-br from-green-500 to-green-600 shadow-lg">
          <div className="p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0 rounded-lg bg-white bg-opacity-30 p-3">
                <svg className="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-green-100 truncate">Toplam Stok</dt>
                  <dd className="flex items-baseline">
                    <div className="text-3xl font-bold text-white">{stats.totalStockQuantity || 0}</div>
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        {/* Düşük Stok */}
        <div className="overflow-hidden rounded-xl bg-gradient-to-br from-amber-500 to-amber-600 shadow-lg">
          <div className="p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0 rounded-lg bg-white bg-opacity-30 p-3">
                <svg className="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-amber-100 truncate">Düşük Stok</dt>
                  <dd className="flex items-baseline">
                    <div className="text-3xl font-bold text-white">{stats.lowStockProducts || 0}</div>
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>

        {/* Stok Yok */}
        <div className="overflow-hidden rounded-xl bg-gradient-to-br from-red-500 to-red-600 shadow-lg">
          <div className="p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0 rounded-lg bg-white bg-opacity-30 p-3">
                <svg className="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-red-100 truncate">Stok Yok</dt>
                  <dd className="flex items-baseline">
                    <div className="text-3xl font-bold text-white">{stats.outOfStockProducts || 0}</div>
                  </dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Finansal Genel Bakış */}
      <div className="mt-8 grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
        {[
          {
            title: 'Toplam Envanter Değeri',
            value: stats.totalInventoryCost,
            description: 'Stoktaki ürünlerin alış fiyatlarına göre toplam maliyeti',
            gradient: 'from-indigo-500 to-indigo-600',
            icon: (
              <svg className="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 1.12-3 2.5S10.343 13 12 13s3-1.12 3-2.5S13.657 8 12 8z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 10.5c0 2.485-2.462 4.5-5 4.5s-5-2.015-5-4.5" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v1M12 19v1M6.5 6.5l.707.707M16.793 16.793l.707.707M4 12h1M19 12h1" />
              </svg>
            ),
          },
          {
            title: 'Beklenen Toplam Satış',
            value: stats.totalExpectedSalesRevenue,
            description: 'Ürün satış fiyatı x stok adedi toplamı',
            gradient: 'from-emerald-500 to-emerald-600',
            icon: (
              <svg className="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v8m0 0l3-3m-3 3l-3-3" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 15a7 7 0 0114 0 7 7 0 01-14 0z" />
              </svg>
            ),
          },
          {
            title: 'Potansiyel Kar',
            value: stats.totalPotentialProfit,
            description: 'Satış sonrası elde edilebilecek toplam kar',
            gradient: 'from-purple-500 to-purple-600',
            icon: (
              <svg className="h-8 w-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7H7v6h6V7z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 11v6M15 17h4M17 3v4m0 0a4 4 0 110 8 4 4 0 110-8z" />
              </svg>
            ),
          },
        ].map((card) => (
          <div key={card.title} className={`overflow-hidden rounded-xl bg-gradient-to-br ${card.gradient} shadow-lg`}>
            <div className="p-6">
              <div className="flex items-center">
                <div className="flex-shrink-0 rounded-lg bg-white bg-opacity-30 p-3">
                  {card.icon}
                </div>
                <div className="ml-5 w-0 flex-1">
                  <dl>
                    <dt className="text-sm font-medium text-white/80 truncate">{card.title}</dt>
                    <dd className="flex items-baseline">
                      <div className="text-2xl font-bold text-white">{formatCurrency(card.value)}</div>
                    </dd>
                    <dd className="mt-2 text-xs text-white/70">{card.description}</dd>
                  </dl>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Grafikler - 3 Yan Yana */}
      <div className="mt-8 grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Kategori Bazlı Ürün Sayısı */}
        <div className="overflow-hidden rounded-xl bg-white/30 backdrop-blur-lg shadow-lg border border-white/20">
          <div className="p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Kategorilere Göre Ürün Çeşitliliği</h3>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={stats.categoryStats || []}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                <XAxis dataKey="categoryName" tick={{ fontSize: 11 }} />
                <YAxis />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#fff',
                    border: '1px solid #e5e7eb',
                    borderRadius: '0.5rem',
                  }}
                  formatter={(value: any, _name: any, props: any) => {
                    const categoryName = props.payload.categoryName;
                    const isOther = categoryName === 'Diğer';
                    const tooltipText = isOther 
                      ? `${value} Çeşit Ürün (Toplam ${stats.totalCategories || 0} kategoriden kalanlar birleştirildi)`
                      : `${value} Çeşit Ürün`;
                    return [tooltipText, categoryName];
                  }}
                  labelFormatter={(label) => `Kategori: ${label}`}
                />
                <Legend />
                <Bar dataKey="productCount" name="Ürün Çeşit Sayısı" radius={[8, 8, 0, 0]}>
                  {stats.categoryStats?.map((entry, index) => (
                    <Cell 
                      key={`cell-${index}`} 
                      fill={entry.categoryName === 'Diğer' ? '#94a3b8' : '#8b5cf6'} 
                    />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Stok Dağılımı */}
        <div className="overflow-hidden rounded-xl bg-white/30 backdrop-blur-lg shadow-lg border border-white/20">
          <div className="p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Stok Durumu Dağılımı</h3>
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={(stats.stockDistribution || []) as any}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={false}
                  outerRadius={100}
                  fill="#8884d8"
                  dataKey="count"
                >
                  {stats.stockDistribution?.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[entry.status as keyof typeof COLORS]} />
                  ))}
                </Pie>
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#fff',
                    border: '1px solid #e5e7eb',
                    borderRadius: '0.5rem',
                  }}
                  formatter={(value: any, _name: any, props: any) => {
                    const percentage = props.payload.percentage || 0;
                    return [`${value} Ürün (%${percentage})`, props.payload.status];
                  }}
                />
                <Legend
                  verticalAlign="top"
                  align="right"
                  layout="vertical"
                  formatter={(value: any, entry: any) => {
                    const percentage = entry.payload?.percentage || 0;
                    const status = entry.payload?.status || '';
                    // Backend'den gelen status değerlerini Türkçe'ye çevir
                    let statusText = '';
                    if (status === 'In Stock' || status === 'Stokta Var') {
                      statusText = 'Stok Var';
                    } else if (status === 'Low Stock' || status === 'Düşük Stok') {
                      statusText = 'Stok Düşük';
                    } else if (status === 'Out of Stock' || status === 'Stokta Yok') {
                      statusText = 'Stok Yok';
                    } else {
                      statusText = status || value;
                    }
                    return `${statusText}: %${percentage.toFixed(1)}`;
                  }}
                  wrapperStyle={{
                    paddingLeft: '20px',
                    paddingTop: '0px',
                  }}
                  iconType="circle"
                />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Kategori Bazlı Stok Miktarı - Yatay Bar Chart */}
        <div className="overflow-hidden rounded-xl bg-white/30 backdrop-blur-lg shadow-lg border border-white/20">
          <div className="p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Kategorilere Göre Toplam Stok</h3>
            <ResponsiveContainer width="100%" height={400}>
              <BarChart 
                data={stats.categoryStats || []} 
                layout="vertical"
                margin={{ top: 5, right: 150, left: 80, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                <XAxis type="number" />
                <YAxis 
                  type="category" 
                  dataKey="categoryName" 
                  width={70}
                  tick={{ fontSize: 12 }}
                />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#fff',
                    border: '1px solid #e5e7eb',
                    borderRadius: '0.5rem',
                    whiteSpace: 'normal',
                    maxWidth: '300px',
                    wordWrap: 'break-word',
                  }}
                  formatter={(value: any, _name: any, props: any) => {
                    const categoryName = props.payload.categoryName;
                    const isOther = categoryName === 'Diğer';
                    const tooltipText = isOther 
                      ? `${value} Adet (Toplam ${stats.totalCategories || 0} kategoriden kalanlar birleştirildi)`
                      : `${value} Adet`;
                    return [tooltipText, categoryName];
                  }}
                  labelFormatter={(label) => `Kategori: ${label}`}
                  wrapperStyle={{ zIndex: 1000 }}
                />
                <Legend />
                <Bar 
                  dataKey="totalStock" 
                  name="Toplam Stok Miktarı" 
                  radius={[0, 8, 8, 0]}
                >
                  {stats.categoryStats?.map((entry, index) => (
                    <Cell 
                      key={`cell-${index}`} 
                      fill={entry.categoryName === 'Diğer' ? '#94a3b8' : '#3b82f6'} 
                    />
                  ))}
                  <LabelList 
                    dataKey="totalStock" 
                    position="right" 
                    formatter={(value: any) => `${value} Adet`}
                    style={{ fontSize: '11px', fill: '#374151', fontWeight: 500 }}
                  />
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>

      {/* Kategori Bazlı Finansal Değerler */}
      {stats.categoryValueDistribution && stats.categoryValueDistribution.length > 0 && (
        <div className="mt-8">
          <div className="overflow-hidden rounded-xl bg-white/30 backdrop-blur-lg shadow-lg border border-white/20">
            <div className="p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-gray-900">Kategorilere Göre Envanter Değeri ve Beklenen Kar</h3>
                <span className="text-xs uppercase tracking-wide text-gray-500">
                  {stats.categoryValueDistribution.length} kategori
                </span>
              </div>
              <ResponsiveContainer width="100%" height={380}>
                <ComposedChart
                  data={stats.categoryValueDistribution.map((category) => ({
                    categoryName: category.categoryName,
                    totalCost: Number(category.totalCost ?? 0),
                    totalRevenue: Number(category.totalPotentialRevenue ?? 0),
                    totalProfit: Number(category.totalPotentialProfit ?? 0),
                  }))}
                  margin={{ top: 10, right: 40, left: 10, bottom: 40 }}
                >
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                  <XAxis dataKey="categoryName" tick={{ fontSize: 11 }} interval={0} angle={-25} textAnchor="end" height={70} />
                  <YAxis tickFormatter={(value) => formatCompactCurrency(value)} />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: '#fff',
                      border: '1px solid #e5e7eb',
                      borderRadius: '0.5rem',
                    }}
                    formatter={(value: number, name: string, props: any) => {
                      const dataKey = props?.dataKey ?? name
                      const label =
                        dataKey === 'totalCost'
                          ? 'Envanter Maliyeti'
                          : dataKey === 'totalRevenue'
                          ? 'Beklenen Satış'
                          : 'Beklenen Kar'
                      return [formatCurrency(value), label]
                    }}
                    labelFormatter={(label) => `Kategori: ${label}`}
                  />
                  <Legend
                    formatter={(_value, entry) => {
                      const dataKey = entry?.dataKey ?? entry?.value
                      return dataKey === 'totalCost'
                        ? 'Envanter Maliyeti'
                        : dataKey === 'totalRevenue'
                        ? 'Beklenen Satış'
                        : 'Beklenen Kar'
                    }}
                  />
                  <Bar
                    dataKey="totalCost"
                    name="Envanter Maliyeti"
                    barSize={22}
                    radius={[4, 4, 0, 0]}
                    fill="#3b82f6"
                  />
                  <Bar
                    dataKey="totalRevenue"
                    name="Beklenen Satış"
                    barSize={22}
                    radius={[4, 4, 0, 0]}
                    fill="#10b981"
                  />
                  <Line
                    type="monotone"
                    dataKey="totalProfit"
                    name="Beklenen Kar"
                    stroke="#a855f7"
                    strokeWidth={3}
                    dot={{ r: 5 }}
                    activeDot={{ r: 7 }}
                  />
                </ComposedChart>
              </ResponsiveContainer>
            </div>
          </div>
        </div>
      )}

      {/* Kritik Stok Uyarıları - Tam Genişlik */}
      <div className="mt-8 overflow-hidden rounded-xl backdrop-blur-lg shadow-lg border border-white/10">
        <div className="p-6">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold text-gray-900">🚨 Kritik Stok Uyarıları</h3>
            <button
              onClick={handleDownloadPdf}
              disabled={isDownloading}
              className="inline-flex items-center gap-x-2 rounded-lg bg-red-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-red-500 transition-all duration-200 hover:shadow-md disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isDownloading ? (
                <>
                  <svg className="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  İndiriliyor...
                </>
              ) : (
                <>
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                  PDF İndir
                </>
              )}
            </button>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200/20">
              <thead className="backdrop-blur-md">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider whitespace-nowrap min-w-[150px]">
                    Ürün Adı
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider whitespace-nowrap min-w-[120px]">
                    Stok Kodu
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider whitespace-nowrap min-w-[120px]">
                    Kategori
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider whitespace-nowrap">
                    Stok Miktarı
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider whitespace-nowrap">
                    Durum
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200/20 backdrop-blur-md">
                {stats.productStockStatus?.map((product) => (
                  <tr key={product.productId} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">{product.productName}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm font-mono text-gray-600">{product.stockCode}</span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm text-gray-900">{product.categoryName}</span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm font-semibold text-gray-900">{product.stockQuantity}</span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span
                        className={`inline-flex items-center px-3 py-1 rounded-full text-xs font-medium ${
                          product.status === 'Stokta Var'
                            ? 'bg-green-100 text-green-800'
                            : product.status === 'Düşük Stok'
                            ? 'bg-yellow-100 text-yellow-800'
                            : 'bg-red-100 text-red-800'
                        }`}
                      >
                        {product.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Stok Hareketleri Trend Grafiği */}
      <div className="mt-8">
        <div className="sm:flex sm:items-center mb-4">
          <h2 className="text-xl font-semibold text-gray-900">Stok Hareketleri Trend Analizi</h2>
        </div>
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          {/* Area Chart - Son 1 Ay (Günlük) */}
          <div className="overflow-hidden rounded-xl bg-white/30 backdrop-blur-lg shadow-lg border border-white/20">
            <div className="p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Son 1 Ay - Günlük Trend</h3>
              <ResponsiveContainer width="100%" height={350}>
                <AreaChart data={stats.stockMovementTrend || []}>
                  <defs>
                    <linearGradient id="colorStockIn" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#10b981" stopOpacity={0.8}/>
                      <stop offset="95%" stopColor="#10b981" stopOpacity={0.1}/>
                    </linearGradient>
                    <linearGradient id="colorStockOut" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#ef4444" stopOpacity={0.8}/>
                      <stop offset="95%" stopColor="#ef4444" stopOpacity={0.1}/>
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                  <XAxis 
                    dataKey="dateLabel" 
                    tick={{ fontSize: 10 }}
                    angle={-45}
                    textAnchor="end"
                    height={80}
                    interval="preserveStartEnd"
                  />
                  <YAxis />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: '#fff',
                      border: '1px solid #e5e7eb',
                      borderRadius: '0.5rem',
                    }}
                    formatter={(value: any, name: any) => {
                      const label = name === 'stockIn' ? 'Giriş' : 'Çıkış';
                      return [`${value} Adet`, label];
                    }}
                    labelFormatter={(label) => `Tarih: ${label}`}
                  />
                  <Legend 
                    formatter={(value) => value === 'stockIn' ? 'Giriş' : 'Çıkış'}
                  />
                  <Area 
                    type="monotone" 
                    dataKey="stockIn" 
                    name="stockIn"
                    stroke="#10b981" 
                    fillOpacity={1}
                    fill="url(#colorStockIn)"
                    strokeWidth={2}
                    dot={{ fill: '#10b981', r: 3 }}
                    activeDot={{ r: 5 }}
                  />
                  <Area 
                    type="monotone" 
                    dataKey="stockOut" 
                    name="stockOut"
                    stroke="#ef4444" 
                    fillOpacity={1}
                    fill="url(#colorStockOut)"
                    strokeWidth={2}
                    dot={{ fill: '#ef4444', r: 3 }}
                    activeDot={{ r: 5 }}
                  />
                </AreaChart>
              </ResponsiveContainer>
            </div>
          </div>

          {/* Area Chart - Son 1 Yıl (Aylık) */}
          <div className="overflow-hidden rounded-xl bg-white/30 backdrop-blur-lg shadow-lg border border-white/20">
            <div className="p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Son 1 Yıl - Aylık Trend</h3>
              <ResponsiveContainer width="100%" height={350}>
                <AreaChart data={stats.lastYearStockMovementTrend || []}>
                  <defs>
                    <linearGradient id="colorStockInAllTime" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#10b981" stopOpacity={0.8}/>
                      <stop offset="95%" stopColor="#10b981" stopOpacity={0.1}/>
                    </linearGradient>
                    <linearGradient id="colorStockOutAllTime" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#ef4444" stopOpacity={0.8}/>
                      <stop offset="95%" stopColor="#ef4444" stopOpacity={0.1}/>
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                  <XAxis 
                    dataKey="dateLabel" 
                    tick={{ fontSize: 10 }}
                    angle={-45}
                    textAnchor="end"
                    height={80}
                    interval="preserveStartEnd"
                  />
                  <YAxis />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: '#fff',
                      border: '1px solid #e5e7eb',
                      borderRadius: '0.5rem',
                    }}
                    formatter={(value: any, name: any) => {
                      const label = name === 'stockIn' ? 'Giriş' : 'Çıkış';
                      return [`${value} Adet`, label];
                    }}
                    labelFormatter={(label) => `Ay: ${label}`}
                  />
                  <Legend 
                    formatter={(value) => value === 'stockIn' ? 'Giriş' : 'Çıkış'}
                  />
                  <Area 
                    type="monotone" 
                    dataKey="stockIn" 
                    name="stockIn"
                    stroke="#10b981" 
                    fillOpacity={1} 
                    fill="url(#colorStockInAllTime)" 
                  />
                  <Area 
                    type="monotone" 
                    dataKey="stockOut" 
                    name="stockOut"
                    stroke="#ef4444" 
                    fillOpacity={1} 
                    fill="url(#colorStockOutAllTime)" 
                  />
                </AreaChart>
              </ResponsiveContainer>
            </div>
          </div>
        </div>
      </div>

      </div>
    </div>
  )
}

