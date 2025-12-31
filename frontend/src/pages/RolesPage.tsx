import { useState, useMemo, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { authService } from '../services/authService';
import type { RoleDto, ClaimDto } from '../services/authService';
import { signalRService } from '../services/signalRService';

// Yetki kategorileri ve yetkiler
interface Permission {
  value: string;
  label: string;
  description?: string;
}

interface PermissionCategory {
  id: string;
  name: string;
  permissions: Permission[];
}

const PERMISSION_CATEGORIES: PermissionCategory[] = [
  {
    id: 'user-management',
    name: 'Kullanıcı Yönetimi',
    permissions: [
      {
        value: 'CanCreateUser',
        label: 'Kullanıcı Oluşturma',
        description: 'Yeni kullanıcı oluşturma yetkisi',
      },
      {
        value: 'CanManageUsers',
        label: 'Kullanıcı Yönetimi',
        description: 'Kullanıcıları düzenleme ve silme yetkisi',
      },
    ],
  },
  {
    id: 'role-management',
    name: 'Rol Yönetimi',
    permissions: [
      {
        value: 'CanViewRoles',
        label: 'Rolleri Görüntüleme',
        description: 'Rol listesini görüntüleme yetkisi',
      },
      {
        value: 'CanManageRoles',
        label: 'Rol Yönetimi',
        description: 'Rol oluşturma, düzenleme ve silme yetkisi',
      },
    ],
  },
  {
    id: 'category-management',
    name: 'Kategori Yönetimi',
    permissions: [
      {
        value: 'CanViewCategories',
        label: 'Kategorileri Görüntüleme',
        description: 'Kategori listesini görüntüleme yetkisi',
      },
      {
        value: 'CanManageCategories',
        label: 'Kategori Yönetimi',
        description: 'Kategori oluşturma, düzenleme ve silme yetkisi',
      },
    ],
  },
  {
    id: 'product-management',
    name: 'Ürün Yönetimi',
    permissions: [
      {
        value: 'CanViewProducts',
        label: 'Ürünleri Görüntüleme',
        description: 'Ürün listesini görüntüleme ve Excel export yetkisi',
      },
      {
        value: 'CanManageProducts',
        label: 'Ürün Yönetimi',
        description: 'Ürün oluşturma, düzenleme ve silme yetkisi',
      },
    ],
  },
  {
    id: 'product-attribute-management',
    name: 'Ürün Özellikleri Yönetimi',
    permissions: [
      {
        value: 'CanViewProductAttributes',
        label: 'Ürün Özelliklerini Görüntüleme',
        description: 'Ürün özellikleri listesini görüntüleme ve Excel export yetkisi',
      },
      {
        value: 'CanManageProductAttributes',
        label: 'Ürün Özellikleri Yönetimi',
        description: 'Ürün özellikleri oluşturma, düzenleme ve silme yetkisi',
      },
    ],
  },
  {
    id: 'stock-movement-management',
    name: 'Stok Hareketleri Yönetimi',
    permissions: [
      {
        value: 'CanViewStockMovements',
        label: 'Stok Hareketlerini Görüntüleme',
        description: 'Stok hareketleri listesini görüntüleme ve Excel export yetkisi',
      },
      {
        value: 'CanManageStockMovements',
        label: 'Stok Hareketleri Yönetimi',
        description: 'Stok hareketi oluşturma yetkisi',
      },
    ],
  },
  {
    id: 'todo-management',
    name: 'Yapılacaklar Yönetimi',
    permissions: [
      {
        value: 'CanViewTodos',
        label: 'Yapılacakları Görüntüleme',
        description: 'Yapılacaklar listesini görüntüleme yetkisi',
      },
      {
        value: 'CanManageTodos',
        label: 'Yapılacaklar Yönetimi',
        description: 'Yapılacak oluşturma, düzenleme ve silme yetkisi',
      },
    ],
  },
  {
    id: 'location-management',
    name: 'Lokasyon Yönetimi',
    permissions: [
      {
        value: 'CanViewLocations',
        label: 'Lokasyonları Görüntüleme',
        description: 'Lokasyon listesini görüntüleme yetkisi',
      },
      {
        value: 'CanManageLocations',
        label: 'Lokasyon Yönetimi',
        description: 'Lokasyon oluşturma, düzenleme ve silme yetkisi',
      },
    ],
  },
  {
    id: 'dashboard',
    name: 'Dashboard',
    permissions: [
      {
        value: 'CanViewDashboard',
        label: 'Dashboard Görüntüleme',
        description: 'Dashboard istatistiklerini görüntüleme yetkisi',
      },
    ],
  },
  {
    id: 'reports',
    name: 'Raporlar',
    permissions: [
      {
        value: 'CanViewReports',
        label: 'Raporları Görüntüleme',
        description: 'Rapor oluşturma ve görüntüleme yetkisi',
      },
    ],
  },
  {
    id: 'chat',
    name: 'Chat',
    permissions: [
      {
        value: 'CanUseChat',
        label: 'Chat Kullanımı',
        description: 'AI chat özelliğini kullanma yetkisi',
      },
    ],
  },
];

export default function RolesPage() {
  const queryClient = useQueryClient();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [editingRole, setEditingRole] = useState<RoleDto | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    selectedPermissions: [] as string[], // Permission value'ları
  });
  const [expandedRoles, setExpandedRoles] = useState<Set<string>>(new Set());

  const token = authService.getToken() || '';
  const currentUser = authService.getUser();

  // SignalR setup
  useEffect(() => {
    signalRService.startConnection().catch((error) => {
      console.error('SignalR bağlantı hatası:', error);
    });

    const handleRoleCreated = () => {
      queryClient.invalidateQueries({ queryKey: ['roles'] });
    };

    const handleRoleUpdated = () => {
      queryClient.invalidateQueries({ queryKey: ['roles'] });
    };

    const handleRoleDeleted = () => {
      queryClient.invalidateQueries({ queryKey: ['roles'] });
    };

    signalRService.onRoleCreated(handleRoleCreated);
    signalRService.onRoleUpdated(handleRoleUpdated);
    signalRService.onRoleDeleted(handleRoleDeleted);

    return () => {
      signalRService.offRoleCreated(handleRoleCreated);
      signalRService.offRoleUpdated(handleRoleUpdated);
      signalRService.offRoleDeleted(handleRoleDeleted);
    };
  }, [queryClient]);

  // Check permissions
  const canView = currentUser?.roles ? (currentUser.roles.includes('Admin') || currentUser.roles.includes('Manager')) : false;
  const canManage = currentUser?.roles ? currentUser.roles.includes('Admin') : false;

  const { data: roles, isLoading } = useQuery({
    queryKey: ['roles'],
    queryFn: () => authService.getRoles(token),
    enabled: !!token && canView,
  });

  const createRoleMutation = useMutation({
    mutationFn: (data: any) => authService.createRole(data, token),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roles'] });
      setIsCreateModalOpen(false);
      setFormData({ name: '', selectedPermissions: [] });
    },
  });

  const updateRoleMutation = useMutation({
    mutationFn: (data: any) => authService.updateRole(data, token),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roles'] });
      setEditingRole(null);
      setFormData({ name: '', selectedPermissions: [] });
    },
  });

  const deleteRoleMutation = useMutation({
    mutationFn: (roleId: string) => authService.deleteRole(roleId, token),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['roles'] });
    },
  });

  // Seçili yetkileri ClaimDto formatına dönüştür
  const selectedClaims = useMemo(() => {
    return formData.selectedPermissions.map(permission => ({
      type: 'Permission',
      value: permission,
    } as ClaimDto));
  }, [formData.selectedPermissions]);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    createRoleMutation.mutate({
      name: formData.name,
      claims: selectedClaims,
    });
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingRole) return;
    updateRoleMutation.mutate({
      id: editingRole.id,
      name: formData.name || editingRole.name,
      claims: selectedClaims,
    });
  };

  const handleDelete = async (roleId: string, roleName: string) => {
    if (roleName === 'User' || roleName === 'Admin') {
      alert('User ve Admin rolleri silinemez!');
      return;
    }
    if (window.confirm(`"${roleName}" rolünü silmek istediğinize emin misiniz? Bu role sahip kullanıcılar "User" rolüne atanacaktır.`)) {
      deleteRoleMutation.mutate(roleId);
    }
  };

  const isAdminRole = (roleName: string) => roleName === 'Admin';
  const isUserRole = (roleName: string) => roleName === 'User';
  const isProtectedRole = (roleName: string) => isAdminRole(roleName) || isUserRole(roleName);

  const togglePermission = (permissionValue: string) => {
    setFormData(prev => {
      const isSelected = prev.selectedPermissions.includes(permissionValue);
      return {
        ...prev,
        selectedPermissions: isSelected
          ? prev.selectedPermissions.filter(p => p !== permissionValue)
          : [...prev.selectedPermissions, permissionValue],
      };
    });
  };

  const startEdit = (role: RoleDto) => {
    // Admin rolü düzenlenemez (sadece görüntüleme için)
    if (isAdminRole(role.name)) {
      return;
    }
    
    setEditingRole(role);
    // Mevcut claim'leri permission value'lara dönüştür
    const permissionValues = role.claims
      .filter(claim => claim.type === 'Permission')
      .map(claim => claim.value);
    
    setFormData({
      name: role.name,
      selectedPermissions: permissionValues,
    });
  };

  if (!canView) {
    return (
      <div className="flex justify-center items-center h-64">
        <p className="text-red-600">Bu sayfayı görüntüleme yetkiniz yok.</p>
      </div>
    );
  }

  if (isLoading) {
    return <div className="flex justify-center items-center h-64">Yükleniyor...</div>;
  }

  return (
    <div className="px-2 sm:px-4 lg:px-6">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-3xl font-semibold text-gray-900">Rol Yönetimi</h1>
          <p className="mt-2 text-sm text-gray-700">
            Sistem rollerini yönetin ve yetkilendirin.
          </p>
        </div>
        {canManage && (
          <div className="mt-4 sm:ml-16 sm:mt-0 sm:flex-none">
            <button
              onClick={() => {
                setIsCreateModalOpen(true);
                setFormData({ name: '', selectedPermissions: [] });
              }}
              className="inline-flex items-center gap-x-2 rounded-lg bg-gradient-to-r from-blue-600 to-blue-700 px-4 py-2.5 text-sm font-semibold text-white shadow-lg hover:from-blue-700 hover:to-blue-800 hover:shadow-xl transform hover:scale-105 transition-all duration-200"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              Yeni Rol
            </button>
          </div>
        )}
      </div>

      <div className="mt-8 flow-root">
        <div className="-mx-2 -my-2 overflow-x-auto sm:-mx-4 lg:-mx-6 rounded-xl shadow-lg backdrop-blur-lg border border-white/10">
          <div className="inline-block min-w-full py-2 align-middle">
            <table className="min-w-full divide-y divide-gray-300/20">
              <thead className="backdrop-blur-md">
                <tr>
                  <th className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900">Rol Adı</th>
                  <th className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Yetkiler (Claims)</th>
                  <th className="relative py-3.5 pl-3 pr-6 sm:pr-4">
                    <span className="sr-only">İşlemler</span>
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200/20 backdrop-blur-md">
                {roles?.map((role) => (
                  <tr key={role.id} className="hover:bg-gray-50 transition-colors duration-150">
                    <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900">
                      <span className={`inline-flex items-center rounded-md px-2 py-1 text-xs font-medium ${
                        role.name === 'Admin' ? 'bg-red-100 text-red-800' :
                        role.name === 'User' ? 'bg-gray-100 text-gray-800' :
                        'bg-blue-100 text-blue-800'
                      }`}>
                        {role.name}
                        {(role.name === 'Admin' || role.name === 'User') && (
                          <span className="ml-2 text-xs text-gray-500">(Korumalı)</span>
                        )}
                      </span>
                    </td>
                    <td className="px-3 py-4 text-sm">
                      <div className="flex flex-wrap gap-1">
                        {role.claims.length > 0 ? (
                          <>
                            {(expandedRoles.has(role.id) ? role.claims : role.claims.slice(0, 10)).map((claim, idx) => (
                              <span
                                key={idx}
                                className="inline-flex items-center rounded-md px-2 py-1 text-xs font-medium bg-purple-50 text-purple-700 ring-1 ring-inset ring-purple-600/20"
                              >
                                {claim.type}: {claim.value}
                              </span>
                            ))}
                            {role.claims.length > 10 && (
                              <button
                                onClick={() => {
                                  const newExpanded = new Set(expandedRoles);
                                  if (newExpanded.has(role.id)) {
                                    newExpanded.delete(role.id);
                                  } else {
                                    newExpanded.add(role.id);
                                  }
                                  setExpandedRoles(newExpanded);
                                }}
                                className="inline-flex items-center rounded-md px-2 py-1 text-xs font-medium bg-indigo-50 text-indigo-700 ring-1 ring-inset ring-indigo-600/20 hover:bg-indigo-100 transition-colors cursor-pointer"
                              >
                                {expandedRoles.has(role.id) 
                                  ? `Daha az göster (${role.claims.length - 10} gizle)`
                                  : `+${role.claims.length - 10} daha fazla göster`}
                              </button>
                            )}
                          </>
                        ) : (
                          <span className="text-gray-400">-</span>
                        )}
                      </div>
                    </td>
                    <td className="relative whitespace-nowrap py-4 pl-3 pr-6 text-right text-sm font-medium sm:pr-4">
                      {canManage && (
                        <div className="flex justify-end gap-2">
                          {!isAdminRole(role.name) && (
                            <button
                              onClick={() => startEdit(role)}
                              className="text-indigo-600 hover:text-indigo-900"
                            >
                              Düzenle
                            </button>
                          )}
                          {!isProtectedRole(role.name) && (
                            <button
                              onClick={() => handleDelete(role.id, role.name)}
                              className="text-red-600 hover:text-red-900"
                            >
                              Sil
                            </button>
                          )}
                        </div>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Create/Edit Modal */}
      {(isCreateModalOpen || editingRole) && (
        <div className="fixed inset-0 z-10 overflow-y-auto">
          <div className="flex min-h-full items-end justify-center p-4 text-center sm:items-center sm:p-0">
            <div
              className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"
              onClick={() => {
                setIsCreateModalOpen(false);
                setEditingRole(null);
                setFormData({ name: '', selectedPermissions: [] });
              }}
            />
            <div className="relative transform overflow-hidden rounded-lg bg-white px-4 pb-4 pt-5 text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-3xl sm:p-6">
              <form onSubmit={editingRole ? handleUpdate : handleCreate}>
                <div>
                  <h3 className="text-lg font-semibold leading-6 text-gray-900 mb-4">
                    {editingRole ? 'Rol Düzenle' : 'Yeni Rol Oluştur'}
                  </h3>
                  <div className="space-y-6">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2">Rol Adı</label>
                      <input
                        type="text"
                        required
                        value={formData.name}
                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                        disabled={isAdminRole(formData.name) || isUserRole(formData.name)}
                        className={`block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm px-3 py-2 border ${
                          (isAdminRole(formData.name) || isUserRole(formData.name)) 
                            ? 'bg-gray-100 cursor-not-allowed' 
                            : ''
                        }`}
                        placeholder="Örn: Editor, Viewer, Moderator"
                      />
                      {(isAdminRole(formData.name) || isUserRole(formData.name)) && (
                        <p className="mt-1 text-xs text-gray-500">
                          {isAdminRole(formData.name) 
                            ? 'Admin rolünün adı değiştirilemez.' 
                            : 'User rolünün adı değiştirilemez.'}
                        </p>
                      )}
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-3">Yetkiler</label>
                      {isAdminRole(formData.name) ? (
                        <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg">
                          <p className="text-sm text-blue-800 mb-3">
                            <strong>Admin rolü tüm yetkilere otomatik sahiptir.</strong> Yeni yetkiler eklendiğinde otomatik olarak Admin rolüne atanır.
                          </p>
                          <div className="space-y-4">
                            {PERMISSION_CATEGORIES.map((category) => (
                              <div key={category.id} className="border border-blue-200 rounded-lg overflow-hidden bg-white">
                                <div className="bg-blue-50 px-4 py-3 border-b border-blue-200">
                                  <h4 className="text-sm font-semibold text-gray-900">{category.name}</h4>
                                </div>
                                <div className="bg-white">
                                  <table className="min-w-full divide-y divide-gray-200">
                                    <thead className="bg-gray-50">
                                      <tr>
                                        <th scope="col" className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                          Yetki
                                        </th>
                                        <th scope="col" className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                          Açıklama
                                        </th>
                                        <th scope="col" className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-20">
                                          Durum
                                        </th>
                                      </tr>
                                    </thead>
                                    <tbody className="bg-white divide-y divide-gray-200">
                                      {category.permissions.map((permission) => (
                                          <tr
                                            key={permission.value}
                                            className="bg-green-50"
                                          >
                                            <td className="px-4 py-4 whitespace-nowrap">
                                              <div className="text-sm font-medium text-gray-900">{permission.label}</div>
                                              <div className="text-xs text-gray-500 mt-1">{permission.value}</div>
                                            </td>
                                            <td className="px-4 py-4">
                                              <div className="text-sm text-gray-500">{permission.description || '-'}</div>
                                            </td>
                                            <td className="px-4 py-4 whitespace-nowrap">
                                              <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                                                <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
                                                  <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                                                </svg>
                                                Aktif
                                              </span>
                                            </td>
                                          </tr>
                                        ))}
                                    </tbody>
                                  </table>
                                </div>
                              </div>
                            ))}
                          </div>
                        </div>
                      ) : (
                        <>
                          <div className="space-y-4">
                            {PERMISSION_CATEGORIES.map((category) => (
                              <div key={category.id} className="border border-gray-200 rounded-lg overflow-hidden">
                                <div className="bg-gray-50 px-4 py-3 border-b border-gray-200">
                                  <h4 className="text-sm font-semibold text-gray-900">{category.name}</h4>
                                </div>
                                <div className="bg-white">
                                  <table className="min-w-full divide-y divide-gray-200">
                                    <thead className="bg-gray-50">
                                      <tr>
                                        <th scope="col" className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-12">
                                          <span className="sr-only">Seç</span>
                                        </th>
                                        <th scope="col" className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                          Yetki
                                        </th>
                                        <th scope="col" className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                          Açıklama
                                        </th>
                                      </tr>
                                    </thead>
                                    <tbody className="bg-white divide-y divide-gray-200">
                                      {category.permissions.map((permission) => {
                                        const isSelected = formData.selectedPermissions.includes(permission.value);
                                        return (
                                          <tr
                                            key={permission.value}
                                            className={`hover:bg-gray-50 transition-colors cursor-pointer ${
                                              isSelected ? 'bg-indigo-50' : ''
                                            }`}
                                            onClick={() => togglePermission(permission.value)}
                                          >
                                            <td className="px-4 py-4 whitespace-nowrap">
                                              <input
                                                type="checkbox"
                                                checked={isSelected}
                                                onChange={() => togglePermission(permission.value)}
                                                onClick={(e) => e.stopPropagation()}
                                                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                                              />
                                            </td>
                                            <td className="px-4 py-4 whitespace-nowrap">
                                              <div className="text-sm font-medium text-gray-900">{permission.label}</div>
                                              <div className="text-xs text-gray-500 mt-1">{permission.value}</div>
                                            </td>
                                            <td className="px-4 py-4">
                                              <div className="text-sm text-gray-500">{permission.description || '-'}</div>
                                            </td>
                                          </tr>
                                        );
                                      })}
                                    </tbody>
                                  </table>
                                </div>
                              </div>
                            ))}
                          </div>
                          {formData.selectedPermissions.length > 0 && (
                            <div className="mt-4 p-3 bg-indigo-50 rounded-md">
                              <p className="text-xs text-indigo-700">
                                <span className="font-medium">{formData.selectedPermissions.length}</span> yetki seçildi
                              </p>
                            </div>
                          )}
                        </>
                      )}
                    </div>
                  </div>
                </div>
                <div className="mt-5 sm:mt-6 sm:grid sm:grid-flow-row-dense sm:grid-cols-2 sm:gap-3">
                  <button
                    type="submit"
                    disabled={createRoleMutation.isPending || updateRoleMutation.isPending}
                    className="inline-flex w-full justify-center rounded-md bg-indigo-600 px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 sm:col-start-2 disabled:opacity-50"
                  >
                    {editingRole ? 'Güncelle' : 'Oluştur'}
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setIsCreateModalOpen(false);
                      setEditingRole(null);
                      setFormData({ name: '', selectedPermissions: [] });
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

