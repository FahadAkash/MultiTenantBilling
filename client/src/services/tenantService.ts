import api from './api';
import type { Tenant } from '../features/tenant/tenantSlice';

export interface TenantDto {
  id: string;
  name: string;
  subdomain: string;
  email: string;
  status: string;
}

class TenantService {
  async getTenants(): Promise<Tenant[]> {
    const response = await api.get<TenantDto[]>('/api/admin/tenants');
    return response.data.map(tenant => ({
      id: tenant.id,
      name: tenant.name,
      subdomain: tenant.subdomain,
      email: tenant.email,
      status: tenant.status
    }));
  }

  async createTenant(data: Omit<Tenant, 'id'>): Promise<Tenant> {
    const response = await api.post<TenantDto>('/api/admin/tenants', data);
    return {
      id: response.data.id,
      name: response.data.name,
      subdomain: response.data.subdomain,
      email: response.data.email,
      status: response.data.status
    };
  }

  setCurrentTenant(tenant: Tenant) {
    localStorage.setItem('currentTenant', JSON.stringify(tenant));
  }

  getCurrentTenant(): Tenant | null {
    const tenant = localStorage.getItem('currentTenant');
    return tenant ? JSON.parse(tenant) : null;
  }

  clearCurrentTenant() {
    localStorage.removeItem('currentTenant');
  }
}

export default new TenantService();