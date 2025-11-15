import api from './api';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  roles: string[];
}

export interface Tenant {
  id: string;
  tenantId: string;
  name: string;
  subdomain: string;
  email: string;
  status: string;
}

export interface Plan {
  id: string;
  name: string;
  description: string;
  monthlyPrice: number;
  maxUsers: number;
  maxStorageGb: number;
  isActive: boolean;
}

export interface CreatePlanRequest {
  name: string;
  description: string;
  monthlyPrice: number;
  maxUsers: number;
  maxStorageGb: number;
  isActive: boolean;
}

export interface AdminRegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  tenantId: string;
  roles: string[];
}

class AdminService {
  async getAllUsers(): Promise<User[]> {
    const response = await api.get<User[]>('/api/admin/users');
    return response.data;
  }

  async activateUser(userId: string): Promise<boolean> {
    const response = await api.post<boolean>(`/api/admin/users/${userId}/activate`);
    return response.data;
  }

  async deactivateUser(userId: string): Promise<boolean> {
    const response = await api.post<boolean>(`/api/admin/users/${userId}/deactivate`);
    return response.data;
  }

  async getAllTenants(): Promise<Tenant[]> {
    const response = await api.get<Tenant[]>('/api/admin/tenants');
    return response.data;
  }

  async createPlanForTenant(tenantId: string, planData: CreatePlanRequest): Promise<Plan> {
    const response = await api.post<Plan>(`/api/admin/tenants/${tenantId}/plans`, planData);
    return response.data;
  }

  async adminRegister(userData: AdminRegisterRequest): Promise<any> {
    const response = await api.post<any>('/api/admin/register', userData);
    return response.data;
  }
}

export default new AdminService();