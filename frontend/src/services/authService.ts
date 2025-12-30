export interface LoginRequest {
  email: string;
  password: string;
}


export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserDto;
}

export interface UserDto {
  id: string;
  email: string;
  userName: string;
  firstName: string;
  lastName: string;
  roles: string[];
  mustChangePassword?: boolean;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  userName?: string;
  role?: string; // Single role only
  mustChangePassword?: boolean;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ForceChangePasswordRequest {
  newPassword: string;
  confirmPassword: string;
}

export interface RoleDto {
  id: string;
  name: string;
  claims: ClaimDto[];
}

export interface ClaimDto {
  type: string;
  value: string;
}

export interface CreateRoleRequest {
  name: string;
  claims: ClaimDto[];
}

export interface UpdateRoleRequest {
  id: string;
  name?: string;
  claims?: ClaimDto[];
}

export interface UpdateUserRequest {
  id: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  isActive?: boolean;
  role?: string;
}

export interface UserListDto {
  id: string;
  email: string;
  userName: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  mustChangePassword: boolean;
  createdAt: string;
  roles: string[];
  claims: string[];
}

export interface RoleDto {
  id: string;
  name: string;
  claims: ClaimDto[];
}

export interface ClaimDto {
  type: string;
  value: string;
}

export interface CreateRoleRequest {
  name: string;
  claims: ClaimDto[];
}

export interface UpdateRoleRequest {
  id: string;
  name?: string;
  claims?: ClaimDto[];
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

class AuthService {
  private getApiBaseUrl = () => {
    if (import.meta.env.VITE_API_BASE_URL) {
      return import.meta.env.VITE_API_BASE_URL;
    }
    if (import.meta.env.PROD) {
      return `http://${window.location.hostname}:5134`;
    }
    return 'http://localhost:5134';
  };

  async login(request: LoginRequest): Promise<AuthResponse> {
    const response = await fetch(`${this.getApiBaseUrl()}/api/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Login failed' }));
      throw new Error(error.message || 'Login failed');
    }

    return response.json();
  }

  async createUser(request: CreateUserRequest, token: string): Promise<UserDto> {
    const response = await fetch(`${this.getApiBaseUrl()}/api/auth/users`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'User creation failed' }));
      throw new Error(error.message || 'User creation failed');
    }

    return response.json();
  }

  async changePassword(request: ChangePasswordRequest, token: string): Promise<void> {
    const response = await fetch(`${this.getApiBaseUrl()}/api/auth/change-password`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Password change failed' }));
      throw new Error(error.message || 'Password change failed');
    }
  }

  async forceChangePassword(request: ForceChangePasswordRequest, token: string): Promise<void> {
    const response = await fetch(`${this.getApiBaseUrl()}/api/auth/force-change-password`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Password change failed' }));
      throw new Error(error.message || 'Password change failed');
    }
  }

  async refreshToken(refreshToken: string): Promise<AuthResponse> {
    const response = await fetch(`${this.getApiBaseUrl()}/api/auth/refresh-token`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ refreshToken }),
    });

    if (!response.ok) {
      throw new Error('Token refresh failed');
    }

    return response.json();
  }

  async getCurrentUser(): Promise<UserDto> {
    const token = this.getToken();
    if (!token) {
      throw new Error('No token available');
    }

    const response = await fetch(`${this.getApiBaseUrl()}/api/auth/me`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Failed to get current user');
    }

    return response.json();
  }

  async getUsers(token: string): Promise<UserListDto[]> {
    const response = await fetch(`${this.getApiBaseUrl()}/api/auth/users`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Failed to get users' }));
      throw new Error(error.message || 'Failed to get users');
    }

    return response.json();
  }

  // Token management
  getToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  setTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  }

  clearTokens(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }

  getUser(): UserDto | null {
    const userStr = localStorage.getItem('user');
    if (!userStr) return null;
    try {
      return JSON.parse(userStr);
    } catch {
      return null;
    }
  }

  setUser(user: UserDto): void {
    localStorage.setItem('user', JSON.stringify(user));
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  hasRole(role: string): boolean {
    const user = this.getUser();
    return user?.roles.includes(role) ?? false;
  }

  hasAnyRole(roles: string[]): boolean {
    const user = this.getUser();
    if (!user) return false;
    return roles.some(role => user.roles.includes(role));
  }

  async getRoles(token: string): Promise<RoleDto[]> {
    const response = await fetch(`${this.getApiBaseUrl()}/api/auth/roles`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Failed to get roles' }));
      throw new Error(error.message || 'Failed to get roles');
    }

    return response.json();
  }

  async createRole(request: CreateRoleRequest, token: string): Promise<RoleDto> {
    const response = await fetch(`${this.getApiBaseUrl()}/api/auth/roles`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Failed to create role' }));
      throw new Error(error.message || 'Failed to create role');
    }

    return response.json();
  }

  async updateRole(request: UpdateRoleRequest, token: string): Promise<RoleDto> {
    const response = await fetch(`${this.getApiBaseUrl()}/api/auth/roles`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Failed to update role' }));
      throw new Error(error.message || 'Failed to update role');
    }

    return response.json();
  }

  async deleteRole(roleId: string, token: string): Promise<void> {
    const response = await fetch(`${this.getApiBaseUrl()}/api/auth/roles/${roleId}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Failed to delete role' }));
      throw new Error(error.message || 'Failed to delete role');
    }
  }

  async updateUser(request: UpdateUserRequest, token: string): Promise<UserListDto> {
    const response = await fetch(`${this.getApiBaseUrl()}/api/auth/users`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Failed to update user' }));
      throw new Error(error.message || 'Failed to update user');
    }

    return response.json();
  }

  async deleteUser(userId: string, token: string): Promise<void> {
    const response = await fetch(`${this.getApiBaseUrl()}/api/auth/users/${userId}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Failed to delete user' }));
      throw new Error(error.message || 'Failed to delete user');
    }
  }
}

export const authService = new AuthService();

