import api from './api';

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    isActive: boolean;
    roles: string[];
  };
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

class AuthService {
  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/api/auth/register', data);
    return response.data;
  }

  async login(data: LoginRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/api/auth/login', data);
    return response.data;
  }

  async changePassword(data: ChangePasswordRequest): Promise<boolean> {
    const response = await api.post<boolean>('/api/auth/change-password', data);
    return response.data;
  }

  // Store auth data in localStorage
  setAuthData(token: string, user: AuthResponse['user']) {
    localStorage.setItem('authToken', token);
    localStorage.setItem('authUser', JSON.stringify(user));
  }

  // Clear auth data from localStorage
  clearAuthData() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('authUser');
    localStorage.removeItem('tenantId');
  }

  // Get auth token
  getAuthToken(): string | null {
    return localStorage.getItem('authToken');
  }

  // Get auth user
  getAuthUser(): AuthResponse['user'] | null {
    const user = localStorage.getItem('authUser');
    return user ? JSON.parse(user) : null;
  }

  // Set tenant ID
  setTenantId(tenantId: string) {
    localStorage.setItem('tenantId', tenantId);
  }

  // Get tenant ID
  getTenantId(): string | null {
    return localStorage.getItem('tenantId');
  }
}

export default new AuthService();