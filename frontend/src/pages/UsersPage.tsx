import { useState, useEffect, useRef } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { authService } from '../services/authService';
import type { UserListDto } from '../services/authService';

export default function UsersPage() {
  const queryClient = useQueryClient();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<UserListDto | null>(null);
  const [isRoleDropdownOpen, setIsRoleDropdownOpen] = useState(false);
  const [roleSearchTerm, setRoleSearchTerm] = useState('');
  const roleDropdownRef = useRef<HTMLDivElement>(null);
  const [formData, setFormData] = useState({
    email: '',
    password: 'Temp123!',
    firstName: '',
    lastName: '',
    userName: '',
    role: 'User' as string,
  });

  const token = authService.getToken() || '';

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (roleDropdownRef.current && !roleDropdownRef.current.contains(event.target as Node)) {
        setIsRoleDropdownOpen(false);
        setRoleSearchTerm('');
      }
    };

    if (isRoleDropdownOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isRoleDropdownOpen]);

  const { data: users, isLoading } = useQuery({
    queryKey: ['users'],
    queryFn: () => authService.getUsers(token),
    enabled: !!token,
  });

  const { data: roles } = useQuery({
    queryKey: ['roles'],
    queryFn: () => authService.getRoles(token),
    enabled: !!token,
  });

  const availableRoles = roles?.map(r => r.name) || ['Admin', 'Manager', 'User'];
  const filteredRoles = availableRoles.filter(role =>
    role.toLowerCase().includes(roleSearchTerm.toLowerCase())
  );

  const createUserMutation = useMutation({
    mutationFn: (data: any) => authService.createUser(data, token),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setIsCreateModalOpen(false);
      setFormData({
        email: '',
        password: 'Temp123!',
        firstName: '',
        lastName: '',
        userName: '',
        role: 'User',
      });
    },
  });

  const updateUserMutation = useMutation({
    mutationFn: (data: any) => authService.updateUser(data, token),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      setEditingUser(null);
      setFormData({
        email: '',
        password: 'Temp123!',
        firstName: '',
        lastName: '',
        userName: '',
        role: 'User',
      });
    },
  });

  const deleteUserMutation = useMutation({
    mutationFn: (userId: string) => authService.deleteUser(userId, token),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
  });

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    createUserMutation.mutate({
      email: formData.email,
      password: formData.password,
      firstName: formData.firstName,
      lastName: formData.lastName,
      userName: formData.userName || undefined,
      role: formData.role,
      mustChangePassword: true,
    });
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingUser) return;
    updateUserMutation.mutate({
      id: editingUser.id,
      firstName: formData.firstName || editingUser.firstName,
      lastName: formData.lastName || editingUser.lastName,
      email: formData.email || editingUser.email,
      role: formData.role,
    });
  };

  const handleDelete = async (userId: string, userName: string) => {
    if (window.confirm(`"${userName}" kullanıcısını silmek istediğinize emin misiniz?`)) {
      deleteUserMutation.mutate(userId);
    }
  };

  const startEdit = (user: UserListDto) => {
    setEditingUser(user);
    setFormData({
      email: user.email,
      password: 'Temp123!',
      firstName: user.firstName,
      lastName: user.lastName,
      userName: user.userName,
      role: user.roles[0] || 'User',
    });
  };

  if (isLoading) {
    return <div className="flex justify-center items-center h-64">Yükleniyor...</div>;
  }

  return (
    <div className="px-2 sm:px-4 lg:px-6">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-3xl font-semibold text-gray-900">Kullanıcı Yönetimi</h1>
          <p className="mt-2 text-sm text-gray-700">
            Sistem kullanıcılarını yönetin ve yetkilendirin.
          </p>
        </div>
        <div className="mt-4 sm:ml-16 sm:mt-0 sm:flex-none">
          <button
            onClick={() => {
              setIsCreateModalOpen(true);
              setFormData({
                email: '',
                password: 'Temp123!',
                firstName: '',
                lastName: '',
                userName: '',
                role: 'User',
              });
            }}
            className="inline-flex items-center gap-x-2 rounded-lg bg-gradient-to-r from-blue-600 to-blue-700 px-4 py-2.5 text-sm font-semibold text-white shadow-lg hover:from-blue-700 hover:to-blue-800 hover:shadow-xl transform hover:scale-105 transition-all duration-200"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            Yeni Kullanıcı
          </button>
        </div>
      </div>

      <div className="mt-8 flow-root">
        <div className="-mx-2 -my-2 overflow-x-auto sm:-mx-4 lg:-mx-6 rounded-xl shadow-lg backdrop-blur-lg border border-white/10">
          <div className="inline-block min-w-full py-2 align-middle">
            <table className="min-w-full divide-y divide-gray-300/20">
              <thead className="backdrop-blur-md">
                <tr>
                  <th className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900">Ad Soyad</th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">E-posta</th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Rol</th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Durum</th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Şifre Değiştirme</th>
                  <th className="relative py-3.5 pl-3 pr-6 sm:pr-4">
                    <span className="sr-only">İşlemler</span>
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200/20 backdrop-blur-md">
                {users?.map((user) => (
                  <tr key={user.id} className="hover:bg-gray-50 transition-colors duration-150">
                    <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900">
                      {user.firstName} {user.lastName}
                    </td>
                    <td className="px-3 py-4 text-sm text-gray-600">{user.email}</td>
                    <td className="px-3 py-4 text-sm">
                      {user.roles.length > 0 ? (
                        <span
                          className={`inline-flex items-center rounded-md px-2 py-1 text-xs font-medium ring-1 ring-inset ${
                            user.roles[0] === 'Admin' ? 'ring-red-300' : user.roles[0] === 'Manager' ? 'ring-blue-300' : 'ring-gray-300'
                          }`}
                          style={{
                            backgroundColor: user.roles[0] === 'Admin' ? '#FEE2E2' : user.roles[0] === 'Manager' ? '#DBEAFE' : '#F3F4F6',
                            color: user.roles[0] === 'Admin' ? '#991B1B' : user.roles[0] === 'Manager' ? '#1E40AF' : '#374151',
                          }}
                        >
                          {user.roles[0]}
                        </span>
                      ) : (
                        <span className="text-gray-400">-</span>
                      )}
                    </td>
                    <td className="px-3 py-4 text-sm">
                      <span
                        className={`inline-flex items-center rounded-md px-2 py-1 text-xs font-medium ${
                          user.isActive
                            ? 'bg-green-50 text-green-700 ring-1 ring-inset ring-green-600/20'
                            : 'bg-red-50 text-red-700 ring-1 ring-inset ring-red-600/20'
                        }`}
                      >
                        {user.isActive ? 'Aktif' : 'Pasif'}
                      </span>
                    </td>
                    <td className="px-3 py-4 text-sm">
                      {user.mustChangePassword ? (
                        <span className="inline-flex items-center rounded-md bg-yellow-50 px-2 py-1 text-xs font-medium text-yellow-700 ring-1 ring-inset ring-yellow-600/20">
                          Gerekli
                        </span>
                      ) : (
                        <span className="text-gray-400">-</span>
                      )}
                    </td>
                    <td className="relative whitespace-nowrap py-4 pl-3 pr-6 text-right text-sm font-medium sm:pr-4">
                      <div className="flex justify-end gap-2">
                        <button
                          onClick={() => startEdit(user)}
                          className="inline-flex items-center justify-center p-2 rounded-lg text-indigo-600 hover:bg-indigo-50 hover:text-indigo-700 transition-colors duration-200"
                          title="Düzenle"
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                          </svg>
                        </button>
                        <button
                          onClick={() => handleDelete(user.id, `${user.firstName} ${user.lastName}`)}
                          className="inline-flex items-center justify-center p-2 rounded-lg text-red-600 hover:bg-red-50 hover:text-red-700 transition-colors duration-200"
                          title="Sil"
                        >
                          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                          </svg>
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

      {/* Create/Edit User Modal */}
      {(isCreateModalOpen || editingUser) && (
        <div className="fixed inset-0 z-50 overflow-y-auto" style={{ overflow: 'auto' }}>
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <div
              className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"
              onClick={() => {
                setIsCreateModalOpen(false);
                setEditingUser(null);
                setFormData({
                  email: '',
                  password: 'Temp123!',
                  firstName: '',
                  lastName: '',
                  userName: '',
                  role: 'User',
                });
                setIsRoleDropdownOpen(false);
                setRoleSearchTerm('');
              }}
            />
            <div className="relative transform rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg sm:p-6" style={{ overflow: 'visible' }}>
              <form onSubmit={editingUser ? handleUpdate : handleCreate}>
                <div>
                  <h3 className="text-lg font-semibold leading-6 text-gray-900 mb-4">
                    {editingUser ? 'Kullanıcı Düzenle' : 'Yeni Kullanıcı Oluştur'}
                  </h3>
                  <div className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700">Ad</label>
                        <input
                          type="text"
                          required
                          value={formData.firstName}
                          onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700">Soyad</label>
                        <input
                          type="text"
                          required
                          value={formData.lastName}
                          onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                        />
                      </div>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">E-posta</label>
                      <input
                        type="email"
                        required
                        value={formData.email}
                        onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                      />
                    </div>
                    {!editingUser && (
                      <>
                        <div>
                          <label className="block text-sm font-medium text-gray-700">Kullanıcı Adı (Opsiyonel)</label>
                          <input
                            type="text"
                            value={formData.userName}
                            onChange={(e) => setFormData({ ...formData, userName: e.target.value })}
                            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                            placeholder="Boş bırakılırsa e-posta kullanılır"
                          />
                        </div>
                        <div>
                          <label className="block text-sm font-medium text-gray-700">Varsayılan Şifre</label>
                          <input
                            type="text"
                            required
                            value={formData.password}
                            onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border"
                          />
                          <p className="mt-1 text-xs text-gray-500">
                            Kullanıcı ilk girişinde bu şifreyi değiştirmek zorunda olacaktır.
                          </p>
                        </div>
                      </>
                    )}
                    <div className="relative" style={{ zIndex: isRoleDropdownOpen ? 1000 : 'auto' }}>
                      <label className="block text-sm font-medium text-gray-700 mb-2">Rol</label>
                      <div className="relative" ref={roleDropdownRef} style={{ zIndex: isRoleDropdownOpen ? 1000 : 'auto' }}>
                        <button
                          type="button"
                          onClick={(e) => {
                            e.preventDefault();
                            e.stopPropagation();
                            setIsRoleDropdownOpen(!isRoleDropdownOpen);
                            setRoleSearchTerm('');
                          }}
                          className="w-full flex items-center justify-between rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-gray-700 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                        >
                          <span>{formData.role || 'Rol seçin'}</span>
                          <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                          </svg>
                        </button>
                        {isRoleDropdownOpen && (
                          <div 
                            className="absolute z-[1001] mt-1 w-full rounded-md bg-white shadow-lg ring-1 ring-black ring-opacity-5"
                            style={{ 
                              position: 'absolute',
                              top: '100%',
                              left: 0,
                              right: 0,
                            }}
                            onWheel={(e) => {
                              e.stopPropagation();
                            }}
                            onTouchMove={(e) => {
                              e.stopPropagation();
                            }}
                          >
                            <div className="p-2 border-b border-gray-200 bg-white sticky top-0">
                              <input
                                type="text"
                                placeholder="Rol ara..."
                                value={roleSearchTerm}
                                onChange={(e) => setRoleSearchTerm(e.target.value)}
                                className="w-full rounded-md border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:ring-indigo-500 border"
                                onClick={(e) => e.stopPropagation()}
                                onKeyDown={(e) => e.stopPropagation()}
                                autoFocus
                              />
                            </div>
                            <div 
                              style={{ 
                                maxHeight: '200px',
                                overflowY: 'auto',
                                overflowX: 'hidden',
                                WebkitOverflowScrolling: 'touch',
                              }}
                              onWheel={(e) => {
                                e.stopPropagation();
                                const target = e.currentTarget;
                                const { scrollTop, scrollHeight, clientHeight } = target;
                                const isAtTop = scrollTop === 0;
                                const isAtBottom = scrollTop + clientHeight >= scrollHeight - 1;
                                
                                if ((isAtTop && e.deltaY < 0) || (isAtBottom && e.deltaY > 0)) {
                                  e.preventDefault();
                                }
                              }}
                              onTouchMove={(e) => {
                                e.stopPropagation();
                              }}
                            >
                              {filteredRoles.length > 0 ? (
                                <div className="py-1">
                                  {filteredRoles.map((role) => (
                                    <button
                                      key={role}
                                      type="button"
                                      onClick={(e) => {
                                        e.preventDefault();
                                        e.stopPropagation();
                                        setFormData({ ...formData, role });
                                        setIsRoleDropdownOpen(false);
                                        setRoleSearchTerm('');
                                      }}
                                      className={`w-full text-left px-4 py-2.5 text-sm hover:bg-indigo-50 transition-colors ${
                                        formData.role === role ? 'bg-indigo-100 text-indigo-700 font-medium' : 'text-gray-700'
                                      }`}
                                    >
                                      <div className="flex items-center">
                                        <svg className={`w-4 h-4 mr-2 flex-shrink-0 ${formData.role === role ? 'text-indigo-600' : 'text-gray-400'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                          {formData.role === role ? (
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                                          ) : (
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                          )}
                                        </svg>
                                        <span>{role}</span>
                                      </div>
                                    </button>
                                  ))}
                                </div>
                              ) : (
                                <div className="px-4 py-3 text-sm text-gray-500 text-center">Rol bulunamadı</div>
                              )}
                            </div>
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
                <div className="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
                  <button
                    type="submit"
                    disabled={createUserMutation.isPending || updateUserMutation.isPending}
                    className="inline-flex w-full justify-center rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 sm:col-start-2 disabled:opacity-50"
                  >
                    {editingUser
                      ? (updateUserMutation.isPending ? 'Güncelleniyor...' : 'Güncelle')
                      : (createUserMutation.isPending ? 'Oluşturuluyor...' : 'Oluştur')}
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setIsCreateModalOpen(false);
                      setEditingUser(null);
                      setFormData({
                        email: '',
                        password: 'Temp123!',
                        firstName: '',
                        lastName: '',
                        userName: '',
                        role: 'User',
                      });
                      setIsRoleDropdownOpen(false);
                      setRoleSearchTerm('');
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
  );
}
