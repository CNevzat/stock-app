import { useState, useEffect, useRef } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { productAttributeService } from '../services/productAttributeService'
import { productService } from '../services/productService'
import { signalRService } from '../services/signalRService'
import type { CreateProductAttributeCommand, UpdateProductAttributeCommand } from '../Api'

// Ürün renklerini belirle (aynı ürüne ait öznitelikler aynı renkte olacak)
const getProductColor = (productId: number) => {
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
  return colors[productId % colors.length];
};

export default function ProductAttributesPage() {
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)
  const [pageSize] = useState(10)
  const [searchInput, setSearchInput] = useState('')
  const [searchKey, setSearchKey] = useState('')
  const [productSearchInput, setProductSearchInput] = useState('')
  const searchInputRef = useRef<HTMLInputElement>(null)

  // Debounce search input - Preserve focus and cursor position
  useEffect(() => {
    const timer = setTimeout(() => {
      const input = searchInputRef.current
      const wasFocused = document.activeElement === input
      const cursorPosition = input?.selectionStart ?? null
      
      setSearchKey(searchInput)
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

  // SignalR - Real-time updates için event listener'lar
  useEffect(() => {
    // SignalR bağlantısını başlat
    signalRService.startConnection().catch((error) => {
      console.error('SignalR bağlantı hatası:', error)
    })

    // ProductAttribute Created event
    const handleProductAttributeCreated = () => {
      queryClient.invalidateQueries({ queryKey: ['product-attributes'] })
      queryClient.invalidateQueries({ queryKey: ['product-attributes-by-product'] })
    }

    // ProductAttribute Updated event
    const handleProductAttributeUpdated = () => {
      queryClient.invalidateQueries({ queryKey: ['product-attributes'] })
      queryClient.invalidateQueries({ queryKey: ['product-attributes-by-product'] })
    }

    // ProductAttribute Deleted event
    const handleProductAttributeDeleted = () => {
      queryClient.invalidateQueries({ queryKey: ['product-attributes'] })
      queryClient.invalidateQueries({ queryKey: ['product-attributes-by-product'] })
    }

    // Event listener'ları kaydet
    signalRService.onProductAttributeCreated(handleProductAttributeCreated)
    signalRService.onProductAttributeUpdated(handleProductAttributeUpdated)
    signalRService.onProductAttributeDeleted(handleProductAttributeDeleted)

    // Cleanup
    return () => {
      signalRService.offProductAttributeCreated(handleProductAttributeCreated)
      signalRService.offProductAttributeUpdated(handleProductAttributeUpdated)
      signalRService.offProductAttributeDeleted(handleProductAttributeDeleted)
    }
  }, [queryClient])

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [isBulkCreateModalOpen, setIsBulkCreateModalOpen] = useState(false)
  const [isEditModalOpen, setIsEditModalOpen] = useState(false)
  const [selectedAttributeId, setSelectedAttributeId] = useState<number | null>(null)
  const [showProductDropdown, setShowProductDropdown] = useState(false)
  const [formData, setFormData] = useState({
    productId: 0,
    key: '',
    value: '',
  })
  // Çoklu ekleme için state
  const [bulkFormData, setBulkFormData] = useState({
    productId: 0,
    attributes: [{ key: '', value: '' }],
  })
  const [isExporting, setIsExporting] = useState(false)

  // Fetch products for dropdown
  const { data: productsData } = useQuery({
    queryKey: ['products-all'],
    queryFn: () => productService.getAll({ pageNumber: 1, pageSize: 1000 }),
  })

  // Fetch attributes
  const { data: attributesData, isLoading } = useQuery({
    queryKey: ['product-attributes', page, pageSize, searchKey],
    queryFn: () => {
      return productAttributeService.getAll({
        pageNumber: page,
        pageSize,
        searchKey: searchKey && searchKey.trim() !== '' ? searchKey.trim() : undefined,
      });
    },
    enabled: true, // Her zaman aktif
  })

  // Seçilen ürüne ait öznitelikleri getir
  const { data: existingAttributes } = useQuery({
    queryKey: ['product-attributes-by-product', formData.productId, bulkFormData.productId],
    queryFn: () => {
      const productId = formData.productId || bulkFormData.productId
      if (!productId || productId === 0) return null
      return productAttributeService.getAll({
        pageNumber: 1,
        pageSize: 1000,
        productId: productId,
      });
    },
    enabled: (formData.productId > 0 || bulkFormData.productId > 0),
  })

  // Create mutation
  const createMutation = useMutation({
    mutationFn: (dto: CreateProductAttributeCommand) => productAttributeService.create(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['product-attributes'] })
      setIsCreateModalOpen(false)
      resetForm()
    },
  })

  // Update mutation
  const updateMutation = useMutation({
    mutationFn: ({dto }: { id: number; dto: UpdateProductAttributeCommand }) =>
      productAttributeService.update(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['product-attributes'] })
      setIsEditModalOpen(false)
      resetForm()
    },
  })

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: (id: number) => productAttributeService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['product-attributes'] })
    },
  })

  const resetForm = () => {
    setFormData({ productId: 0, key: '', value: '' })
    setSelectedAttributeId(null)
    setProductSearchInput('')
    setShowProductDropdown(false)
  }

  const resetBulkForm = () => {
    setBulkFormData({
      productId: 0,
      attributes: [{ key: '', value: '' }],
    })
    setProductSearchInput('')
    setShowProductDropdown(false)
  }

  const addBulkAttributeRow = () => {
    setBulkFormData({
      ...bulkFormData,
      attributes: [...bulkFormData.attributes, { key: '', value: '' }],
    })
  }

  const removeBulkAttributeRow = (index: number) => {
    if (bulkFormData.attributes.length > 1) {
      setBulkFormData({
        ...bulkFormData,
        attributes: bulkFormData.attributes.filter((_, i) => i !== index),
      })
    }
  }

  const updateBulkAttribute = (index: number, field: 'key' | 'value', value: string) => {
    const updatedAttributes = [...bulkFormData.attributes]
    updatedAttributes[index] = { ...updatedAttributes[index], [field]: value }
    setBulkFormData({
      ...bulkFormData,
      attributes: updatedAttributes,
    })
  }

  const handleBulkCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (bulkFormData.productId === 0) {
      alert('Lütfen bir ürün seçin!')
      return
    }

    // Boş olmayan öznitelikleri filtrele
    const validAttributes = bulkFormData.attributes.filter(
      attr => attr.key.trim() !== '' && attr.value.trim() !== ''
    )

    if (validAttributes.length === 0) {
      alert('En az bir geçerli öznitelik eklemelisiniz!')
      return
    }

    // Her bir öznitelik için mutation çağır
    try {
      const promises = validAttributes.map(attr =>
        productAttributeService.create({
          productId: bulkFormData.productId,
          key: attr.key.trim(),
          value: attr.value.trim(),
        })
      )
      
      await Promise.all(promises)
      
      queryClient.invalidateQueries({ queryKey: ['product-attributes'] })
      setIsBulkCreateModalOpen(false)
      resetBulkForm()
    } catch (error) {
      console.error('Çoklu ekleme hatası:', error)
      alert('Bazı öznitelikler eklenirken hata oluştu. Lütfen tekrar deneyin.')
    }
  }

  const handleCreate = (e: React.FormEvent) => {
    e.preventDefault()
    createMutation.mutate(formData)
  }

  const handleUpdate = (e: React.FormEvent) => {
    e.preventDefault()
    if (selectedAttributeId) {
      updateMutation.mutate({
        id: selectedAttributeId,
        dto: {
          id: selectedAttributeId,
          key: formData.key,
          value: formData.value,
        },
      })
    }
  }

  const openEditModal = (attribute: any) => {
    setSelectedAttributeId(attribute.id)
    setFormData({
      productId: attribute.productId,
      key: attribute.key,
      value: attribute.value,
    })
    setIsEditModalOpen(true)
  }

  if (isLoading) {
    return <div className="flex justify-center items-center h-64">Yükleniyor...</div>
  }

  return (
    <div className="px-2 sm:px-4 lg:px-6">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-3xl font-semibold text-gray-900">Ürün Öznitelikleri</h1>
          <p className="mt-2 text-sm text-gray-700">
            Tüm ürün özniteliklerinin listesi.
          </p>
        </div>
        <div className="mt-4 sm:ml-16 sm:mt-0 sm:flex-none flex gap-2">
          <button
            onClick={async () => {
              setIsExporting(true)
              try {
                await productAttributeService.exportExcel()
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
              resetBulkForm()
              setIsBulkCreateModalOpen(true)
            }}
            className="inline-flex items-center gap-x-2 rounded-lg bg-gradient-to-r from-blue-600 to-blue-700 px-4 py-2.5 text-sm font-semibold text-white shadow-lg hover:from-blue-700 hover:to-blue-800 hover:shadow-xl transform hover:scale-105 transition-all duration-200"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            Öznitelik Ekle
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="mt-4">
        <input
          ref={searchInputRef}
          type="text"
          placeholder="Ara..."
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          className="block rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
        />
      </div>

      <div className="mt-8 flow-root">
        <div className="-mx-2 -my-2 overflow-x-auto sm:-mx-4 lg:-mx-6 rounded-xl shadow-lg backdrop-blur-lg border border-white/10">
          <div className="inline-block min-w-full py-2 align-middle">
            <table className="min-w-full divide-y divide-gray-300/20">
              <thead className="backdrop-blur-md">
                <tr>
                  <th className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 whitespace-nowrap">
                    #
                  </th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[150px] max-w-[150px]">
                    Ürün
                  </th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[150px] max-w-[150px]">
                    Anahtar
                  </th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[150px] max-w-[150px]">
                    Değer
                  </th>
                  <th className="relative py-3.5 pl-3 pr-6 sm:pr-4 whitespace-nowrap">
                    <span className="sr-only">İşlemler</span>
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200/20 backdrop-blur-md">
                {attributesData?.items?.map((attribute, index) => (
                  <tr key={attribute.id} className="hover:bg-gray-50 transition-colors duration-150">
                    <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900">
                      <span className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-gray-100 text-gray-600 font-semibold">
                        {(page - 1) * pageSize + index + 1}
                      </span>
                    </td>
                    <td className="px-3 py-4 text-sm max-w-[150px]">
                      <div className="overflow-hidden">
                        {(() => {
                          const color = getProductColor(attribute.productId || 0);
                          return (
                            <span className={`inline-block max-w-full rounded-md px-2.5 py-1 text-xs font-medium ring-1 ring-inset truncate ${color.bg} ${color.text} ${color.ring}`}>
                              {attribute.productName}
                            </span>
                          );
                        })()}
                      </div>
                    </td>
                    <td className="px-3 py-4 text-sm font-medium text-gray-900 max-w-[150px]">
                      <div className="flex items-center min-w-0">
                        <svg className="w-4 h-4 text-purple-500 mr-1.5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
                        </svg>
                        <span className="truncate min-w-0">{attribute.key}</span>
                      </div>
                    </td>
                    <td className="px-3 py-4 text-sm text-gray-600 max-w-[150px]">
                      <div className="flex items-center min-w-0">
                        <svg className="w-4 h-4 text-gray-400 mr-1.5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                        </svg>
                        <span className="truncate min-w-0">{attribute.value}</span>
                      </div>
                    </td>
                    <td className="relative whitespace-nowrap py-4 pl-3 pr-6 text-right text-sm font-medium sm:pr-4">
                      <div className="flex justify-end gap-2">
                        <button
                          onClick={() => openEditModal(attribute)}
                          className="inline-flex items-center gap-x-1.5 rounded-lg bg-blue-600 px-3 py-1.5 text-xs font-semibold text-white shadow-sm hover:bg-blue-500 transition-all duration-200 hover:shadow-md"
                        >
                          <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                          </svg>
                          Düzenle
                        </button>
                        <button
                          onClick={() => {
                            if (confirm('Silmek istediğinize emin misiniz?')) {
                              attribute.id && deleteMutation.mutate(attribute.id)
                            }
                          }}
                          className="inline-flex items-center gap-x-1.5 rounded-lg bg-red-600 px-3 py-1.5 text-xs font-semibold text-white shadow-sm hover:bg-red-500 transition-all duration-200 hover:shadow-md"
                        >
                          <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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
      {attributesData && attributesData.totalPages && attributesData.totalPages > 1 && (
        <div className="flex items-center justify-between border-t border-white/20 bg-white/30 backdrop-blur-lg px-4 py-3 sm:px-6 mt-4 rounded-xl">
          <div className="flex flex-1 justify-between sm:hidden">
            <button
              onClick={() => setPage(page - 1)}
              disabled={!attributesData.hasPreviousPage}
              className="relative inline-flex items-center rounded-md border border-white/50 bg-white/70 backdrop-blur-md px-4 py-2 text-sm font-medium text-gray-900 hover:bg-white/80 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Önceki
            </button>
            <button
              onClick={() => setPage(page + 1)}
              disabled={!attributesData.hasNextPage}
              className="relative ml-3 inline-flex items-center rounded-md border border-white/50 bg-white/70 backdrop-blur-md px-4 py-2 text-sm font-medium text-gray-900 hover:bg-white/80 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Sonraki
            </button>
          </div>
          <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
            <div>
              <p className="text-sm text-gray-800">
                <span className="font-medium">{attributesData.pageNumber}</span> / <span className="font-medium">{attributesData.totalPages}</span> sayfa gösteriliyor
              </p>
            </div>
            <div>
              <nav className="isolate inline-flex -space-x-px rounded-md shadow-sm">
                <button
                  onClick={() => setPage(page - 1)}
                  disabled={!attributesData.hasPreviousPage}
                  className="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-800 ring-1 ring-inset ring-white/50 bg-white/70 backdrop-blur-md hover:bg-white/80 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Önceki
                </button>
                <button
                  onClick={() => setPage(page + 1)}
                  disabled={!attributesData.hasNextPage}
                  className="relative inline-flex items-center rounded-r-md px-2 py-2 text-gray-800 ring-1 ring-inset ring-white/50 bg-white/70 backdrop-blur-md hover:bg-white/80 disabled:opacity-50 disabled:cursor-not-allowed"
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
                    Ürün Özniteliği Oluştur
                  </h3>
                  <div className="space-y-4">
                    <div>
                      <label htmlFor="productSearch" className="block text-sm font-medium text-gray-700 mb-2">
                        Ürün Seçin
                      </label>
                      <div className="relative">
                        <input
                          type="text"
                          id="productSearch"
                          placeholder="Ürün ara..."
                          value={productSearchInput}
                          onChange={(e) => {
                            setProductSearchInput(e.target.value)
                            setShowProductDropdown(true)
                            if (formData.productId > 0) {
                              setFormData({ ...formData, productId: 0 })
                            }
                          }}
                          onFocus={() => setShowProductDropdown(true)}
                          className="block w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-4 py-2.5 border bg-white"
                        />
                        <div className="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
                          <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                          </svg>
                        </div>
                        {/* Arama Sonuçları Listesi */}
                        {showProductDropdown && productSearchInput && formData.productId === 0 && (
                          <div className="absolute z-10 mt-1 w-full bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
                            {productsData?.items
                              ?.filter((product: any) => 
                                product.name?.toLowerCase().includes(productSearchInput.toLowerCase())
                              )
                              .length === 0 ? (
                                <div className="px-4 py-3 text-sm text-gray-500">
                                  Ürün bulunamadı
                                </div>
                              ) : (
                                productsData?.items
                                  ?.filter((product: any) => 
                                    product.name?.toLowerCase().includes(productSearchInput.toLowerCase())
                                  )
                                  .map((product: any) => (
                                    <button
                                      key={product.id}
                                      type="button"
                                      onClick={() => {
                                        setFormData({ ...formData, productId: product.id })
                                        setProductSearchInput(`${product.name} (Stok: ${product.stockQuantity})`)
                                        setShowProductDropdown(false)
                                      }}
                                      className="w-full text-left px-4 py-3 hover:bg-gray-50 focus:bg-gray-50 focus:outline-none transition-colors border-b border-gray-100 last:border-b-0"
                                    >
                                      <div className="font-medium text-gray-900">{product.name}</div>
                                      <div className="text-sm text-gray-500">Stok: {product.stockQuantity}</div>
                                    </button>
                                  ))
                              )}
                          </div>
                        )}
                      </div>
                      {formData.productId > 0 && (
                        <div className="mt-2 space-y-2">
                          <div className="p-2 bg-indigo-50 border border-indigo-200 rounded-lg">
                            <p className="text-sm text-indigo-700">
                              <span className="font-medium">Seçilen:</span> {
                                productsData?.items?.find((p: any) => p.id === formData.productId)?.name
                              }
                            </p>
                          </div>
                          {/* Mevcut Öznitelikler */}
                          {existingAttributes?.items && existingAttributes.items.length > 0 && (
                            <div className="p-3 bg-amber-50 border border-amber-200 rounded-lg">
                              <p className="text-sm font-medium text-amber-800 mb-2">
                                Bu ürüne ait mevcut öznitelikler:
                              </p>
                              <div className="space-y-1 max-h-40 overflow-y-auto">
                                {existingAttributes.items.map((attr: any) => (
                                  <div key={attr.id} className="text-xs text-amber-700 bg-white px-2 py-1 rounded border border-amber-100">
                                    <span className="font-medium">{attr.key}:</span> {attr.value}
                                  </div>
                                ))}
                              </div>
                            </div>
                          )}
                        </div>
                      )}
                    </div>
                    <div>
                      <label htmlFor="key" className="block text-sm font-medium text-gray-700">
                        Anahtar
                      </label>
                      <input
                        type="text"
                        id="key"
                        required
                        value={formData.key}
                        onChange={(e) => setFormData({ ...formData, key: e.target.value })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="value" className="block text-sm font-medium text-gray-700">
                        Değer
                      </label>
                      <input
                        type="text"
                        id="value"
                        required
                        value={formData.value}
                        onChange={(e) => setFormData({ ...formData, value: e.target.value })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      />
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

      {/* Bulk Create Modal */}
      {isBulkCreateModalOpen && (
        <div className="fixed inset-0 z-10 overflow-y-auto">
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={() => {
              setIsBulkCreateModalOpen(false)
              resetBulkForm()
            }} />
            <div className="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-2xl sm:p-6 max-h-[90vh] overflow-y-auto">
              <form onSubmit={handleBulkCreate}>
                <div>
                  <h3 className="text-lg font-semibold leading-6 text-gray-900 mb-4">
                    Çoklu Ürün Özniteliği Oluştur
                  </h3>
                  <div className="space-y-4">
                    <div>
                      <label htmlFor="bulkProductSearch" className="block text-sm font-medium text-gray-700 mb-2">
                        Ürün Seçin
                      </label>
                      <div className="relative">
                        <input
                          type="text"
                          id="bulkProductSearch"
                          placeholder="Ürün ara..."
                          value={productSearchInput}
                          onChange={(e) => {
                            setProductSearchInput(e.target.value)
                            setShowProductDropdown(true)
                            if (bulkFormData.productId > 0) {
                              setBulkFormData({ ...bulkFormData, productId: 0 })
                            }
                          }}
                          onFocus={() => setShowProductDropdown(true)}
                          className="block w-full rounded-lg border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-4 py-2.5 border bg-white"
                        />
                        <div className="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
                          <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                          </svg>
                        </div>
                        {/* Arama Sonuçları Listesi */}
                        {showProductDropdown && productSearchInput && bulkFormData.productId === 0 && (
                          <div className="absolute z-10 mt-1 w-full bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
                            {productsData?.items
                              ?.filter((product: any) => 
                                product.name?.toLowerCase().includes(productSearchInput.toLowerCase())
                              )
                              .length === 0 ? (
                                <div className="px-4 py-3 text-sm text-gray-500">
                                  Ürün bulunamadı
                                </div>
                              ) : (
                                productsData?.items
                                  ?.filter((product: any) => 
                                    product.name?.toLowerCase().includes(productSearchInput.toLowerCase())
                                  )
                                  .map((product: any) => (
                                    <button
                                      key={product.id}
                                      type="button"
                                      onClick={() => {
                                        setBulkFormData({ ...bulkFormData, productId: product.id })
                                        setProductSearchInput(`${product.name} (Stok: ${product.stockQuantity})`)
                                        setShowProductDropdown(false)
                                      }}
                                      className="w-full text-left px-4 py-3 hover:bg-gray-50 focus:bg-gray-50 focus:outline-none transition-colors border-b border-gray-100 last:border-b-0"
                                    >
                                      <div className="font-medium text-gray-900">{product.name}</div>
                                      <div className="text-sm text-gray-500">Stok: {product.stockQuantity}</div>
                                    </button>
                                  ))
                              )}
                          </div>
                        )}
                      </div>
                      {bulkFormData.productId > 0 && (
                        <div className="mt-2 space-y-2">
                          <div className="p-2 bg-indigo-50 border border-indigo-200 rounded-lg">
                            <p className="text-sm text-indigo-700">
                              <span className="font-medium">Seçilen:</span> {
                                productsData?.items?.find((p: any) => p.id === bulkFormData.productId)?.name
                              }
                            </p>
                          </div>
                          {/* Mevcut Öznitelikler */}
                          {existingAttributes?.items && existingAttributes.items.length > 0 && (
                            <div className="p-3 bg-amber-50 border border-amber-200 rounded-lg">
                              <p className="text-sm font-medium text-amber-800 mb-2">
                                Bu ürüne ait mevcut öznitelikler:
                              </p>
                              <div className="space-y-1 max-h-40 overflow-y-auto">
                                {existingAttributes.items.map((attr: any) => (
                                  <div key={attr.id} className="text-xs text-amber-700 bg-white px-2 py-1 rounded border border-amber-100">
                                    <span className="font-medium">{attr.key}:</span> {attr.value}
                                  </div>
                                ))}
                              </div>
                            </div>
                          )}
                        </div>
                      )}
                    </div>
                    
                    <div>
                      <div className="flex items-center justify-between mb-2">
                        <label className="block text-sm font-medium text-gray-700">
                          Öznitelikler
                        </label>
                        <button
                          type="button"
                          onClick={addBulkAttributeRow}
                          className="inline-flex items-center gap-x-1 rounded-md bg-green-600 px-2.5 py-1.5 text-xs font-semibold text-white shadow-sm hover:bg-green-500"
                        >
                          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                          </svg>
                          Satır Ekle
                        </button>
                      </div>
                      
                      <div className="space-y-3 max-h-96 overflow-y-auto">
                        {bulkFormData.attributes.map((attr, index) => (
                          <div key={index} className="flex gap-2 items-start p-3 border border-gray-200 rounded-lg bg-gray-50">
                            <div className="flex-1">
                              <label className="block text-xs font-medium text-gray-700 mb-1">
                                Anahtar
                              </label>
                              <input
                                type="text"
                                required
                                value={attr.key}
                                onChange={(e) => updateBulkAttribute(index, 'key', e.target.value)}
                                placeholder="Örn: Renk, Boyut..."
                                className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                              />
                            </div>
                            <div className="flex-1">
                              <label className="block text-xs font-medium text-gray-700 mb-1">
                                Değer
                              </label>
                              <input
                                type="text"
                                required
                                value={attr.value}
                                onChange={(e) => updateBulkAttribute(index, 'value', e.target.value)}
                                placeholder="Örn: Kırmızı, XL..."
                                className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                              />
                            </div>
                            <div className="flex items-end">
                              <button
                                type="button"
                                onClick={() => removeBulkAttributeRow(index)}
                                disabled={bulkFormData.attributes.length === 1}
                                className="inline-flex items-center rounded-md bg-red-600 px-2 py-2 text-sm font-semibold text-white shadow-sm hover:bg-red-500 disabled:opacity-50 disabled:cursor-not-allowed"
                              >
                                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                </svg>
                              </button>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>
                </div>
                <div className="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
                  <button
                    type="submit"
                    className="inline-flex w-full justify-center rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 sm:col-start-2"
                  >
                    Tümünü Ekle
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setIsBulkCreateModalOpen(false)
                      resetBulkForm()
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
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={() => {
              setIsEditModalOpen(false)
              resetForm()
            }} />
            <div className="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6">
              <form onSubmit={handleUpdate}>
                <div>
                  <h3 className="text-lg font-semibold leading-6 text-gray-900 mb-4">
                    Ürün Özniteliği Düzenle
                  </h3>
                  <div className="space-y-4">
                    <div>
                      <label htmlFor="edit-key" className="block text-sm font-medium text-gray-700">
                        Anahtar
                      </label>
                      <input
                        type="text"
                        id="edit-key"
                        value={formData.key}
                        onChange={(e) => setFormData({ ...formData, key: e.target.value })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="edit-value" className="block text-sm font-medium text-gray-700">
                        Değer
                      </label>
                      <input
                        type="text"
                        id="edit-value"
                        value={formData.value}
                        onChange={(e) => setFormData({ ...formData, value: e.target.value })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm px-3 py-2 border"
                      />
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
                    onClick={() => {
                      setIsEditModalOpen(false)
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
    </div>
  )
}

