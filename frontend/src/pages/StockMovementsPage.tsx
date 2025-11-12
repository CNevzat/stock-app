import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { stockMovementService } from '../services/stockMovementService'

export default function StockMovementsPage() {
  const [page, setPage] = useState(1)
  const [pageSize] = useState(10)
  const [isExporting, setIsExporting] = useState(false)

  // Fetch stock movements
  const { data: movementsData, isLoading } = useQuery({
    queryKey: ['stock-movements', page, pageSize],
    queryFn: () => {
      return stockMovementService.getAll({
        pageNumber: page,
        pageSize,
      });
    }
  })

  if (isLoading) {
    return <div className="flex justify-center items-center h-64">Yükleniyor...</div>
  }

  return (
    <div className="px-4 sm:px-6 lg:px-8">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-3xl font-semibold text-gray-900">Stok Hareketleri</h1>
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


