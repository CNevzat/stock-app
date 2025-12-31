import { useState, useEffect, useRef } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { stockMovementService } from '../services/stockMovementService'
import { signalRService } from '../services/signalRService'
import { TechnologyInfo } from '../components/TechnologyInfo'

export default function StockMovementsPage() {
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)
  const [pageSize] = useState(10)
  const [isExporting, setIsExporting] = useState(false)
  const [searchInput, setSearchInput] = useState('')
  const [searchTerm, setSearchTerm] = useState('')
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const searchInputRef = useRef<HTMLInputElement>(null)
  const isTypingRef = useRef(false)

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

  // SignalR setup
  useEffect(() => {
    signalRService.startConnection().catch((error) => {
      console.error('SignalR bağlantı hatası:', error)
    })

    const handleStockMovementCreated = () => {
      queryClient.invalidateQueries({ queryKey: ['stock-movements'] })
    }

    signalRService.onStockMovementCreated(handleStockMovementCreated)

    return () => {
      signalRService.offStockMovementCreated(handleStockMovementCreated)
    }
  }, [queryClient])

  // Fetch stock movements
  const { data: movementsData, isLoading, isFetching } = useQuery({
    queryKey: ['stock-movements', page, pageSize, searchTerm, startDate, endDate],
    queryFn: () => {
      return stockMovementService.getAll({
        pageNumber: page,
        pageSize,
        searchTerm: searchTerm || undefined,
        startDate: startDate || undefined,
        endDate: endDate || undefined,
      });
    }
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
  }, [isFetching, movementsData])

  if (isLoading) {
    return <div className="flex justify-center items-center h-64">Yükleniyor...</div>
  }

  return (
    <div className="px-4 sm:px-6 lg:px-8">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <div className="flex items-center gap-2">
            <h1 className="text-3xl font-semibold text-gray-900">Stok Hareketleri</h1>
            <TechnologyInfo
              technologies={[
                'Redis Cache (60s TTL) - Hızlı veri erişimi',
                'Elasticsearch - Gelişmiş arama (ürün, kategori, açıklama)',
                'SignalR - Real-time güncellemeler'
              ]}
              description="Arama sonuçları Redis'te cache'lenir, Elasticsearch ile hızlı arama yapılır."
            />
          </div>
          <p className="mt-2 text-sm text-gray-700">
            Tüm stok giriş ve çıkış hareketleri.
          </p>
        </div>
        <div className="mt-4 sm:ml-16 sm:mt-0 sm:flex-none">
          <button
            onClick={async () => {
              setIsExporting(true)
              try {
                await stockMovementService.exportExcel()
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
        </div>
      </div>

      {/* Search and Filter Bar */}
      <div className="mt-6 space-y-4">
        {/* Text Search */}
        <div className="relative">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          <input
            ref={searchInputRef}
            type="text"
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
            placeholder="Ürün, kategori veya açıklama ile ara..."
            className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
          />
          {searchInput && (
            <button
              onClick={() => {
                setSearchInput('')
                setSearchTerm('')
                setPage(1)
              }}
              className="absolute inset-y-0 right-0 pr-3 flex items-center"
            >
              <svg className="h-5 w-5 text-gray-400 hover:text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          )}
        </div>

        {/* Date Range Filter */}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 date-input-container">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Başlangıç Tarihi
            </label>
            <input
              type="date"
              value={startDate}
              onChange={(e) => {
                setStartDate(e.target.value)
                if (page !== 1) {
                  setPage(1)
                }
              }}
              className="block w-full px-3 py-2 border border-gray-300 rounded-lg leading-5 bg-white focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Bitiş Tarihi
            </label>
            <input
              type="date"
              value={endDate}
              onChange={(e) => {
                setEndDate(e.target.value)
                if (page !== 1) {
                  setPage(1)
                }
              }}
              min={startDate || undefined}
              className="block w-full px-3 py-2 border border-gray-300 rounded-lg leading-5 bg-white focus:outline-none focus:ring-1 focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
            />
          </div>
        </div>

        {/* Clear Filters Button */}
        {(startDate || endDate) && (
          <div className="flex justify-end">
            <button
              onClick={() => {
                setStartDate('')
                setEndDate('')
                setPage(1)
              }}
              className="text-sm text-gray-600 hover:text-gray-800 underline"
            >
              Tarih filtrelerini temizle
            </button>
          </div>
        )}
      </div>

      <div className="mt-8 flow-root">
        <div className="-mx-4 -my-2 overflow-x-auto sm:-mx-6 lg:-mx-8 rounded-lg shadow-sm border border-gray-200 bg-white">
          <div className="inline-block min-w-full py-2 align-middle sm:px-6 lg:px-8">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 whitespace-nowrap">
                    #
                  </th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[150px]">
                    Ürün
                  </th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[120px]">
                    Kategori
                  </th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap">
                    İşlem Tipi
                  </th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap">
                    Miktar
                  </th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap">
                    Mevcut Stok
                  </th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[200px]">
                    Açıklama
                  </th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap">
                    Tarih
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 bg-white">
                {movementsData?.items.map((movement: any, index: number) => (
                  <tr key={movement.id} className="hover:bg-gray-50 transition-colors duration-150">
                    <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900">
                      <span className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-gray-100 text-gray-600 font-semibold">
                        {(page - 1) * pageSize + index + 1}
                      </span>
                    </td>
                    <td className="px-3 py-4 text-sm text-gray-900 font-medium max-w-[150px]">
                      <div className="truncate">{movement.productName}</div>
                    </td>
                    <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-700">
                      {movement.categoryName || '-'}
                    </td>
                    <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-700">
                      {movement.type === 1 ? 'Giriş' : 'Çıkış'}
                    </td>
                    <td className="whitespace-nowrap px-3 py-4 text-sm font-semibold text-gray-900">
                      {movement.quantity}
                    </td>
                    <td className="whitespace-nowrap px-3 py-4 text-sm">
                      <span className="text-gray-900 font-medium">
                        {movement.currentStockQuantity ?? (movement as any).currentStockQuantity ?? 0}
                      </span>
                    </td>
                    <td className="px-3 py-4 text-sm text-gray-600 max-w-xs truncate">
                      {movement.description || '-'}
                    </td>
                    <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
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
        </div>
      </div>

      {/* Pagination */}
      {movementsData && movementsData.totalPages > 1 && (
        <div className="flex items-center justify-between border-t border-gray-200 bg-gray-50 px-4 py-3 sm:px-6 mt-4 rounded-lg">
          <div className="flex flex-1 justify-between sm:hidden">
            <button
              onClick={() => setPage(page - 1)}
              disabled={!movementsData.hasPreviousPage}
              className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Önceki
            </button>
            <button
              onClick={() => setPage(page + 1)}
              disabled={!movementsData.hasNextPage}
              className="relative ml-3 inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Sonraki
            </button>
          </div>
          <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
            <div>
              <p className="text-sm text-gray-800">
                <span className="font-medium">{movementsData.pageNumber}</span> / <span className="font-medium">{movementsData.totalPages}</span> sayfa gösteriliyor
              </p>
            </div>
            <div>
              <nav className="isolate inline-flex -space-x-px rounded-md shadow-sm">
                <button
                  onClick={() => setPage(page - 1)}
                  disabled={!movementsData.hasPreviousPage}
                  className="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-700 ring-1 ring-inset ring-gray-300 bg-white hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Önceki
                </button>
                <button
                  onClick={() => setPage(page + 1)}
                  disabled={!movementsData.hasNextPage}
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
  )
}


