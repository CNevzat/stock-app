import { useState, useEffect, useRef, useMemo, useCallback } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useSearchParams } from 'react-router-dom'
import { productService } from '../services/productService'
import { categoryService } from '../services/categoryService'
import { locationService } from '../services/locationService'
import { stockMovementService } from '../services/stockMovementService'
import { productAttributeService } from '../services/productAttributeService'
import { signalRService } from '../services/signalRService'
import { TechnologyInfo } from '../components/TechnologyInfo'
import type {CreateProductCommand, UpdateProductCommand} from "../Api";
import { ResponsiveContainer, LineChart, CartesianGrid, XAxis, YAxis, Tooltip, Legend, Line } from 'recharts'

// API base URL helper - hem dev hem production'da 5134 portunu kullan
const getApiBaseUrl = () => {
  if (import.meta.env.VITE_API_BASE_URL) {
    return import.meta.env.VITE_API_BASE_URL;
  }
  if (import.meta.env.PROD) {
    return `http://${window.location.hostname}:5134`;
  }
  return 'http://localhost:5134';
};

// Image URL helper
const getImageUrl = (imagePath: string | null | undefined) => {
  if (!imagePath) return null;
  return `${getApiBaseUrl()}${imagePath}`;
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
  const isTypingRef = useRef(false)

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

  // Debounce search input - Preserve focus during debounce
  useEffect(() => {
    const timer = setTimeout(() => {
      const input = searchInputRef.current
      const wasFocused = document.activeElement === input
      const cursorPosition = input?.selectionStart ?? null
      
      setSearchTerm(searchInput)
      setPage((prevPage) => prevPage !== 1 ? 1 : prevPage)
      
      // Restore focus after state update if input was focused
      if (wasFocused && input) {
        // Use requestAnimationFrame + setTimeout to ensure it runs after React's render cycle
        requestAnimationFrame(() => {
          setTimeout(() => {
            if (input && document.body.contains(input)) {
              input.focus()
              if (cursorPosition !== null && cursorPosition <= (input.value?.length ?? 0)) {
                input.setSelectionRange(cursorPosition, cursorPosition)
              }
            }
          }, 0)
        })
      }
    }, 800) // 800ms debounce

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

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [isEditModalOpen, setIsEditModalOpen] = useState(false)
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false)
  const [selectedProductId, setSelectedProductId] = useState<number | null>(null)
  const [detailProductId, setDetailProductId] = useState<number | null>(null)
  const detailProductIdRef = useRef<number | null>(null)

  useEffect(() => {
    detailProductIdRef.current = detailProductId
  }, [detailProductId])

  // SignalR - Real-time updates için event listener'lar
  useEffect(() => {
    signalRService.startConnection().catch((error) => {
      console.error('SignalR bağlantı hatası:', error)
    })

    const handleProductCreated = () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      queryClient.invalidateQueries({ queryKey: ['categories'] })
    }

    const handleProductUpdated = (product: any) => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      queryClient.invalidateQueries({ queryKey: ['categories'] })

      if (product?.id && product.id === detailProductIdRef.current) {
        queryClient.invalidateQueries({ queryKey: ['product-detail', detailProductIdRef.current] })
        queryClient.invalidateQueries({ queryKey: ['stock-movements-detail', detailProductIdRef.current] })
        queryClient.invalidateQueries({ queryKey: ['product-attributes-detail', detailProductIdRef.current] })
      }
    }

    const handleProductDeleted = () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      queryClient.invalidateQueries({ queryKey: ['categories'] })
    }

    signalRService.onProductCreated(handleProductCreated)
    signalRService.onProductUpdated(handleProductUpdated)
    signalRService.onProductDeleted(handleProductDeleted)

    return () => {
      signalRService.offProductCreated(handleProductCreated)
      signalRService.offProductUpdated(handleProductUpdated)
      signalRService.offProductDeleted(handleProductDeleted)
    }
  }, [queryClient])

  const [stockMovementPage, setStockMovementPage] = useState(1)
  const [stockMovementPageSize] = useState(10)
  const [isStockMovementModalOpen, setIsStockMovementModalOpen] = useState(false)
  const [stockMovementFormData, setStockMovementFormData] = useState({
    productId: 0,
    type: 1, // 1 = Giriş, 2 = Çıkış
    quantity: 0,
    unitPrice: 0,
    description: '',
  })
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    stockQuantity: 0,
    lowStockThreshold: 5,
    categoryId: 0,
    locationId: undefined as number | undefined,
    purchasePrice: 0,
    salePrice: 0,
  })
  const [selectedImage, setSelectedImage] = useState<File | null>(null)
  const [imagePreview, setImagePreview] = useState<string | null | undefined>(null)
  const [isExporting, setIsExporting] = useState(false)

  // Fetch products
  const { data: productsData, isLoading, isFetching } = useQuery({
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

  // Preserve focus during and after query - Keep cursor in input at all times during search
  useEffect(() => {
    const input = searchInputRef.current
    if (input && isTypingRef.current) {
      // User is typing, maintain focus throughout query lifecycle
      requestAnimationFrame(() => {
        setTimeout(() => {
          if (input && document.body.contains(input) && isTypingRef.current) {
            const cursorPosition = input.selectionStart ?? input.value.length
            input.focus()
            if (cursorPosition <= input.value.length) {
              input.setSelectionRange(cursorPosition, cursorPosition)
            }
          }
        }, 0)
      })
    }
  }, [isFetching, productsData])

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

  // Fetch product attributes for detail modal
  const { data: productAttributesData } = useQuery({
    queryKey: ['product-attributes-detail', detailProductId],
    queryFn: () => productAttributeService.getAll({
      pageNumber: 1,
      pageSize: 100, // Tüm öznitelikleri getir
      productId: detailProductId || undefined,
    }),
    enabled: !!detailProductId,
  })

  const { data: detailProductData, isLoading: isDetailProductLoading } = useQuery({
    queryKey: ['product-detail', detailProductId],
    queryFn: () => productService.getById(detailProductId ?? 0),
    enabled: detailProductId !== null,
  })

  const activeDetailProduct = useMemo(() => {
    if (detailProductData) {
      return detailProductData
    }
    return productsData?.items?.find((p: any) => p.id === detailProductId) || null
  }, [detailProductData, productsData?.items, detailProductId])

  const getDefaultMovementUnitPrice = useCallback((type: number) => {
    if (!activeDetailProduct) {
      return 0
    }
    return type === 1
      ? Number(activeDetailProduct.currentPurchasePrice ?? 0)
      : Number(activeDetailProduct.currentSalePrice ?? 0)
  }, [activeDetailProduct])

  const formatCurrency = (value: number) =>
    `₺${value.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`

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
    onError: (error: any) => {
      const errorMessage = error?.message || 'Ürün silinirken bir hata oluştu';
      alert(errorMessage);
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
      setStockMovementFormData({
        productId: detailProductId || 0,
        type: 1,
        quantity: 0,
        unitPrice: getDefaultMovementUnitPrice(1),
        description: '',
      })
    },
    onError: (error: any) => {
      const errorMessage = error?.message || error?.response?.data?.message || 'Bir hata oluştu!'
      alert(errorMessage)
    },
  })

  const resetForm = () => {
    setFormData({
      name: '',
      description: '',
      stockQuantity: 0,
      lowStockThreshold: 5,
      categoryId: 0,
      locationId: undefined,
      purchasePrice: 0,
      salePrice: 0,
    })
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
    if (!formData.name.trim()) {
      alert('Ürün adı gereklidir.')
      return
    }
    if (!formData.categoryId) {
      alert('Lütfen bir kategori seçin.')
      return
    }
    if (formData.purchasePrice <= 0 || formData.salePrice <= 0) {
      alert('Satın alma ve satış fiyatları 0\'dan büyük olmalıdır.')
      return
    }
    createMutation.mutate({ ...formData, image: selectedImage || undefined } as any)
  }

  const handleUpdate = (e: React.FormEvent) => {
    e.preventDefault()
    if (selectedProductId) {
      if (!formData.name.trim()) {
        alert('Ürün adı gereklidir.')
        return
      }
      if (!formData.categoryId) {
        alert('Lütfen bir kategori seçin.')
        return
      }
      if (formData.purchasePrice <= 0 || formData.salePrice <= 0) {
        alert('Satın alma ve satış fiyatları 0\'dan büyük olmalıdır.')
        return
      }
      updateMutation.mutate({
        id: selectedProductId,
        dto: {
          id: selectedProductId,
          name: formData.name,
          description: formData.description,
          lowStockThreshold: formData.lowStockThreshold,
          categoryId: formData.categoryId,
          locationId: formData.locationId,
          purchasePrice: formData.purchasePrice,
          salePrice: formData.salePrice,
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
      categoryId: product.categoryId || 0,
      locationId: product.locationId || undefined,
      purchasePrice: product.currentPurchasePrice || 0,
      salePrice: product.currentSalePrice || 0,
    })
    setSelectedImage(null)
    const imagePath = (product as any).imagePath
    if (imagePath) {
      setImagePreview(getImageUrl(imagePath))
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

  const handleDelete = (product: any) => {
    if (product.id && confirm('Silmek istediğinize emin misiniz?')) {
      deleteMutation.mutate(product.id)
    }
  }

  const detailPriceHistory = useMemo(() => {
    if (!activeDetailProduct?.priceHistory) {
      return [] as Array<{ id: number; purchasePrice: number; salePrice: number; effectiveDate: string }>;
    }
    return [...activeDetailProduct.priceHistory]
      .filter((entry): entry is { id: number; purchasePrice: number; salePrice: number; effectiveDate: string } => !!entry.effectiveDate)
      .sort((a, b) => new Date(a.effectiveDate ?? '').getTime() - new Date(b.effectiveDate ?? '').getTime())
  }, [activeDetailProduct?.priceHistory])

  const priceHistoryChartData = useMemo(() =>
    detailPriceHistory.map((entry) => {
      const dateObj = entry.effectiveDate ? new Date(entry.effectiveDate) : null
      const dateLabel = dateObj
        ? dateObj.toLocaleString('tr-TR', {
            day: '2-digit',
            month: 'short',
            hour: '2-digit',
            minute: '2-digit',
          })
        : ''
      return {
        id: entry.id,
        dateLabel,
        purchasePrice: Number(entry.purchasePrice ?? 0),
        salePrice: Number(entry.salePrice ?? 0),
      }
    }),
  [detailPriceHistory])

  if (isLoading) {
    return <div className="flex justify-center items-center h-64">Yükleniyor...</div>
  }

  return (
    <div className="px-4 sm:px-6 lg:px-8">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <div className="flex items-center gap-2">
            <h1 className="text-3xl font-semibold text-gray-900">Ürünler</h1>
            <TechnologyInfo
              technologies={[
                'Redis Cache (60s TTL) - Hızlı veri erişimi',
                'Elasticsearch - Gelişmiş arama (fuzzy, case-insensitive)',
                'SignalR - Real-time güncellemeler'
              ]}
              description="Arama sonuçları Redis'te cache'lenir, Elasticsearch ile hızlı ve akıllı arama yapılır."
            />
          </div>
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
          onChange={(e) => {
            isTypingRef.current = true
            setSearchInput(e.target.value)
          }}
          onFocus={() => {
            isTypingRef.current = true
          }}
          onBlur={(e) => {
            // Keep focus during query - only allow blur if query is not running
            if (isFetching) {
              setTimeout(() => {
                if (searchInputRef.current && isTypingRef.current) {
                  searchInputRef.current.focus()
                }
              }, 0)
            } else {
              // After query completes, allow blur but restore if user is still typing
              setTimeout(() => {
                if (document.activeElement !== searchInputRef.current && isTypingRef.current) {
                  const wasTyping = isTypingRef.current
                  // Reset after a delay to allow user to click elsewhere if they want
                  setTimeout(() => {
                    isTypingRef.current = false
                  }, 200)
                  if (wasTyping && searchInputRef.current) {
                    searchInputRef.current.focus()
                  }
                }
              }, 50)
            }
          }}
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
        <div className="inline-block w-full align-middle">
          <div className="overflow-hidden shadow ring-1 ring-black ring-opacity-5 sm:rounded-lg">
            <table className="min-w-full divide-y divide-gray-300">
              <thead className="bg-gray-50">
                <tr>
                  <th scope="col" className="py-3.5 pl-2 pr-2 text-left text-sm font-semibold text-gray-900 whitespace-nowrap w-[50px]">
                    #
                  </th>
                  <th className="px-2 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[260px] max-w-[260px]">
                    İsim
                  </th>
                  <th className="px-2 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap w-[100px]">
                    Stok Kodu
                  </th>
                  <th className="px-1 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap w-[60px]">
                    Stok
                  </th>
                  <th className="px-2 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[140px] max-w-[180px]">
                    Kategori
                  </th>
                  <th className="px-2 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[160px] max-w-[200px]">
                    Lokasyon
                  </th>
                  <th className="px-2 py-3.5 text-right text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[110px]">
                    Satın Alma
                  </th>
                  <th className="px-2 py-3.5 text-right text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[110px]">
                    Satış
                  </th>
                  <th className="relative py-3.5 pl-2 pr-2 text-right text-sm font-semibold text-gray-900 whitespace-nowrap w-[280px]">
                    <span className="sr-only">İşlemler</span>
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 bg-white">
                {productsData?.items?.map((product: any, index: number) => (
                  <tr key={product.id} className="hover:bg-gray-50 transition-colors duration-150">
                    <td className="whitespace-nowrap py-4 pl-2 pr-2 text-sm font-medium text-gray-900">
                      <span className="inline-flex items-center justify-center w-7 h-7 rounded-full bg-gray-100 text-gray-600 font-semibold text-xs">
                        {(page - 1) * pageSize + index + 1}
                      </span>
                    </td>
                    <td className="px-2 py-4 text-sm font-medium text-gray-900 min-w-[260px] max-w-[260px]">
                      <div className="flex items-center gap-2">
                        {(product as any).imagePath && (
                          <img
                            src={getImageUrl((product as any).imagePath) || ''}
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
                    <td className="whitespace-nowrap px-2 py-4 text-sm min-w-[140px] max-w-[180px]">
                      <span className="block truncate text-gray-700">
                        {product.categoryName || '-'}
                      </span>
                    </td>
                    <td className="whitespace-nowrap px-2 py-4 text-sm min-w-[160px] max-w-[200px]">
                      <span className="block truncate max-w-[220px]">{product.locationName || '-'}</span>
                    </td>
                    <td className="whitespace-nowrap px-2 py-4 text-sm text-right min-w-[110px]">
                      ₺{Number(product.currentPurchasePrice ?? 0).toLocaleString('tr-TR', {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2,
                      })}
                    </td>
                    <td className="whitespace-nowrap px-2 py-4 text-sm text-right min-w-[110px]">
                      ₺{Number(product.currentSalePrice ?? 0).toLocaleString('tr-TR', {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2,
                      })}
                    </td>
                    <td className="relative py-4 pl-2 pr-4 text-right text-sm font-medium whitespace-nowrap w-[280px]">
                      <div className="flex justify-end items-center gap-1.5">
                         <button
                           onClick={() => openDetailModal(product)}
                           className="inline-flex items-center gap-1 rounded-md bg-indigo-50 px-2.5 py-1.5 text-xs font-semibold text-indigo-600 hover:bg-indigo-100">
                           Detay
                         </button>
                         <button
                           onClick={() => openEditModal(product)}
                           className="inline-flex items-center gap-1 rounded-md bg-blue-50 px-2.5 py-1.5 text-xs font-semibold text-blue-600 hover:bg-blue-100">
                           Düzenle
                         </button>
                         <button
                           onClick={() => handleDelete(product)}
                           className="inline-flex items-center gap-1 rounded-md bg-red-50 px-2.5 py-1.5 text-xs font-semibold text-red-600 hover:bg-red-100">
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
                      <label htmlFor="purchasePrice" className="block text-sm font-medium text-gray-700">
                        Satın Alma Fiyatı *
                      </label>
                      <input
                        type="number"
                        step="0.01"
                        id="purchasePrice"
                        required
                        value={formData.purchasePrice === 0 ? '' : formData.purchasePrice}
                        onChange={(e) => {
                          const value = parseFloat(e.target.value)
                          setFormData({ ...formData, purchasePrice: Number.isNaN(value) ? 0 : value })
                        }}
                        onBlur={(e) => {
                          if (e.target.value === '') {
                            setFormData({ ...formData, purchasePrice: 0 })
                          }
                        }}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="salePrice" className="block text-sm font-medium text-gray-700">
                        Satış Fiyatı *
                      </label>
                      <input
                        type="number"
                        step="0.01"
                        id="salePrice"
                        required
                        value={formData.salePrice === 0 ? '' : formData.salePrice}
                        onChange={(e) => {
                          const value = parseFloat(e.target.value)
                          setFormData({ ...formData, salePrice: Number.isNaN(value) ? 0 : value })
                        }}
                        onBlur={(e) => {
                          if (e.target.value === '') {
                            setFormData({ ...formData, salePrice: 0 })
                          }
                        }}
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
                      <label htmlFor="edit-purchasePrice" className="block text-sm font-medium text-gray-700">
                        Satın Alma Fiyatı *
                      </label>
                      <input
                        type="number"
                        step="0.01"
                        id="edit-purchasePrice"
                        value={formData.purchasePrice === 0 ? '' : formData.purchasePrice}
                        onChange={(e) => {
                          const value = parseFloat(e.target.value)
                          setFormData({ ...formData, purchasePrice: Number.isNaN(value) ? 0 : value })
                        }}
                        onBlur={(e) => {
                          if (e.target.value === '') {
                            setFormData({ ...formData, purchasePrice: 0 })
                          }
                        }}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="edit-salePrice" className="block text-sm font-medium text-gray-700">
                        Satış Fiyatı *
                      </label>
                      <input
                        type="number"
                        step="0.01"
                        id="edit-salePrice"
                        value={formData.salePrice === 0 ? '' : formData.salePrice}
                        onChange={(e) => {
                          const value = parseFloat(e.target.value)
                          setFormData({ ...formData, salePrice: Number.isNaN(value) ? 0 : value })
                        }}
                        onBlur={(e) => {
                          if (e.target.value === '') {
                            setFormData({ ...formData, salePrice: 0 })
                          }
                        }}
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
                    <label htmlFor="edit-categoryId" className="block text-sm font-medium text-gray-700">
                      Kategori
                    </label>
                    <select
                      id="edit-categoryId"
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
        <div className="fixed inset-0 z-50 overflow-y-auto">
          <div className="flex min-h-full items-center justify-center p-4 pt-20">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity z-40" onClick={() => {
              setIsDetailModalOpen(false)
              setDetailProductId(null)
            }} />
            <div className="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all w-full max-w-4xl max-h-[85vh] flex flex-col z-50">
              <div className="flex-1 overflow-y-auto px-4 pb-4 pt-5">
                <h3 className="text-lg font-semibold leading-6 text-gray-900 mb-6">
                  Ürün Detayları ve Stok Hareketleri
                </h3>
                
                {/* Ürün Bilgileri ve Fiyatlar */}
                {(() => {
                  const baseProduct = detailProductData ?? (productsData?.items?.find((p: any) => p.id === detailProductId) ?? null)

                  if (!baseProduct) {
                    return isDetailProductLoading ? (
                      <div className="mb-6 p-4 bg-gray-50 rounded-lg text-center">
                        <span className="text-sm text-gray-500">Ürün bilgileri yükleniyor...</span>
                      </div>
                    ) : null;
                  }

                  const createdAtDate = baseProduct?.createdAt
                    ? new Date(baseProduct.createdAt).toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric' })
                    : '-'
                  const createdAtTime = baseProduct?.createdAt
                    ? new Date(baseProduct.createdAt).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })
                    : ''
                  const updatedAtDate = baseProduct?.updatedAt
                    ? new Date(baseProduct.updatedAt).toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric' })
                    : '-'
                  const updatedAtTime = baseProduct?.updatedAt
                    ? new Date(baseProduct.updatedAt).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })
                    : ''

                  const productImage = (baseProduct as any).imagePath
                  const lastPurchasePrice = Number(baseProduct.currentPurchasePrice ?? 0)
                  const averagePurchasePrice = Number(baseProduct.averagePurchasePrice ?? lastPurchasePrice)
                  const lastSalePrice = Number(baseProduct.currentSalePrice ?? 0)
                  const averageSalePrice = Number(baseProduct.averageSalePrice ?? lastSalePrice)
                  const inventoryCost = Number((baseProduct.stockQuantity ?? 0) * lastPurchasePrice)
                  const potentialRevenue = Number((baseProduct.stockQuantity ?? 0) * lastSalePrice)
                  const potentialProfit = potentialRevenue - inventoryCost

                  return (
                    <>
                      <div className="mb-6 p-4 bg-gray-50 rounded-lg">
                        <div className="flex gap-6">
                          {/* Ürün Resmi */}
                          {productImage && (
                            <div className="flex-shrink-0">
                              <img
                                src={getImageUrl(productImage) || ''}
                                alt={baseProduct.name}
                                className="h-48 w-48 object-cover rounded-lg border-2 border-gray-200 shadow-md"
                              />
                            </div>
                          )}
                          {/* Ürün Bilgileri */}
                          <div className="flex-1 grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                              <label className="text-sm font-medium text-gray-500">Ürün Adı</label>
                              <p className="text-base font-semibold text-gray-900">{baseProduct.name}</p>
                            </div>
                            <div>
                              <label className="text-sm font-medium text-gray-500">Stok Kodu</label>
                              <p className="text-base font-mono text-gray-900">{baseProduct.stockCode}</p>
                            </div>
                            <div>
                              <label className="text-sm font-medium text-gray-500">Kategori</label>
                              <p className="text-base text-gray-900">{baseProduct.categoryName}</p>
                            </div>
                            <div>
                              <label className="text-sm font-medium text-gray-500">Lokasyon</label>
                              <p className="text-base text-gray-900">{(baseProduct as any).locationName || '-'}</p>
                            </div>
                            <div>
                              <label className="text-sm font-medium text-gray-500">Mevcut Stok</label>
                              <p className="text-base font-semibold text-gray-900">{baseProduct.stockQuantity}</p>
                            </div>
                            <div>
                              <label className="text-sm font-medium text-gray-500">Eklenme Tarihi</label>
                              <p className="text-base text-gray-900">
                                {createdAtDate}
                                {createdAtTime && (
                                  <span className="text-sm text-gray-500 ml-2">{createdAtTime}</span>
                                )}
                              </p>
                            </div>
                            <div>
                              <label className="text-sm font-medium text-gray-500">Güncelleme Tarihi</label>
                              <p className="text-base text-gray-900">
                                {updatedAtDate}
                                {updatedAtTime && (
                                  <span className="text-sm text-gray-500 ml-2">{updatedAtTime}</span>
                                )}
                              </p>
                            </div>
                            <div className="md:col-span-2">
                              <label className="text-sm font-medium text-gray-500">Açıklama</label>
                              <p className="text-base text-gray-900">{baseProduct.description || '-'}</p>
                            </div>
                          </div>
                        </div>
                      </div>

                      <div className="mb-6 grid grid-cols-1 md:grid-cols-4 gap-4">
                        <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
                          <div className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Satın Alma Fiyatı</div>
                          <div className="mt-2 text-lg font-bold text-gray-900">{formatCurrency(lastPurchasePrice)}</div>
                          <div className="text-sm text-gray-500">Ortalama: {formatCurrency(averagePurchasePrice)}</div>
                        </div>
                        <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
                          <div className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Satış Fiyatı</div>
                          <div className="mt-2 text-lg font-bold text-gray-900">{formatCurrency(lastSalePrice)}</div>
                          <div className="text-sm text-gray-500">Ortalama: {formatCurrency(averageSalePrice)}</div>
                        </div>
                        <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
                          <div className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Envanter Maliyeti</div>
                          <div className="mt-2 text-lg font-bold text-blue-600">{formatCurrency(inventoryCost)}</div>
                        </div>
                        <div className="bg-white border border-gray-200 rounded-lg p-4 shadow-sm">
                          <div className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Potansiyel Kar</div>
                          <div className={`mt-2 text-lg font-bold ${potentialProfit >= 0 ? 'text-emerald-600' : 'text-red-600'}`}>
                            {formatCurrency(potentialProfit)}
                          </div>
                        </div>
                      </div>

                      {detailPriceHistory.length > 0 && (
                        <div className="mb-6 bg-gray-50 rounded-lg p-4">
                          <h4 className="text-md font-semibold text-gray-900 mb-4">Fiyat Geçmişi</h4>
                          <div className="h-64">
                            <ResponsiveContainer width="100%" height="100%">
                              <LineChart data={priceHistoryChartData}>
                                <CartesianGrid strokeDasharray="3 3" />
                                <XAxis dataKey="dateLabel" tick={{ fontSize: 12 }} />
                                <YAxis />
                                <Tooltip
                                  formatter={(value: number) =>
                                    `₺${Number(value).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
                                  }
                                  labelFormatter={(label) => label}
                                />
                                <Legend />
                                <Line type="monotone" dataKey="purchasePrice" name="Satın Alma" stroke="#6366f1" strokeWidth={2} dot={false} />
                                <Line type="monotone" dataKey="salePrice" name="Satış" stroke="#f97316" strokeWidth={2} dot={false} />
                              </LineChart>
                            </ResponsiveContainer>
                          </div>
                          <div className="mt-4 overflow-x-auto bg-white rounded-lg border border-gray-200">
                            <table className="min-w-full divide-y divide-gray-200">
                              <thead className="bg-gray-100">
                                <tr>
                                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Tarih</th>
                                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Satın Alma</th>
                                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Satış</th>
                                </tr>
                              </thead>
                              <tbody className="divide-y divide-gray-200">
                                {[...detailPriceHistory].reverse().map((entry) => (
                                  <tr key={entry.id} className="hover:bg-gray-50">
                                    <td className="px-4 py-3 text-sm text-gray-700">
                                      {new Date(entry.effectiveDate ?? '').toLocaleString('tr-TR', {
                                        day: '2-digit',
                                        month: '2-digit',
                                        year: 'numeric',
                                        hour: '2-digit',
                                        minute: '2-digit',
                                      })}
                                    </td>
                                    <td className="px-4 py-3 text-sm font-semibold text-gray-900">
                                      ₺{Number(entry.purchasePrice ?? 0).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                                    </td>
                                    <td className="px-4 py-3 text-sm font-semibold text-gray-900">
                                      ₺{Number(entry.salePrice ?? 0).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                                    </td>
                                  </tr>
                                ))}
                              </tbody>
                            </table>
                          </div>
                        </div>
                      )}

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
                                unitPrice: getDefaultMovementUnitPrice(1),
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
                                  Birim Fiyat
                                </th>
                                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                                  Toplam
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
                                    <td className="px-4 py-3 whitespace-nowrap text-sm font-mono text-right text-gray-700">
                                      ₺{Number(movement.unitPrice ?? 0).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                                    </td>
                                    <td className="px-4 py-3 whitespace-nowrap text-sm font-semibold text-right text-blue-600">
                                      ₺{Number(movement.totalValue ?? ((movement.quantity || 0) * (movement.unitPrice || 0))).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                                    </td>
                                    <td className="px-4 py-3 text-sm text-gray-600 max-w-xs truncate">
                                      {movement.description || '-'}
                                    </td>
                                    <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-500">
                                      {movement.createdAt ? new Date(movement.createdAt).toLocaleString('tr-TR', {
                                        day: '2-digit',
                                        month: '2-digit',
                                        year: 'numeric',
                                        hour: '2-digit',
                                        minute: '2-digit'
                                      }) : '-'}
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
                    </>
                  )
                })()}

                {/* Ürün Öznitelikleri */}
                <div className="mt-6 mb-6">
                  <h4 className="text-md font-semibold text-gray-900 mb-3">Ürün Öznitelikleri</h4>
                  {productAttributesData?.items && productAttributesData.items.length > 0 ? (
                    <div className="bg-gray-50 rounded-lg p-4">
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                        {productAttributesData.items.map((attribute: any) => (
                          <div key={attribute.id} className="bg-white rounded-lg p-3 border border-gray-200 shadow-sm">
                            <div className="flex items-start justify-between">
                              <div className="flex-1">
                                <div className="text-xs font-medium text-gray-500 uppercase tracking-wide mb-1">
                                  {attribute.key}
                                </div>
                                <div className="text-sm font-semibold text-gray-900">
                                  {attribute.value}
                                </div>
                              </div>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  ) : (
                    <div className="bg-gray-50 rounded-lg p-4 text-center text-gray-500 text-sm">
                      Bu ürün için henüz öznitelik tanımlanmamıştır.
                    </div>
                  )}
                </div>
              </div>
              <div className="flex-shrink-0 border-t border-gray-200 px-4 py-3 flex justify-end bg-white">
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
        <div className="fixed inset-0 z-[70] overflow-y-auto">
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <div className="fixed inset-0 z-[65] bg-gray-500 bg-opacity-75 transition-opacity" onClick={() => {
              setIsStockMovementModalOpen(false)
              setStockMovementFormData({
                productId: detailProductId || 0,
                type: 1,
                quantity: 0,
                unitPrice: getDefaultMovementUnitPrice(1),
                description: '',
              })
            }} />
            <div className="relative z-[70] transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6">
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
                if (stockMovementFormData.unitPrice <= 0) {
                  alert('Birim fiyat 0\'dan büyük olmalıdır!')
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
                        onChange={(e) => {
                          const newType = Number(e.target.value)
                          const defaultPrice = getDefaultMovementUnitPrice(newType)
                          setStockMovementFormData({
                            ...stockMovementFormData,
                            type: newType,
                            unitPrice: defaultPrice || 0,
                          })
                        }}
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
                      <label htmlFor="stock-movement-unitPrice" className="block text-sm font-medium text-gray-700">
                        {stockMovementFormData.type === 1 ? 'Satın Alma Fiyatı' : 'Satış Fiyatı'} *
                      </label>
                      <input
                        type="number"
                        step="0.01"
                        min="0"
                        id="stock-movement-unitPrice"
                        required
                        value={stockMovementFormData.unitPrice === 0 ? '' : stockMovementFormData.unitPrice}
                        onChange={(e) => {
                          const value = parseFloat(e.target.value)
                          setStockMovementFormData({
                            ...stockMovementFormData,
                            unitPrice: Number.isNaN(value) ? 0 : value,
                          })
                        }}
                        onBlur={(e) => {
                          if (e.target.value === '') {
                            setStockMovementFormData({ ...stockMovementFormData, unitPrice: 0 })
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
                      setStockMovementFormData({
                        productId: detailProductId || 0,
                        type: 1,
                        quantity: 0,
                        unitPrice: getDefaultMovementUnitPrice(1),
                        description: '',
                      })
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

