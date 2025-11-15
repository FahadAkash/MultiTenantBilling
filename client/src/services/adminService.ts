import api from './api';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  roles: string[];
  tenantId: string; // Add tenantId property
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

export interface Subscription {
  id: string;
  tenantId: string;
  planId: string;
  startDate: string;
  endDate: string;
  status: string;
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

export interface GenerateInvoiceRequest {
  tenantId: string;
  description: string;
  amount: number;
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

  async getSubscriptionsForTenant(tenantId: string): Promise<Subscription[]> {
    const response = await api.get<Subscription[]>(`/api/admin/tenants/${tenantId}/subscriptions`);
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
  
  async generateManualInvoice(invoiceData: GenerateInvoiceRequest): Promise<any> {
    const response = await api.post<any>('/api/admin/invoices', invoiceData);
    return response.data;
  }
}

export default new AdminService();