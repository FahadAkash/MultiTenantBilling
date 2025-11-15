import api from './api';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
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
}

export default new AdminService();