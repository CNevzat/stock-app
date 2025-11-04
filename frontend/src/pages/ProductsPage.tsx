import { useState, useEffect, useRef } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useSearchParams } from 'react-router-dom'
import { productService } from '../services/productService'
import { categoryService } from '../services/categoryService'
import { locationService } from '../services/locationService'
import { stockMovementService } from '../services/stockMovementService'
import { signalRService } from '../services/signalRService'
import type {CreateProductCommand, UpdateProductCommand} from "../Api";

// Kategori renklerini belirle
const getCategoryColor = (categoryId: number) => {
  const colors = [
    { bg: 'bg-purple-50', text: 'text-purple-700', ring: 'ring-purple-600/20' },
    { bg: 'bg-blue-50', text: 'text-blue-700', ring: 'ring-blue-600/20' },
    { bg: 'bg-green-50', text: 'text-green-700', ring: 'ring-green-600/20' },
    { bg: 'bg-yellow-50', text: 'text-yellow-700', ring: 'ring-yellow-600/20' },
    { bg: 'bg-red-50', text: 'text-red-700', ring: 'ring-red-600/20' },
    { bg: 'bg-pink-50', text: 'text-pink-700', ring: 'ring-pink-600/20' },
    { bg: 'bg-indigo-50', text: 'text-indigo-700', ring: 'ring-indigo-600/20' },
    { bg: 'bg-cyan-50', text: 'text-cyan-700', ring: 'ring-cyan-600/20' },
    { bg: 'bg-orange-50', text: 'text-orange-700', ring: 'ring-orange-600/20' },
    { bg: 'bg-teal-50', text: 'text-teal-700', ring: 'ring-teal-600/20' },
  ];
  return colors[categoryId % colors.length];
};

export default function ProductsPage() {
  const queryClient = useQueryClient()
  const [searchParams] = useSearchParams()
  const [page, setPage] = useState(1)
  const [pageSize] = useState(10)
  const [searchInput, setSearchInput] = useState('')
  const [searchTerm, setSearchTerm] = useState('')
  const [categoryFilter, setCategoryFilter] = useState<number | undefined>()
  const [locationFilter, setLocationFilter] = useState<number | undefined>()
  const [locationSearchInput, setLocationSearchInput] = useState('')
  const [showLocationDropdown, setShowLocationDropdown] = useState(false)
  const [showLocationDropdownInModal, setShowLocationDropdownInModal] = useState(false)
  const [locationSearchInputInModal, setLocationSearchInputInModal] = useState('')
  const searchInputRef = useRef<HTMLInputElement>(null)

  // URL parametrelerinden categoryId ve locationId'yi oku
  useEffect(() => {
    const categoryIdParam = searchParams.get('categoryId')
    if (categoryIdParam) {
      setCategoryFilter(Number(categoryIdParam))
    }
    const locationIdParam = searchParams.get('locationId')
    if (locationIdParam) {
      setLocationFilter(Number(locationIdParam))
    }
  }, [searchParams])

  // Debounce search input - Preserve focus and cursor position
  useEffect(() => {
    const timer = setTimeout(() => {
      const input = searchInputRef.current
      const wasFocused = document.activeElement === input
      const cursorPosition = input?.selectionStart ?? null
      
      setSearchTerm(searchInput)
      setPage((prevPage) => prevPage !== 1 ? 1 : prevPage)
      
      // Restore focus and cursor position if input was focused
      if (wasFocused && input) {
        // Use requestAnimationFrame to ensure DOM is updated
        requestAnimationFrame(() => {
          input.focus()
          if (cursorPosition !== null && cursorPosition <= (input.value?.length ?? 0)) {
            input.setSelectionRange(cursorPosition, cursorPosition)
          }
        })
      }
    }, 800) // Increased debounce time to 800ms for better UX

    return () => clearTimeout(timer)
  }, [searchInput])

  // Close location dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as HTMLElement
      if (!target.closest('.location-dropdown-container')) {
        setShowLocationDropdown(false)
        setShowLocationDropdownInModal(false)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => {
      document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [])

  // SignalR - Real-time updates için event listener'lar
  useEffect(() => {
    // SignalR bağlantısını başlat
    signalRService.startConnection().catch((error) => {
      console.error('SignalR bağlantı hatası:', error)
    })

    // Product Created event
    const handleProductCreated = () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      queryClient.invalidateQueries({ queryKey: ['categories'] }) // Product count için
    }

    // Product Updated event
    const handleProductUpdated = () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      queryClient.invalidateQueries({ queryKey: ['categories'] }) // Product count için
    }

    // Product Deleted event
    const handleProductDeleted = () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      queryClient.invalidateQueries({ queryKey: ['categories'] }) // Product count için
    }

    // Event listener'ları kaydet
    signalRService.onProductCreated(handleProductCreated)
    signalRService.onProductUpdated(handleProductUpdated)
    signalRService.onProductDeleted(handleProductDeleted)

    // Cleanup
    return () => {
      signalRService.offProductCreated(handleProductCreated)
      signalRService.offProductUpdated(handleProductUpdated)
      signalRService.offProductDeleted(handleProductDeleted)
    }
  }, [queryClient])
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [isEditModalOpen, setIsEditModalOpen] = useState(false)
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false)
  const [selectedProductId, setSelectedProductId] = useState<number | null>(null)
  const [detailProductId, setDetailProductId] = useState<number | null>(null)
  const [stockMovementPage, setStockMovementPage] = useState(1)
  const [stockMovementPageSize] = useState(10)
  const [isStockMovementModalOpen, setIsStockMovementModalOpen] = useState(false)
  const [stockMovementFormData, setStockMovementFormData] = useState({
    productId: 0,
    type: 1, // 1 = Giriş, 2 = Çıkış
    quantity: 0,
    description: '',
  })
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    stockQuantity: 0,
    lowStockThreshold: 5,
    categoryId: 0,
    locationId: undefined as number | undefined,
  })
  const [selectedImage, setSelectedImage] = useState<File | null>(null)
  const [imagePreview, setImagePreview] = useState<string | null | undefined>(null)
  const [isExporting, setIsExporting] = useState(false)

  // Fetch products
  const { data: productsData, isLoading } = useQuery({
    queryKey: ['products', page, pageSize, searchTerm, categoryFilter, locationFilter],
    queryFn: () =>
      productService.getAll({
        pageNumber: page,
        pageSize,
        searchTerm: searchTerm || undefined,
        categoryId: categoryFilter,
        locationId: locationFilter,
      }),
  })

  // Fetch categories for dropdown
  const { data: categoriesData } = useQuery({
    queryKey: ['categories', 1, 100],
    queryFn: () => categoryService.getAll({ pageNumber: 1, pageSize: 100 }),
  })

  // Fetch locations for dropdown
  const { data: locationsData } = useQuery({
    queryKey: ['locations', 1, 100],
    queryFn: () => locationService.getAll({ pageNumber: 1, pageSize: 100 }),
  })

  // Fetch stock movements for detail modal
  const { data: stockMovementsData } = useQuery({
    queryKey: ['stock-movements-detail', detailProductId, stockMovementPage],
    queryFn: () => stockMovementService.getAll({
      pageNumber: stockMovementPage,
      pageSize: stockMovementPageSize,
      productId: detailProductId || undefined,
    }),
    enabled: !!detailProductId,
  })

  // Create mutation
  const createMutation = useMutation({
    mutationFn: (dto: CreateProductCommand & { image?: File }) => productService.create(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      setIsCreateModalOpen(false)
      resetForm()
    },
  })

  // Update mutation
  const updateMutation = useMutation({
    mutationFn: ({dto }: { id: number; dto: UpdateProductCommand & { image?: File } }) =>
      productService.update(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      setIsEditModalOpen(false)
      resetForm()
    },
  })

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: (id: number) => productService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
    },
  })

  // Stock Movement mutation for product detail
  const stockMovementMutation = useMutation({
    mutationFn: (data: typeof stockMovementFormData) => stockMovementService.create(data),
    onSuccess: () => {
      // Tüm ilgili query'leri invalidate et
      queryClient.invalidateQueries({ queryKey: ['stock-movements'] })
      queryClient.invalidateQueries({ queryKey: ['stock-movements-detail'] })
      queryClient.invalidateQueries({ queryKey: ['products'] })
      queryClient.invalidateQueries({ queryKey: ['products-all'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
      setIsStockMovementModalOpen(false)
      setStockMovementFormData({ productId: detailProductId || 0, type: 1, quantity: 0, description: '' })
    },
    onError: (error: any) => {
      const errorMessage = error?.message || error?.response?.data?.message || 'Bir hata oluştu!'
      alert(errorMessage)
    },
  })

  const resetForm = () => {
    setFormData({ name: '', description: '', stockQuantity: 0, lowStockThreshold: 5, categoryId: 0, locationId: undefined })
    setSelectedImage(null)
    setImagePreview(null)
    setSelectedProductId(null)
    setLocationSearchInputInModal('')
    setShowLocationDropdownInModal(false)
  }

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (file) {
      setSelectedImage(file)
      // Preview oluştur
      const reader = new FileReader()
      reader.onloadend = () => {
        setImagePreview(reader.result as string)
      }
      reader.readAsDataURL(file)
    }
  }

  const handleCreate = (e: React.FormEvent) => {
    e.preventDefault()
    createMutation.mutate({ ...formData, image: selectedImage || undefined } as any)
  }

  const handleUpdate = (e: React.FormEvent) => {
    e.preventDefault()
    if (selectedProductId) {
      updateMutation.mutate({
        id: selectedProductId,
        dto: {
          id: selectedProductId,
          name: formData.name,
          description: formData.description,
          lowStockThreshold: formData.lowStockThreshold,
          locationId: formData.locationId,
          image: selectedImage || undefined,
        } as any,
      })
    }
  }

  const openEditModal = (product: any) => {
    setSelectedProductId(product.id)
    setFormData({
      name: product.name,
      description: product.description,
      stockQuantity: 0, // Artık kullanılmıyor, backend'e gönderilmiyor
      lowStockThreshold: product.lowStockThreshold || 5,
      categoryId: 0,
      locationId: product.locationId || undefined,
    })
    // Mevcut resmi göster
    setSelectedImage(null)
    const imagePath = (product as any).imagePath
    if (imagePath) {
      setImagePreview(`http://localhost:5134${imagePath}`)
    } else {
      setImagePreview(null)
    }
    // Lokasyon arama input'unu ayarla
    if (product.locationId && locationsData?.items) {
      const selectedLocation = locationsData.items.find((loc: any) => loc.id === product.locationId)
      setLocationSearchInputInModal(selectedLocation?.name || '')
    } else {
      setLocationSearchInputInModal('')
    }
    setShowLocationDropdownInModal(false)
    setIsEditModalOpen(true)
  }

  const openDetailModal = (product: any) => {
    setDetailProductId(product.id)
    setIsDetailModalOpen(true)
  }

  if (isLoading) {
    return <div className="flex justify-center items-center h-64">Yükleniyor...</div>
  }

  return (
    <div className="px-4 sm:px-6 lg:px-8">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-3xl font-semibold text-gray-900">Ürünler</h1>
          <p className="mt-2 text-sm text-gray-700">
            Stokta bulunan tüm ürünlerin listesi.
          </p>
        </div>
            <div className="mt-4 sm:ml-16 sm:mt-0 sm:flex-none flex gap-2">
              <button
                onClick={async () => {
                  setIsExporting(true)
                  try {
                    await productService.exportExcel()
                  } catch (error: any) {
                    alert(error?.message || 'Excel export sırasında bir hata oluştu!')
                  } finally {
                    setIsExporting(false)
                  }
                }}
                disabled={isExporting}
                className="inline-flex items-center gap-x-2 rounded-lg bg-gradient-to-r from-green-600 to-green-700 px-4 py-2.5 text-sm font-semibold text-white shadow-lg hover:from-green-700 hover:to-green-800 hover:shadow-xl transform hover:scale-105 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                {isExporting ? 'Excel Oluşturuluyor...' : 'Excel\'e Aktar'}
              </button>
              <button
                onClick={() => {
                  resetForm()
                  setIsCreateModalOpen(true)
                }}
                className="inline-flex items-center gap-x-2 rounded-lg bg-gradient-to-r from-blue-600 to-blue-700 px-4 py-2.5 text-sm font-semibold text-white shadow-lg hover:from-blue-700 hover:to-blue-800 hover:shadow-xl transform hover:scale-105 transition-all duration-200"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
                Yeni Ürün Ekle
              </button>
            </div>
      </div>

      {/* Filters */}
      <div className="mt-4 flex gap-4">
        <input
          ref={searchInputRef}
          type="text"
          placeholder="Ürün, stok kodu, açıklama veya lokasyon ara..."
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          className="block rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
        />
        <select
          value={categoryFilter || ''}
          onChange={(e) => {
            setCategoryFilter(e.target.value ? Number(e.target.value) : undefined)
            setPage(1)
          }}
          className="block rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
        >
          <option value="">Tüm Kategoriler</option>
          {categoriesData?.items?.map((cat) => (
            <option key={cat.id} value={cat.id}>
              {cat.name}
            </option>
          ))}
        </select>
        <div className="relative location-dropdown-container">
          <input
            type="text"
            placeholder="Lokasyon ara..."
            value={locationFilter ? locationsData?.items?.find((loc: any) => loc.id === locationFilter)?.name || '' : locationSearchInput}
            onChange={(e) => {
              setLocationSearchInput(e.target.value)
              setShowLocationDropdown(true)
              if (locationFilter) {
                setLocationFilter(undefined)
              }
            }}
            onFocus={() => setShowLocationDropdown(true)}
            className="block rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
          />
          <div className="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
            <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          {showLocationDropdown && (
            <div className="absolute z-10 mt-1 w-full bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
              <button
                type="button"
                onClick={() => {
                  setLocationFilter(undefined)
                  setLocationSearchInput('')
                  setShowLocationDropdown(false)
                }}
                className="w-full text-left px-4 py-3 hover:bg-gray-50 focus:bg-gray-50 focus:outline-none transition-colors border-b border-gray-100"
              >
                <div className="font-medium text-gray-900">Tüm Lokasyonlar</div>
              </button>
              {locationsData?.items
                ?.filter((loc: any) => 
                  loc.name?.toLowerCase().includes(locationSearchInput.toLowerCase())
                )
                .length === 0 ? (
                  <div className="px-4 py-3 text-sm text-gray-500">
                    Lokasyon bulunamadı
                  </div>
                ) : (
                  locationsData?.items
                    ?.filter((loc: any) => 
                      loc.name?.toLowerCase().includes(locationSearchInput.toLowerCase())
                    )
                    .map((loc: any) => (
                      <button
                        key={loc.id}
                        type="button"
                        onClick={() => {
                          setLocationFilter(loc.id)
                          setLocationSearchInput(loc.name)
                          setShowLocationDropdown(false)
                          setPage(1)
                        }}
                        className="w-full text-left px-4 py-3 hover:bg-gray-50 focus:bg-gray-50 focus:outline-none transition-colors border-b border-gray-100 last:border-b-0"
                      >
                        <div className="font-medium text-gray-900">{loc.name}</div>
                        {loc.description && (
                          <div className="text-sm text-gray-500">{loc.description}</div>
                        )}
                      </button>
                    ))
                )}
            </div>
          )}
        </div>
      </div>

      <div className="mt-8 flow-root">
        <div className="rounded-lg shadow-sm border border-gray-200 bg-white overflow-hidden">
          <div className="px-4 sm:px-6 lg:px-8 py-2">
            <table className="w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="py-3.5 pl-2 pr-2 text-left text-sm font-semibold text-gray-900 whitespace-nowrap w-[50px]">
                    #
                  </th>
                  <th className="px-2 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[280px] max-w-[280px]">
                    İsim
                  </th>
                  <th className="px-2 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap w-[100px]">
                    Stok Kodu
                  </th>
                  <th className="px-1 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap w-[60px]">
                    Stok
                  </th>
                  <th className="px-2 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[100px] max-w-[100px]">
                    Kategori
                  </th>
                  <th className="px-2 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[100px] max-w-[100px]">
                    Lokasyon
                  </th>
                  <th className="relative py-3.5 pl-2 pr-2 text-right text-sm font-semibold text-gray-900 whitespace-nowrap w-[210px]">
                    <span className="sr-only">İşlemler</span>
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 bg-white">
                {productsData?.items?.map((product, index) => (
                  <tr key={product.id} className="hover:bg-gray-50 transition-colors duration-150">
                    <td className="whitespace-nowrap py-4 pl-2 pr-2 text-sm font-medium text-gray-900">
                      <span className="inline-flex items-center justify-center w-7 h-7 rounded-full bg-gray-100 text-gray-600 font-semibold text-xs">
                        {(page - 1) * pageSize + index + 1}
                      </span>
                    </td>
                    <td className="px-2 py-4 text-sm font-medium text-gray-900 min-w-[280px] max-w-[280px]">
                      <div className="flex items-center gap-2">
                        {(product as any).imagePath && (
                          <img
                            src={`http://localhost:5134${(product as any).imagePath || ''}`}
                            alt={product.name}
                            className="h-8 w-8 object-cover rounded flex-shrink-0"
                          />
                        )}
                        <div className="truncate">{product.name}</div>
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-2 py-4 text-sm w-[100px]">
                      <div className="truncate">
                        <span className="inline-flex items-center rounded-md bg-blue-50 px-2 py-0.5 text-xs font-mono font-semibold text-gray-900 ring-1 ring-inset ring-blue-600/20">
                          {product.stockCode}
                        </span>
                      </div>
                    </td>
                    <td className="whitespace-nowrap px-1 py-4 text-sm text-center w-[60px]">
                      <span className="text-gray-900 font-medium">
                        {product.stockQuantity ?? 0}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-2 py-4 text-sm min-w-[100px] max-w-[100px]">
                      {(() => {
                        const color = getCategoryColor(product.categoryId || 0);
                        return (
                          <span className={`inline-flex items-center rounded-md px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${color.bg} ${color.text} ${color.ring} truncate`}>
                            {product.categoryName}
                          </span>
                        );
                      })()}
                    </td>
                    <td className="whitespace-nowrap px-2 py-4 text-sm min-w-[100px] max-w-[100px]">
                      {(product as any).locationName ? (
                        <span className="inline-flex items-center rounded-md px-2 py-0.5 text-xs font-medium ring-1 ring-inset bg-gray-50 text-gray-700 ring-gray-600/20 truncate">
                          <svg className="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                          </svg>
                          {(product as any).locationName}
                        </span>
                      ) : (
                        <span className="text-gray-400 text-xs">-</span>
                      )}
                    </td>
                    <td className="relative whitespace-nowrap py-4 pl-2 pr-2 text-right text-sm font-medium w-[210px]">
                      <div className="flex justify-end gap-1">
                        <button
                          onClick={() => openDetailModal(product)}
                          className="inline-flex items-center gap-x-1 rounded-lg bg-green-600 px-2 py-1 text-xs font-semibold text-white shadow-sm hover:bg-green-500 transition-all duration-200"
                        >
                          <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                          </svg>
                          Detay
                        </button>
                        <button
                          onClick={() => openEditModal(product)}
                          className="inline-flex items-center gap-x-1 rounded-lg bg-blue-600 px-2 py-1 text-xs font-semibold text-white shadow-sm hover:bg-blue-500 transition-all duration-200"
                        >
                          <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                          </svg>
                          Düzenle
                        </button>
                        <button
                          onClick={() => {
                            if (product.id && confirm('Silmek istediğinize emin misiniz?')) {
                              deleteMutation.mutate(product.id)
                            }
                          }}
                          className="inline-flex items-center gap-x-1 rounded-lg bg-red-600 px-2 py-1 text-xs font-semibold text-white shadow-sm hover:bg-red-500 transition-all duration-200"
                        >
                          <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                          </svg>
                          Sil
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Pagination */}
      {productsData && (productsData.totalPages ?? 0) > 1 && (
        <div className="flex items-center justify-between border-t border-gray-200 bg-gray-50 px-4 py-3 sm:px-6 mt-4 rounded-lg">
          <div className="flex flex-1 justify-between sm:hidden">
            <button
              onClick={() => setPage(page - 1)}
              disabled={!productsData.hasPreviousPage}
              className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Önceki
            </button>
            <button
              onClick={() => setPage(page + 1)}
              disabled={!productsData.hasNextPage}
              className="relative ml-3 inline-flex items-center rounded-md border border-white/50 bg-white/70 backdrop-blur-md px-4 py-2 text-sm font-medium text-gray-900 hover:bg-white/80 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Sonraki
            </button>
          </div>
          <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
            <div>
              <p className="text-sm text-gray-800">
                <span className="font-medium">{productsData.pageNumber}</span> / <span className="font-medium">{productsData.totalPages}</span> sayfa gösteriliyor
              </p>
            </div>
            <div>
              <nav className="isolate inline-flex -space-x-px rounded-md shadow-sm">
                <button
                  onClick={() => setPage(page - 1)}
                  disabled={!productsData.hasPreviousPage}
                  className="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-700 ring-1 ring-inset ring-gray-300 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Önceki
                </button>
                <button
                  onClick={() => setPage(page + 1)}
                  disabled={!productsData.hasNextPage}
                  className="relative inline-flex items-center rounded-r-md px-2 py-2 text-gray-700 ring-1 ring-inset ring-gray-300 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Sonraki
                </button>
              </nav>
            </div>
          </div>
        </div>
      )}

      {/* Create Modal */}
      {isCreateModalOpen && (
        <div className="fixed inset-0 z-10 overflow-y-auto">
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={() => {
              setIsCreateModalOpen(false)
              resetForm()
            }} />
            <div className="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6">
              <form onSubmit={handleCreate}>
                <div>
                  <h3 className="text-lg font-semibold leading-6 text-gray-900 mb-4">
                    Ürün Oluştur
                  </h3>
                  <div className="space-y-4">
                    <div>
                      <label htmlFor="name" className="block text-sm font-medium text-gray-700">
                        İsim
                      </label>
                      <input
                        type="text"
                        id="name"
                        required
                        value={formData.name}
                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="description" className="block text-sm font-medium text-gray-700">
                        Açıklama
                      </label>
                      <textarea
                        id="description"
                        required
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="stockQuantity" className="block text-sm font-medium text-gray-700">
                        Stok Miktarı
                      </label>
                      <input
                        type="text"
                        inputMode="numeric"
                        id="stockQuantity"
                        required
                        value={formData.stockQuantity === 0 ? '' : formData.stockQuantity}
                        onChange={(e) => {
                          const value = e.target.value.replace(/[^0-9]/g, '')
                          if (value === '') {
                            setFormData({ ...formData, stockQuantity: 0 })
                          } else {
                            setFormData({ ...formData, stockQuantity: parseInt(value, 10) || 0 })
                          }
                        }}
                        onBlur={(e) => {
                          if (e.target.value === '') {
                            setFormData({ ...formData, stockQuantity: 0 })
                          }
                        }}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="lowStockThreshold" className="block text-sm font-medium text-gray-700">
                        Düşük Stok Eşiği
                      </label>
                      <input
                        type="text"
                        inputMode="numeric"
                        id="lowStockThreshold"
                        required
                        value={formData.lowStockThreshold === 0 ? '' : formData.lowStockThreshold}
                        onChange={(e) => {
                          const value = e.target.value.replace(/[^0-9]/g, '')
                          if (value === '') {
                            setFormData({ ...formData, lowStockThreshold: 0 })
                          } else {
                            setFormData({ ...formData, lowStockThreshold: parseInt(value, 10) || 0 })
                          }
                        }}
                        onBlur={(e) => {
                          if (e.target.value === '') {
                            setFormData({ ...formData, lowStockThreshold: 5 })
                          }
                        }}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="categoryId" className="block text-sm font-medium text-gray-700">
                        Kategori
                      </label>
                      <select
                        id="categoryId"
                        required
                        value={formData.categoryId}
                        onChange={(e) => setFormData({ ...formData, categoryId: Number(e.target.value) })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      >
                        <option value="">Kategori Seçin</option>
                        {categoriesData?.items?.map((cat) => (
                          <option key={cat.id} value={cat.id}>
                            {cat.name}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label htmlFor="locationId" className="block text-sm font-medium text-gray-700 mb-2">
                        Lokasyon (Opsiyonel)
                      </label>
                      <div className="relative location-dropdown-container">
                        <input
                          type="text"
                          id="locationId"
                          placeholder="Lokasyon ara..."
                          value={formData.locationId ? locationsData?.items?.find((loc: any) => loc.id === formData.locationId)?.name || '' : locationSearchInputInModal}
                          onChange={(e) => {
                            setLocationSearchInputInModal(e.target.value)
                            setShowLocationDropdownInModal(true)
                            if (formData.locationId) {
                              setFormData({ ...formData, locationId: undefined })
                            }
                          }}
                          onFocus={() => setShowLocationDropdownInModal(true)}
                          className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                        />
                        <div className="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
                          <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                          </svg>
                        </div>
                        {showLocationDropdownInModal && (
                          <div className="absolute z-10 mt-1 w-full bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
                            <button
                              type="button"
                              onClick={() => {
                                setFormData({ ...formData, locationId: undefined })
                                setLocationSearchInputInModal('')
                                setShowLocationDropdownInModal(false)
                              }}
                              className="w-full text-left px-4 py-3 hover:bg-gray-50 focus:bg-gray-50 focus:outline-none transition-colors border-b border-gray-100"
                            >
                              <div className="font-medium text-gray-900">Lokasyon Seçin</div>
                            </button>
                            {locationsData?.items
                              ?.filter((loc: any) => 
                                loc.name?.toLowerCase().includes(locationSearchInputInModal.toLowerCase())
                              )
                              .length === 0 ? (
                                <div className="px-4 py-3 text-sm text-gray-500">
                                  Lokasyon bulunamadı
                                </div>
                              ) : (
                                locationsData?.items
                                  ?.filter((loc: any) => 
                                    loc.name?.toLowerCase().includes(locationSearchInputInModal.toLowerCase())
                                  )
                                  .map((loc: any) => (
                                    <button
                                      key={loc.id}
                                      type="button"
                                      onClick={() => {
                                        setFormData({ ...formData, locationId: loc.id })
                                        setLocationSearchInputInModal(loc.name)
                                        setShowLocationDropdownInModal(false)
                                      }}
                                      className="w-full text-left px-4 py-3 hover:bg-gray-50 focus:bg-gray-50 focus:outline-none transition-colors border-b border-gray-100 last:border-b-0"
                                    >
                                      <div className="font-medium text-gray-900">{loc.name}</div>
                                      {loc.description && (
                                        <div className="text-sm text-gray-500">{loc.description}</div>
                                      )}
                                    </button>
                                  ))
                              )}
                          </div>
                        )}
                      </div>
                    </div>
                    <div>
                      <label htmlFor="image" className="block text-sm font-medium text-gray-700">
                        Ürün Resmi (Opsiyonel)
                      </label>
                      <input
                        type="file"
                        id="image"
                        accept="image/*,.jpg,.jpeg,.png,.gif,.webp,.bmp,.tiff,.tif,.svg,.ico,.heic,.heif,.avif,.jfif,.pjpeg,.pjp,.jpe,.jif,.jp2,.j2k,.jpf"
                        onChange={handleImageChange}
                        className="mt-1 block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
                      />
                      {imagePreview && (
                        <div className="mt-2">
                          <img
                            src={imagePreview}
                            alt="Preview"
                            className="h-32 w-32 object-cover rounded-md border border-gray-300"
                          />
                        </div>
                      )}
                    </div>
                  </div>
                </div>
                <div className="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
                  <button
                    type="submit"
                    disabled={createMutation.isPending}
                    className="inline-flex w-full justify-center rounded-md bg-blue-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 sm:col-start-2 disabled:opacity-50"
                  >
                    {createMutation.isPending ? 'Oluşturuluyor...' : 'Oluştur'}
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setIsCreateModalOpen(false)
                      resetForm()
                    }}
                    className="mt-3 inline-flex w-full justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50 sm:col-start-1 sm:mt-0"
                  >
                    İptal
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Edit Modal */}
      {isEditModalOpen && (
        <div className="fixed inset-0 z-10 overflow-y-auto">
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={() => setIsEditModalOpen(false)} />
            <div className="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6">
              <form onSubmit={handleUpdate}>
                <div>
                  <h3 className="text-lg font-semibold leading-6 text-gray-900 mb-4">
                    Ürün Düzenle
                  </h3>
                  <div className="space-y-4">
                    <div>
                      <label htmlFor="edit-name" className="block text-sm font-medium text-gray-700">
                        İsim
                      </label>
                      <input
                        type="text"
                        id="edit-name"
                        value={formData.name}
                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="edit-description" className="block text-sm font-medium text-gray-700">
                        Açıklama
                      </label>
                      <textarea
                        id="edit-description"
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="edit-lowStockThreshold" className="block text-sm font-medium text-gray-700">
                        Düşük Stok Eşiği
                      </label>
                      <input
                        type="text"
                        inputMode="numeric"
                        id="edit-lowStockThreshold"
                        value={formData.lowStockThreshold === 0 ? '' : formData.lowStockThreshold}
                        onChange={(e) => {
                          const value = e.target.value.replace(/[^0-9]/g, '')
                          if (value === '') {
                            setFormData({ ...formData, lowStockThreshold: 0 })
                          } else {
                            setFormData({ ...formData, lowStockThreshold: parseInt(value, 10) || 0 })
                          }
                        }}
                        onBlur={(e) => {
                          if (e.target.value === '') {
                            setFormData({ ...formData, lowStockThreshold: 5 })
                          }
                        }}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="edit-locationId" className="block text-sm font-medium text-gray-700 mb-2">
                        Lokasyon (Opsiyonel)
                      </label>
                      <div className="relative location-dropdown-container">
                        <input
                          type="text"
                          id="edit-locationId"
                          placeholder="Lokasyon ara..."
                          value={formData.locationId ? locationsData?.items?.find((loc: any) => loc.id === formData.locationId)?.name || '' : locationSearchInputInModal}
                          onChange={(e) => {
                            setLocationSearchInputInModal(e.target.value)
                            setShowLocationDropdownInModal(true)
                            if (formData.locationId) {
                              setFormData({ ...formData, locationId: undefined })
                            }
                          }}
                          onFocus={() => setShowLocationDropdownInModal(true)}
                          className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                        />
                        <div className="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
                          <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                          </svg>
                        </div>
                        {showLocationDropdownInModal && (
                          <div className="absolute z-10 mt-1 w-full bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
                            <button
                              type="button"
                              onClick={() => {
                                setFormData({ ...formData, locationId: undefined })
                                setLocationSearchInputInModal('')
                                setShowLocationDropdownInModal(false)
                              }}
                              className="w-full text-left px-4 py-3 hover:bg-gray-50 focus:bg-gray-50 focus:outline-none transition-colors border-b border-gray-100"
                            >
                              <div className="font-medium text-gray-900">Lokasyon Seçin</div>
                            </button>
                            {locationsData?.items
                              ?.filter((loc: any) => 
                                loc.name?.toLowerCase().includes(locationSearchInputInModal.toLowerCase())
                              )
                              .length === 0 ? (
                                <div className="px-4 py-3 text-sm text-gray-500">
                                  Lokasyon bulunamadı
                                </div>
                              ) : (
                                locationsData?.items
                                  ?.filter((loc: any) => 
                                    loc.name?.toLowerCase().includes(locationSearchInputInModal.toLowerCase())
                                  )
                                  .map((loc: any) => (
                                    <button
                                      key={loc.id}
                                      type="button"
                                      onClick={() => {
                                        setFormData({ ...formData, locationId: loc.id })
                                        setLocationSearchInputInModal(loc.name)
                                        setShowLocationDropdownInModal(false)
                                      }}
                                      className="w-full text-left px-4 py-3 hover:bg-gray-50 focus:bg-gray-50 focus:outline-none transition-colors border-b border-gray-100 last:border-b-0"
                                    >
                                      <div className="font-medium text-gray-900">{loc.name}</div>
                                      {loc.description && (
                                        <div className="text-sm text-gray-500">{loc.description}</div>
                                      )}
                                    </button>
                                  ))
                              )}
                          </div>
                        )}
                      </div>
                    </div>
                    <div>
                      <label htmlFor="edit-image" className="block text-sm font-medium text-gray-700">
                        Ürün Resmi (Opsiyonel)
                      </label>
                      <input
                        type="file"
                        id="edit-image"
                        accept="image/*,.jpg,.jpeg,.png,.gif,.webp,.bmp,.tiff,.tif,.svg,.ico,.heic,.heif,.avif,.jfif,.pjpeg,.pjp,.jpe,.jif,.jp2,.j2k,.jpf"
                        onChange={handleImageChange}
                        className="mt-1 block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
                      />
                      {imagePreview && (
                        <div className="mt-2">
                          <img
                            src={imagePreview}
                            alt="Preview"
                            className="h-32 w-32 object-cover rounded-md border border-gray-300"
                          />
                        </div>
                      )}
                    </div>
                  </div>
                </div>
                <div className="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
                  <button
                    type="submit"
                    disabled={updateMutation.isPending}
                    className="inline-flex w-full justify-center rounded-md bg-blue-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-blue-500 sm:col-start-2 disabled:opacity-50"
                  >
                    {updateMutation.isPending ? 'Güncelleniyor...' : 'Güncelle'}
                  </button>
                  <button
                    type="button"
                    onClick={() => setIsEditModalOpen(false)}
                    className="mt-3 inline-flex w-full justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50 sm:col-start-1 sm:mt-0"
                  >
                    İptal
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Detail Modal */}
      {isDetailModalOpen && detailProductId && (
        <div className="fixed inset-0 z-10 overflow-y-auto">
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={() => {
              setIsDetailModalOpen(false)
              setDetailProductId(null)
            }} />
            <div className="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-4xl sm:p-6 max-h-[90vh] overflow-y-auto">
              <div>
                <h3 className="text-lg font-semibold leading-6 text-gray-900 mb-6">
                  Ürün Detayları ve Stok Hareketleri
                </h3>
                
                {/* Ürün Bilgileri */}
                {productsData?.items?.find(p => p.id === detailProductId) && (() => {
                  const product = productsData.items!.find(p => p.id === detailProductId)!
                  const productImage = (product as any).imagePath
                  return (
                    <div className="mb-6 p-4 bg-gray-50 rounded-lg">
                      <div className="flex gap-6">
                        {/* Ürün Resmi */}
                        {productImage && (
                          <div className="flex-shrink-0">
                            <img
                              src={`http://localhost:5134${productImage}`}
                              alt={product.name}
                              className="h-48 w-48 object-cover rounded-lg border-2 border-gray-200 shadow-md"
                            />
                          </div>
                        )}
                        {/* Ürün Bilgileri */}
                        <div className="flex-1 grid grid-cols-2 gap-4">
                          <div>
                            <label className="text-sm font-medium text-gray-500">Ürün Adı</label>
                            <p className="text-base font-semibold text-gray-900">{product.name}</p>
                          </div>
                          <div>
                            <label className="text-sm font-medium text-gray-500">Stok Kodu</label>
                            <p className="text-base font-mono text-gray-900">{product.stockCode}</p>
                          </div>
                          <div>
                            <label className="text-sm font-medium text-gray-500">Kategori</label>
                            <p className="text-base text-gray-900">{product.categoryName}</p>
                          </div>
                          <div>
                            <label className="text-sm font-medium text-gray-500">Lokasyon</label>
                            <p className="text-base text-gray-900">{(product as any).locationName || '-'}</p>
                          </div>
                          <div>
                            <label className="text-sm font-medium text-gray-500">Mevcut Stok</label>
                            <p className="text-base font-semibold text-gray-900">{product.stockQuantity}</p>
                          </div>
                          <div>
                            <label className="text-sm font-medium text-gray-500">Eklenme Tarihi</label>
                            <p className="text-base text-gray-900">
                              {new Date(product.createdAt + 'Z').toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric' })}
                              <span className="text-sm text-gray-500 ml-2">
                                {new Date(product.createdAt + 'Z').toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}
                              </span>
                            </p>
                          </div>
                          <div>
                            <label className="text-sm font-medium text-gray-500">Güncelleme Tarihi</label>
                            <p className="text-base text-gray-900">
                              {product.updatedAt 
                                ? (
                                  <>
                                    {new Date(product.updatedAt + 'Z').toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric' })}
                                    <span className="text-sm text-gray-500 ml-2">
                                      {new Date(product.updatedAt + 'Z').toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}
                                    </span>
                                  </>
                                )
                                : '-'
                              }
                            </p>
                          </div>
                          <div className="col-span-2">
                            <label className="text-sm font-medium text-gray-500">Açıklama</label>
                            <p className="text-base text-gray-900">{product.description || '-'}</p>
                          </div>
                        </div>
                      </div>
                    </div>
                  )
                })()}

                {/* Stok Hareketleri Tablosu */}
                <div className="mt-4">
                  <div className="flex items-center justify-between mb-3">
                    <h4 className="text-md font-semibold text-gray-900">Stok Hareketleri</h4>
                    <button
                      onClick={() => {
                        setStockMovementFormData({
                          productId: detailProductId || 0,
                          type: 1,
                          quantity: 0,
                          description: '',
                        })
                        setIsStockMovementModalOpen(true)
                      }}
                      className="inline-flex items-center gap-x-2 rounded-lg bg-green-600 px-3 py-1.5 text-sm font-semibold text-white shadow-sm hover:bg-green-500 transition-all duration-200"
                    >
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                      </svg>
                      Stok Hareketi Ekle
                    </button>
                  </div>
                  {(stockMovementsData as any)?.items && (stockMovementsData as any).items.length > 0 ? (
                    <div className="overflow-x-auto">
                      <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-50">
                          <tr>
                            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                              #
                            </th>
                            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                              İşlem Tipi
                            </th>
                            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                              Miktar
                            </th>
                            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                              Açıklama
                            </th>
                            <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                              Tarih
                            </th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-200 bg-white">
                          {(stockMovementsData as any).items.map((movement: any, index: number) => (
                            <tr key={movement.id} className="hover:bg-gray-50">
                              <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-900">
                                {(stockMovementPage - 1) * stockMovementPageSize + index + 1}
                              </td>
                              <td className="px-4 py-3 whitespace-nowrap">
                                <span className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium ${
                                  movement.type === 1
                                    ? 'bg-green-50 text-green-700 ring-1 ring-inset ring-green-600/20'
                                    : 'bg-red-50 text-red-700 ring-1 ring-inset ring-red-600/20'
                                }`}>
                                  {movement.type === 1 ? '📥 Giriş' : '📤 Çıkış'}
                                </span>
                              </td>
                              <td className="px-4 py-3 whitespace-nowrap text-sm font-semibold text-gray-900">
                                {movement.quantity}
                              </td>
                              <td className="px-4 py-3 text-sm text-gray-600 max-w-xs truncate">
                                {movement.description || '-'}
                              </td>
                              <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-500">
                                {new Date(movement.createdAt).toLocaleString('tr-TR', {
                                  day: '2-digit',
                                  month: '2-digit',
                                  year: 'numeric',
                                  hour: '2-digit',
                                  minute: '2-digit'
                                })}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  ) : (
                    <div className="text-center py-8 text-gray-500">
                      Bu ürün için henüz stok hareketi bulunmamaktadır.
                    </div>
                  )}
                  
                  {/* Pagination for Stock Movements */}
                  {stockMovementsData && (stockMovementsData.totalPages ?? 0) > 1 && (
                    <div className="mt-4 flex items-center justify-between border-t border-gray-200 bg-gray-50 px-4 py-3 sm:px-6 rounded-lg">
                      <div className="flex flex-1 justify-between sm:hidden">
                        <button
                          onClick={() => setStockMovementPage(stockMovementPage - 1)}
                          disabled={!stockMovementsData.hasPreviousPage}
                          className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                          Önceki
                        </button>
                        <button
                          onClick={() => setStockMovementPage(stockMovementPage + 1)}
                          disabled={!stockMovementsData.hasNextPage}
                          className="relative ml-3 inline-flex items-center rounded-md border border-white/50 bg-white/70 backdrop-blur-md px-4 py-2 text-sm font-medium text-gray-900 hover:bg-white/80 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                          Sonraki
                        </button>
                      </div>
                      <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
                        <div>
                          <p className="text-sm text-gray-800">
                            <span className="font-medium">{stockMovementsData.pageNumber}</span> / <span className="font-medium">{stockMovementsData.totalPages}</span> sayfa gösteriliyor
                          </p>
                        </div>
                        <div>
                          <nav className="isolate inline-flex -space-x-px rounded-md shadow-sm">
                            <button
                              onClick={() => setStockMovementPage(stockMovementPage - 1)}
                              disabled={!stockMovementsData.hasPreviousPage}
                              className="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-700 ring-1 ring-inset ring-gray-300 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                              Önceki
                            </button>
                            <button
                              onClick={() => setStockMovementPage(stockMovementPage + 1)}
                              disabled={!stockMovementsData.hasNextPage}
                              className="relative inline-flex items-center rounded-r-md px-2 py-2 text-gray-700 ring-1 ring-inset ring-gray-300 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                              Sonraki
                            </button>
                          </nav>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              </div>
              <div className="mt-6 flex justify-end">
                <button
                  type="button"
                  onClick={() => {
                    setIsDetailModalOpen(false)
                    setDetailProductId(null)
                    setStockMovementPage(1)
                  }}
                  className="inline-flex justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50"
                >
                  Kapat
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Stock Movement Modal for Product Detail */}
      {isStockMovementModalOpen && (
        <div className="fixed inset-0 z-20 overflow-y-auto">
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={() => {
              setIsStockMovementModalOpen(false)
              setStockMovementFormData({ productId: detailProductId || 0, type: 1, quantity: 0, description: '' })
            }} />
            <div className="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6">
              <form onSubmit={(e) => {
                e.preventDefault()
                if (stockMovementFormData.productId === 0) {
                  alert('Lütfen bir ürün seçin!')
                  return
                }
                if (stockMovementFormData.quantity <= 0) {
                  alert('Miktar 0\'dan büyük olmalıdır!')
                  return
                }
                stockMovementMutation.mutate(stockMovementFormData)
              }}>
                <div>
                  <h3 className="text-lg font-semibold leading-6 text-gray-900 mb-4">
                    Stok Hareketi Ekle
                  </h3>
                  <div className="space-y-4">
                    <div>
                      <label htmlFor="stock-movement-type" className="block text-sm font-medium text-gray-700">
                        İşlem Tipi
                      </label>
                      <select
                        id="stock-movement-type"
                        required
                        value={stockMovementFormData.type}
                        onChange={(e) => setStockMovementFormData({ ...stockMovementFormData, type: Number(e.target.value) })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-green-500 focus:ring-green-500 sm:text-sm px-3 py-2 border"
                      >
                        <option value="1">📥 Giriş</option>
                        <option value="2">📤 Çıkış</option>
                      </select>
                    </div>
                    <div>
                      <label htmlFor="stock-movement-quantity" className="block text-sm font-medium text-gray-700">
                        Miktar
                      </label>
                      <input
                        type="text"
                        inputMode="numeric"
                        id="stock-movement-quantity"
                        required
                        value={stockMovementFormData.quantity === 0 ? '' : stockMovementFormData.quantity}
                        onChange={(e) => {
                          const value = e.target.value.replace(/[^0-9]/g, '')
                          if (value === '') {
                            setStockMovementFormData({ ...stockMovementFormData, quantity: 0 })
                          } else {
                            setStockMovementFormData({ ...stockMovementFormData, quantity: parseInt(value, 10) || 0 })
                          }
                        }}
                        onBlur={(e) => {
                          if (e.target.value === '') {
                            setStockMovementFormData({ ...stockMovementFormData, quantity: 0 })
                          }
                        }}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-green-500 focus:ring-green-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="stock-movement-description" className="block text-sm font-medium text-gray-700">
                        Açıklama (Opsiyonel)
                      </label>
                      <textarea
                        id="stock-movement-description"
                        value={stockMovementFormData.description}
                        onChange={(e) => setStockMovementFormData({ ...stockMovementFormData, description: e.target.value })}
                        rows={3}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-green-500 focus:ring-green-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                  </div>
                </div>
                <div className="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
                  <button
                    type="submit"
                    disabled={stockMovementMutation.isPending}
                    className="inline-flex w-full justify-center rounded-md bg-green-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-green-500 sm:col-start-2 disabled:opacity-50"
                  >
                    {stockMovementMutation.isPending ? 'Oluşturuluyor...' : 'Oluştur'}
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setIsStockMovementModalOpen(false)
                      setStockMovementFormData({ productId: detailProductId || 0, type: 1, quantity: 0, description: '' })
                    }}
                    className="mt-3 inline-flex w-full justify-center rounded-md bg-white px-3 py-2 text-sm font-semibold text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 hover:bg-gray-50 sm:col-start-1 sm:mt-0"
                  >
                    İptal
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

