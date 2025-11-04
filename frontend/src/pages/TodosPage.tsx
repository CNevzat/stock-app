import { useState, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { todoService } from '../services/todoService'

const STATUS_OPTIONS = [
  { value: 1, label: 'Yapılacak', color: 'bg-gray-100 text-gray-800' },
  { value: 2, label: 'Devam Ediyor', color: 'bg-blue-100 text-blue-800' },
  { value: 3, label: 'Tamamlandı', color: 'bg-green-100 text-green-800' },
]

const PRIORITY_OPTIONS = [
  { value: 1, label: 'Düşük', color: 'bg-green-50 text-green-700 ring-green-600/20' },
  { value: 2, label: 'Orta', color: 'bg-yellow-50 text-yellow-700 ring-yellow-600/20' },
  { value: 3, label: 'Yüksek', color: 'bg-red-50 text-red-700 ring-red-600/20' },
]

export default function TodosPage() {
  const queryClient = useQueryClient()
  const [activeTab, setActiveTab] = useState<'active' | 'completed'>('active')
  const [page, setPage] = useState(1)
  const [pageSize] = useState(10)
  const [filterPriority, setFilterPriority] = useState<number | undefined>()
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [isEditModalOpen, setIsEditModalOpen] = useState(false)
  const [selectedTodoId, setSelectedTodoId] = useState<number | null>(null)
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    status: 1,
    priority: 2,
  })
  
  // Tab değiştiğinde sayfa numarasını sıfırla
  useEffect(() => {
    setPage(1)
  }, [activeTab])
  
  // Tab'a göre status filtresi - Aktif görevler için tamamlanmamış olanları al
  // Backend'den tüm verileri alıp frontend'de filtrelemek yerine, backend'e doğru filter gönderelim
  // Aktif görevler: status !== 3 (completed), Tamamlanan: status === 3
  
  // Fetch todos - backend'den tüm verileri al
  const { data: todosDataRaw, isLoading } = useQuery({
    queryKey: ['todos-all', filterPriority, activeTab],
    queryFn: () => {
      return todoService.getAll({
        pageNumber: 1,
        pageSize: 1000, // Tüm görevleri al, frontend'de filtrele
        priority: filterPriority,
      });
    },
    refetchOnMount: true, // Tab değiştiğinde yeniden fetch
  })

  // Frontend'de tab'a göre filtrele ve sırala
  const todosData = todosDataRaw ? {
    ...(todosDataRaw as any),
    items: ((todosDataRaw as any).items || [])
      .filter((todo: any) => {
        if (activeTab === 'completed') {
          // Tamamlananlar sekmesi: Sadece status === 3 olanlar
          return todo.status === 3
        } else {
          // Aktif görevler sekmesi: Sadece tamamlanmamış olanlar (status !== 3)
          // Yapılacak (status === 1) ve Devam Ediyor (status === 2) görevler
          return todo.status !== 3
        }
      })
      .sort((a: any, b: any) => {
        // Aktif görevler için: Öncelik yüksekten düşüğe (3=High, 2=Medium, 1=Low)
        // Tamamlananlar için: Sadece tarihe göre (en yeni önce)
        if (activeTab === 'active') {
          // Önce önceliğe göre (yüksekten düşüğe: 3 > 2 > 1)
          if (b.priority !== a.priority) {
            return b.priority - a.priority
          }
          // Aynı öncelikteyse status'a göre (Devam Ediyor > Yapılacak)
          if (a.status !== b.status) {
            return b.status - a.status // 2 > 1 (Devam Ediyor önce)
          }
          // Aynı öncelik ve status'taysa tarihe göre (en yeni önce)
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        } else {
          // Tamamlananlar için: En yeni önce
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        }
      })
      .slice((page - 1) * pageSize, page * pageSize)
  } : undefined

  // Pagination bilgilerini güncelle
  const totalFilteredItems = (todosDataRaw as any)?.items?.filter((todo: any) => 
    activeTab === 'completed' ? todo.status === 3 : todo.status !== 3
  ).length || 0
  const totalPages = Math.ceil(totalFilteredItems / pageSize)
  const paginatedTodosData = todosData ? {
    ...todosData,
    pageNumber: page,
    pageSize,
    totalCount: totalFilteredItems,
    totalPages,
    hasPreviousPage: page > 1,
    hasNextPage: page < totalPages,
  } : undefined

  // Create mutation
  const createMutation = useMutation({
    mutationFn: (data: typeof formData) => todoService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['todos-all'] })
      setIsCreateModalOpen(false)
      resetForm()
    },
  })

  // Update mutation
  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<typeof formData> }) =>
      todoService.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['todos-all'] })
      setIsEditModalOpen(false)
      resetForm()
    },
  })

  // Toggle complete mutation (checkbox için)
  const toggleCompleteMutation = useMutation({
    mutationFn: ({ id, isCompleted }: { id: number; isCompleted: boolean }) =>
      todoService.update(id, { status: isCompleted ? 3 : 1 }),
    onMutate: async ({ id, isCompleted }) => {
      // Optimistic update - hemen güncelle
      await queryClient.cancelQueries({ queryKey: ['todos-all'] })
      const previousData = queryClient.getQueryData(['todos-all'])
      
      queryClient.setQueryData(['todos-all'], (old: any) => {
        if (!old) return old
        return {
          ...old,
          items: old.items.map((todo: any) =>
            todo.id === id ? { ...todo, status: isCompleted ? 3 : 1 } : todo
          )
        }
      })
      
      return { previousData }
    },
    onError: (_err, _variables, context) => {
      // Hata durumunda önceki veriyi geri yükle
      if (context?.previousData) {
        queryClient.setQueryData(['todos-all'], context.previousData)
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['todos-all'] })
    },
  })

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: (id: number) => todoService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['todos-all'] })
    },
  })

  const resetForm = () => {
    setFormData({ title: '', description: '', status: 1, priority: 2 })
    setSelectedTodoId(null)
  }

  const handleCreate = (e: React.FormEvent) => {
    e.preventDefault()
    createMutation.mutate(formData)
  }

  const handleUpdate = (e: React.FormEvent) => {
    e.preventDefault()
    if (selectedTodoId) {
      updateMutation.mutate({
        id: selectedTodoId,
        data: formData,
      })
    }
  }

  const openEditModal = (todo: any) => {
    setSelectedTodoId(todo.id)
    setFormData({
      title: todo.title,
      description: todo.description || '',
      status: todo.status,
      priority: todo.priority,
    })
    setIsEditModalOpen(true)
  }

  const getStatusColor = (status: number) => {
    return STATUS_OPTIONS.find(s => s.value === status)?.color || 'bg-gray-100 text-gray-800'
  }

  const getPriorityColor = (priority: number) => {
    return PRIORITY_OPTIONS.find(p => p.value === priority)?.color || 'bg-gray-50 text-gray-700'
  }

  const getPriorityLabel = (priority: number) => {
    return PRIORITY_OPTIONS.find(p => p.value === priority)?.label || 'Bilinmiyor'
  }

  const getStatusLabel = (status: number) => {
    return STATUS_OPTIONS.find(s => s.value === status)?.label || 'Bilinmiyor'
  }

  if (isLoading) {
    return <div className="flex justify-center items-center h-64">Yükleniyor...</div>
  }

  return (
    <div className="px-4 sm:px-6 lg:px-8">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-3xl font-semibold text-gray-900">Yapılacaklar</h1>
          <p className="mt-2 text-sm text-gray-700">
            Görevlerinizi takip edin ve yönetin.
          </p>
        </div>
        {activeTab === 'active' && (
          <div className="mt-4 sm:ml-16 sm:mt-0 sm:flex-none">
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
              Yeni Görev Ekle
            </button>
          </div>
        )}
      </div>

      {/* Tabs */}
      <div className="mt-6 border-b border-gray-200">
        <nav className="-mb-px flex space-x-8" aria-label="Tabs">
          <button
            onClick={() => {
              setActiveTab('active')
              setPage(1)
            }}
            className={`whitespace-nowrap border-b-2 py-4 px-1 text-sm font-medium ${
              activeTab === 'active'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'
            }`}
          >
            Aktif Görevler
          </button>
          <button
            onClick={() => {
              setActiveTab('completed')
              setPage(1)
            }}
            className={`whitespace-nowrap border-b-2 py-4 px-1 text-sm font-medium ${
              activeTab === 'completed'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'
            }`}
          >
            Tamamlanan Görevler
          </button>
        </nav>
      </div>

      {/* Filters */}
      {activeTab === 'active' && (
        <div className="mt-4 flex gap-4">
          <select
            value={filterPriority || ''}
            onChange={(e) => {
              setFilterPriority(e.target.value ? Number(e.target.value) : undefined)
              setPage(1)
            }}
            className="block rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
          >
            <option value="">Tüm Öncelikler</option>
            {PRIORITY_OPTIONS.map(priority => (
              <option key={priority.value} value={priority.value}>{priority.label}</option>
            ))}
          </select>
        </div>
      )}

      <div className="mt-8 flow-root">
        <div className="-mx-4 -my-2 overflow-x-auto sm:-mx-6 lg:-mx-8 rounded-xl shadow-lg backdrop-blur-lg border border-white/10">
          <div className="inline-block min-w-full py-2 align-middle sm:px-6 lg:px-8">
            <table className="min-w-full divide-y divide-gray-300/20">
              <thead className="backdrop-blur-md">
                <tr>
                  {activeTab === 'active' && (
                    <th className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 whitespace-nowrap">
                      <span className="sr-only">Tamamlandı</span>
                    </th>
                  )}
                  <th className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 whitespace-nowrap">
                    #
                  </th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[200px]">
                    Başlık
                  </th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap min-w-[200px]">
                    Açıklama
                  </th>
                  {activeTab === 'completed' && (
                    <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap">
                      Durum
                    </th>
                  )}
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900 whitespace-nowrap">
                    Öncelik
                  </th>
                  <th className="relative py-3.5 pl-3 pr-6 sm:pr-4 whitespace-nowrap">
                    <span className="sr-only">İşlemler</span>
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200/20 backdrop-blur-md">
                {paginatedTodosData?.items.map((todo: any, index: number) => (
                  <tr key={todo.id} className={`hover:bg-gray-50 transition-colors duration-150 ${activeTab === 'completed' ? 'opacity-75' : ''}`}>
                    {activeTab === 'active' && (
                      <td className="whitespace-nowrap py-4 pl-4 pr-3">
                        <input
                          type="checkbox"
                          checked={false}
                          onChange={(e) => {
                            toggleCompleteMutation.mutate({ id: todo.id, isCompleted: e.target.checked })
                            // Görev tamamlandığında (status === 3) bu sekmede gösterilmemeli
                            // Filtreleme zaten status !== 3 olduğu için otomatik olarak çıkacak
                          }}
                          className="h-5 w-5 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500 cursor-pointer"
                        />
                      </td>
                    )}
                    <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900">
                      <span className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-gray-100 text-gray-600 font-semibold">
                        {(page - 1) * pageSize + index + 1}
                      </span>
                    </td>
                    <td className={`px-3 py-4 text-sm font-medium max-w-[200px] ${activeTab === 'completed' ? 'line-through text-gray-500' : 'text-gray-900'}`}>
                      <div className="truncate">{todo.title}</div>
                    </td>
                    <td className="px-3 py-4 text-sm text-gray-600 max-w-[200px]">
                      <div className="truncate">{todo.description || '-'}</div>
                    </td>
                    {activeTab === 'completed' && (
                      <td className="whitespace-nowrap px-3 py-4 text-sm">
                        <span className={`inline-flex items-center rounded-md px-2.5 py-1 text-xs font-medium ${getStatusColor(todo.status)}`}>
                          {getStatusLabel(todo.status)}
                        </span>
                      </td>
                    )}
                    <td className="whitespace-nowrap px-3 py-4 text-sm">
                      <span className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset ${getPriorityColor(todo.priority)}`}>
                        {getPriorityLabel(todo.priority)}
                      </span>
                    </td>
                    <td className="relative whitespace-nowrap py-4 pl-3 pr-6 text-right text-sm font-medium sm:pr-4">
                      <div className="flex justify-end gap-2">
                        <button
                          onClick={() => openEditModal(todo)}
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
                              deleteMutation.mutate(todo.id)
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
      {paginatedTodosData && paginatedTodosData.totalPages > 1 && (
        <div className="flex items-center justify-between border-t border-white/20 bg-white/30 backdrop-blur-lg px-4 py-3 sm:px-6 mt-4 rounded-xl">
          <div className="flex flex-1 justify-between sm:hidden">
            <button
              onClick={() => setPage(page - 1)}
              disabled={!paginatedTodosData.hasPreviousPage}
              className="relative inline-flex items-center rounded-md border border-white/50 bg-white/70 backdrop-blur-md px-4 py-2 text-sm font-medium text-gray-900 hover:bg-white/80 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Önceki
            </button>
            <button
              onClick={() => setPage(page + 1)}
              disabled={!paginatedTodosData.hasNextPage}
              className="relative ml-3 inline-flex items-center rounded-md border border-white/50 bg-white/70 backdrop-blur-md px-4 py-2 text-sm font-medium text-gray-900 hover:bg-white/80 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Sonraki
            </button>
          </div>
          <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
            <div>
              <p className="text-sm text-gray-800">
                <span className="font-medium">{paginatedTodosData.pageNumber}</span> / <span className="font-medium">{paginatedTodosData.totalPages}</span> sayfa gösteriliyor
              </p>
            </div>
            <div>
              <nav className="isolate inline-flex -space-x-px rounded-md shadow-sm">
                <button
                  onClick={() => setPage(page - 1)}
                  disabled={!paginatedTodosData.hasPreviousPage}
                  className="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-800 ring-1 ring-inset ring-white/50 bg-white/70 backdrop-blur-md hover:bg-white/80 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Önceki
                </button>
                <button
                  onClick={() => setPage(page + 1)}
                  disabled={!paginatedTodosData.hasNextPage}
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
                    Yeni Görev Oluştur
                  </h3>
                  <div className="space-y-4">
                    <div>
                      <label htmlFor="title" className="block text-sm font-medium text-gray-700">
                        Başlık
                      </label>
                      <input
                        type="text"
                        id="title"
                        required
                        value={formData.title}
                        onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="description" className="block text-sm font-medium text-gray-700">
                        Açıklama
                      </label>
                      <textarea
                        id="description"
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                        rows={3}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="status" className="block text-sm font-medium text-gray-700">
                        Durum
                      </label>
                      <select
                        id="status"
                        value={formData.status}
                        onChange={(e) => setFormData({ ...formData, status: Number(e.target.value) })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      >
                        {STATUS_OPTIONS.map(status => (
                          <option key={status.value} value={status.value}>{status.label}</option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label htmlFor="priority" className="block text-sm font-medium text-gray-700">
                        Öncelik
                      </label>
                      <select
                        id="priority"
                        value={formData.priority}
                        onChange={(e) => setFormData({ ...formData, priority: Number(e.target.value) })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      >
                        {PRIORITY_OPTIONS.map(priority => (
                          <option key={priority.value} value={priority.value}>{priority.label}</option>
                        ))}
                      </select>
                    </div>
                  </div>
                </div>
                <div className="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
                  <button
                    type="submit"
                    disabled={createMutation.isPending}
                    className="inline-flex w-full justify-center rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 sm:col-start-2 disabled:opacity-50"
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
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={() => {
              setIsEditModalOpen(false)
              resetForm()
            }} />
            <div className="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6">
              <form onSubmit={handleUpdate}>
                <div>
                  <h3 className="text-lg font-semibold leading-6 text-gray-900 mb-4">
                    Görevi Düzenle
                  </h3>
                  <div className="space-y-4">
                    <div>
                      <label htmlFor="edit-title" className="block text-sm font-medium text-gray-700">
                        Başlık
                      </label>
                      <input
                        type="text"
                        id="edit-title"
                        required
                        value={formData.title}
                        onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
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
                        rows={3}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    <div>
                      <label htmlFor="edit-status" className="block text-sm font-medium text-gray-700">
                        Durum
                      </label>
                      <select
                        id="edit-status"
                        value={formData.status}
                        onChange={(e) => setFormData({ ...formData, status: Number(e.target.value) })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      >
                        {STATUS_OPTIONS.map(status => (
                          <option key={status.value} value={status.value}>{status.label}</option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label htmlFor="edit-priority" className="block text-sm font-medium text-gray-700">
                        Öncelik
                      </label>
                      <select
                        id="edit-priority"
                        value={formData.priority}
                        onChange={(e) => setFormData({ ...formData, priority: Number(e.target.value) })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      >
                        {PRIORITY_OPTIONS.map(priority => (
                          <option key={priority.value} value={priority.value}>{priority.label}</option>
                        ))}
                      </select>
                    </div>
                  </div>
                </div>
                <div className="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
                  <button
                    type="submit"
                    disabled={updateMutation.isPending}
                    className="inline-flex w-full justify-center rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 sm:col-start-2 disabled:opacity-50"
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

