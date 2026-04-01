import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getApiBaseUrl } from '../utils/apiConfig'
import { authService } from '../services/authService'

interface SupportRequest {
  id: number
  email: string
  subject: string
  detectedIntent: string | null
  status: 'Pending' | 'Replied'
  createdAt: string
  updatedAt: string | null
}

interface SupportRequestsResult {
  items: SupportRequest[]
  totalCount: number
}

const getAuthHeaders = (): Record<string, string> => {
  const token = authService.getToken()
  const h: Record<string, string> = {}
  if (token) h.Authorization = `Bearer ${token}`
  return h
}

const fetchSupportRequests = async (status: string, page: number): Promise<SupportRequestsResult> => {
  const API_BASE_URL = getApiBaseUrl()
  const params = new URLSearchParams({ page: String(page), pageSize: '20' })
  if (status !== 'all') params.append('status', status)
  const res = await fetch(`${API_BASE_URL}/api/support-requests?${params}`, {
    headers: { ...getAuthHeaders() },
  })
  if (!res.ok) throw new Error('Destek talepleri yüklenemedi')
  return res.json()
}

const updateStatus = async ({ id, status }: { id: number; status: string }) => {
  const API_BASE_URL = getApiBaseUrl()
  const res = await fetch(`${API_BASE_URL}/api/support-requests/${id}/status`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json', ...getAuthHeaders() },
    body: JSON.stringify({ status }),
  })
  if (!res.ok) throw new Error('Durum güncellenemedi')
}

const formatDate = (iso: string) =>
  new Intl.DateTimeFormat('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }).format(new Date(iso))

export default function SupportRequestsPage() {
  const [statusFilter, setStatusFilter] = useState<'all' | 'Pending' | 'Replied'>('all')
  const [page, setPage] = useState(1)
  const queryClient = useQueryClient()

  const { data, isLoading, isError } = useQuery({
    queryKey: ['support-requests', statusFilter, page],
    queryFn: () => fetchSupportRequests(statusFilter, page),
  })

  const mutation = useMutation({
    mutationFn: updateStatus,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['support-requests'] }),
  })

  const totalPages = data ? Math.ceil(data.totalCount / 20) : 1
  const pendingCount = data?.items.filter((r) => r.status === 'Pending').length ?? 0

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">Destek Talepleri</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">Ziyaretçilerden gelen yardım talepleri</p>
        </div>
        {pendingCount > 0 && (
          <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-semibold bg-orange-100 text-orange-800 dark:bg-orange-950/50 dark:text-orange-200">
            {pendingCount} bekleyen talep
          </span>
        )}
      </div>

      {/* Filter tabs */}
      <div className="flex gap-2 border-b border-gray-200 dark:border-gray-600">
        {([['all', 'Tümü'], ['Pending', 'Bekliyor'], ['Replied', 'Yanıtlandı']] as const).map(([val, label]) => (
          <button
            key={val}
            onClick={() => { setStatusFilter(val); setPage(1) }}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
              statusFilter === val
                ? 'border-indigo-600 text-indigo-600 dark:border-indigo-400 dark:text-indigo-400'
                : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
            }`}
          >
            {label}
          </button>
        ))}
      </div>

      {/* Table */}
      <div className="rounded-xl shadow-lg backdrop-blur-lg border border-white/10 dark:border-gray-700/60 bg-white/40 dark:bg-gray-800/60 overflow-hidden">
        {isLoading ? (
          <div className="flex items-center justify-center h-48 text-gray-500 dark:text-gray-400 text-sm">Yükleniyor...</div>
        ) : isError ? (
          <div className="flex items-center justify-center h-48 text-red-500 dark:text-red-400 text-sm">Veriler yüklenemedi.</div>
        ) : data?.items.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-48 text-gray-400 dark:text-gray-500">
            <svg className="w-12 h-12 mb-3 opacity-40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
            </svg>
            <p className="text-sm">Henüz destek talebi bulunmuyor.</p>
          </div>
        ) : (
          <table className="min-w-full divide-y divide-gray-100 dark:divide-gray-600/50">
            <thead className="bg-white/60 dark:bg-gray-800/90">
              <tr>
                <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">E-posta</th>
                <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Konu</th>
                <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider hidden md:table-cell">Tespit Edilen Konu</th>
                <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider hidden lg:table-cell">Tarih</th>
                <th className="px-5 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Durum</th>
                <th className="px-5 py-3 text-right text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider">İşlem</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-600/40">
              {(data as SupportRequestsResult).items.map((req) => (
                <tr key={req.id} className="hover:bg-gray-50 dark:hover:bg-gray-700/40 transition-colors">
                  <td className="px-5 py-4 text-sm font-medium text-gray-900 dark:text-gray-100">{req.email}</td>
                  <td className="px-5 py-4 text-sm text-gray-700 dark:text-gray-300 max-w-xs truncate">{req.subject}</td>
                  <td className="px-5 py-4 text-sm text-gray-500 dark:text-gray-400 hidden md:table-cell">
                    {req.detectedIntent ? (
                      <span className="px-2 py-0.5 rounded-full text-xs bg-indigo-50 text-indigo-700 dark:bg-indigo-950/50 dark:text-indigo-300 font-medium">
                        {req.detectedIntent}
                      </span>
                    ) : (
                      <span className="text-gray-300 dark:text-gray-600">—</span>
                    )}
                  </td>
                  <td className="px-5 py-4 text-sm text-gray-500 dark:text-gray-400 hidden lg:table-cell">{formatDate(req.createdAt)}</td>
                  <td className="px-5 py-4">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold ${
                      req.status === 'Pending'
                        ? 'bg-orange-100 text-orange-700 dark:bg-orange-950/50 dark:text-orange-200'
                        : 'bg-green-100 text-green-700 dark:bg-green-950/50 dark:text-green-200'
                    }`}>
                      {req.status === 'Pending' ? 'Bekliyor' : 'Yanıtlandı'}
                    </span>
                  </td>
                  <td className="px-5 py-4 text-right">
                    <button
                      onClick={() =>
                        mutation.mutate({
                          id: req.id,
                          status: req.status === 'Pending' ? 'Replied' : 'Pending',
                        })
                      }
                      disabled={mutation.isPending}
                      className={`text-xs font-semibold px-3 py-1.5 rounded-lg transition-colors disabled:opacity-50 ${
                        req.status === 'Pending'
                          ? 'bg-green-50 text-green-700 hover:bg-green-100 border border-green-200 dark:bg-green-950/40 dark:text-green-300 dark:border-green-500/30 dark:hover:bg-green-950/60'
                          : 'bg-gray-50 text-gray-600 hover:bg-gray-100 border border-gray-200 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-600 dark:hover:bg-gray-700/80'
                      }`}
                    >
                      {req.status === 'Pending' ? 'Yanıtlandı İşaretle' : 'Bekleyene Al'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between text-sm text-gray-500 dark:text-gray-400">
          <span>Toplam {data?.totalCount ?? 0} talep</span>
          <div className="flex gap-2">
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
              className="px-3 py-1.5 rounded-lg border border-gray-200 dark:border-gray-600 bg-white/70 dark:bg-gray-800/80 hover:bg-gray-50 dark:hover:bg-gray-700 disabled:opacity-40 transition-colors text-gray-800 dark:text-gray-200"
            >
              ← Önceki
            </button>
            <span className="px-3 py-1.5 font-medium text-gray-800 dark:text-gray-200">{page} / {totalPages}</span>
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
              className="px-3 py-1.5 rounded-lg border border-gray-200 dark:border-gray-600 bg-white/70 dark:bg-gray-800/80 hover:bg-gray-50 dark:hover:bg-gray-700 disabled:opacity-40 transition-colors text-gray-800 dark:text-gray-200"
            >
              Sonraki →
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
