import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import { useSelector } from 'react-redux';
import type { RootState } from '../store';
import type { AuthState } from '../features/auth/authSlice';
import adminService from '../services/adminService';
import billingService from '../services/billingService';

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  roles: string[];
  tenantId: string; // Add tenantId property
}

interface Tenant {
  id: string;
  tenantId: string;
  name: string;
  subdomain: string;
  email: string;
  status: string;
}

interface Plan {
  id: string;
  name: string;
  description: string;
  monthlyPrice: number;
  maxUsers: number;
  maxStorageGb: number;
  isActive: boolean;
}

interface Subscription {
  id: string;
  tenantId: string;
  planId: string;
  startDate: string;
  endDate: string;
  status: string;
}

const AdminDashboard = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [tenants, setTenants] = useState<Tenant[]>([]);
  const [subscriptions, setSubscriptions] = useState<Subscription[]>([]);
  const [selectedTenant, setSelectedTenant] = useState<Tenant | null>(null);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [showCreatePlanForm, setShowCreatePlanForm] = useState(false);
  const [newPlan, setNewPlan] = useState({
    name: '',
    description: '',
    monthlyPrice: 0,
    maxUsers: 0,
    maxStorageGb: 0,
    isActive: true
  });
  const [showCreateUserForm, setShowCreateUserForm] = useState(false);
  const [newUser, setNewUser] = useState({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    tenantId: '',
    roles: [] as string[]
  });
  const [showGenerateInvoiceForm, setShowGenerateInvoiceForm] = useState(false);
  const [newInvoice, setNewInvoice] = useState({
    tenantId: '',
    description: '',
    amount: 0
  });
  const [availableRoles] = useState(['User', 'Admin']);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const auth = useSelector((state: RootState) => state.auth) as AuthState;

  useEffect(() => {
    fetchUsers();
    fetchTenants();
  }, []);

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const users = await adminService.getAllUsers();
      setUsers(users);
      setError(null);
    } catch (err) {
      setError('Failed to fetch users');
      console.error('Error fetching users:', err);
    } finally {
      setLoading(false);
    }
  };

  const fetchTenants = async () => {
    try {
      const tenants = await adminService.getAllTenants();
      setTenants(tenants);
    } catch (err) {
      setError('Failed to fetch tenants');
      console.error('Error fetching tenants:', err);
    }
  };

  const fetchSubscriptionsForTenant = async (tenantId: string) => {
    try {
      const subs = await adminService.getSubscriptionsForTenant(tenantId);
      setSubscriptions(subs);
    } catch (err) {
      setError('Failed to fetch subscriptions');
      console.error('Error fetching subscriptions:', err);
    }
  };

  const activateUser = async (userId: string) => {
    try {
      await adminService.activateUser(userId);
      fetchUsers(); // Refresh the user list
    } catch (err) {
      setError('Failed to activate user');
      console.error('Error activating user:', err);
    }
  };

  const deactivateUser = async (userId: string) => {
    try {
      await adminService.deactivateUser(userId);
      fetchUsers(); // Refresh the user list
    } catch (err) {
      setError('Failed to deactivate user');
      console.error('Error deactivating user:', err);
    }
  };

  const handleTenantSelect = async (tenant: Tenant) => {
    setSelectedTenant(tenant);
    setSelectedUser(null);
    setSubscriptions([]);
    await fetchSubscriptionsForTenant(tenant.id);
  };

  const handleUserSelect = (user: User) => {
    setSelectedUser(user);
    // Find the tenant for this user
    const userTenant = tenants.find(t => t.id === user.tenantId);
    if (userTenant) {
      setSelectedTenant(userTenant);
      fetchSubscriptionsForTenant(userTenant.id);
    }
  };

  const handleCreatePlan = async () => {
    if (!selectedTenant) {
      setError('Please select a tenant first');
      return;
    }

    try {
      await adminService.createPlanForTenant(selectedTenant.id, newPlan);
      setShowCreatePlanForm(false);
      setNewPlan({
        name: '',
        description: '',
        monthlyPrice: 0,
        maxUsers: 0,
        maxStorageGb: 0,
        isActive: true
      });
      setError(null);
    } catch (err) {
      setError('Failed to create plan');
      console.error('Error creating plan:', err);
    }
  };

  const handleCreateUser = async () => {
    try {
      await adminService.adminRegister(newUser);
      setShowCreateUserForm(false);
      setNewUser({
        email: '',
        password: '',
        firstName: '',
        lastName: '',
        tenantId: '',
        roles: []
      });
      setError(null);
      fetchUsers(); // Refresh the user list
    } catch (err) {
      setError('Failed to create user');
      console.error('Error creating user:', err);
    }
  };

  const handleRoleChange = (role: string) => {
    setNewUser(prev => {
      const roles = prev.roles.includes(role)
        ? prev.roles.filter(r => r !== role)
        : [...prev.roles, role];
      return { ...prev, roles };
    });
  };

  const handleGenerateInvoice = async () => {
    try {
      await adminService.generateManualInvoice(newInvoice);
      setShowGenerateInvoiceForm(false);
      setNewInvoice({
        tenantId: '',
        description: '',
        amount: 0
      });
      setError(null);
    } catch (err) {
      setError('Failed to generate invoice');
      console.error('Error generating invoice:', err);
    }
  };

  if (!auth.user?.roles.includes('Admin')) {
    return (
      <Layout>
        <div className="px-4 py-6 sm:px-0">
          <div className="border-4 border-dashed border-gray-200 rounded-lg h-96 p-4 flex items-center justify-center">
            <div className="text-center">
              <h2 className="text-2xl font-bold text-gray-900 mb-2">Access Denied</h2>
              <p className="text-gray-600">You don't have permission to access the admin dashboard.</p>
            </div>
          </div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="px-4 py-6 sm:px-0">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-gray-900">Admin Dashboard</h1>
          <p className="text-gray-600 mt-1">Manage users, roles, and tenant settings</p>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-md p-4 mb-6">
            <div className="flex">
              <div className="flex-shrink-0">
                <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                </svg>
              </div>
              <div className="ml-3">
                <h3 className="text-sm font-medium text-red-800">{error}</h3>
              </div>
            </div>
          </div>
        )}

        {/* Tenant Selection Section */}
        <div className="bg-white shadow overflow-hidden sm:rounded-lg mb-6">
          <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
            <h2 className="text-lg leading-6 font-medium text-gray-900">Tenant Management</h2>
            <p className="mt-1 max-w-2xl text-sm text-gray-500">Select a tenant to manage their plans</p>
          </div>
          <div className="px-4 py-5 sm:px-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label htmlFor="tenant-select" className="block text-sm font-medium text-gray-700 mb-2">
                  Select Tenant
                </label>
                <select
                  id="tenant-select"
                  value={selectedTenant?.id || ''}
                  onChange={(e) => {
                    const tenant = tenants.find(t => t.id === e.target.value);
                    if (tenant) handleTenantSelect(tenant);
                  }}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                >
                  <option value="">Choose a tenant</option>
                  {tenants.map((tenant) => (
                    <option key={tenant.id} value={tenant.id}>
                      {tenant.name} ({tenant.subdomain})
                    </option>
                  ))}
                </select>
                {selectedTenant && (
                  <div className="mt-4 p-4 bg-gray-50 rounded-md">
                    <h3 className="text-md font-medium text-gray-900">Selected Tenant</h3>
                    <p className="text-sm text-gray-500">Name: {selectedTenant.name}</p>
                    <p className="text-sm text-gray-500">Subdomain: {selectedTenant.subdomain}</p>
                    <p className="text-sm text-gray-500">Email: {selectedTenant.email}</p>
                    <p className="text-sm text-gray-500">Status: {selectedTenant.status}</p>
                  </div>
                )}
              </div>
              <div>
                <button
                  onClick={() => setShowCreatePlanForm(true)}
                  disabled={!selectedTenant}
                  className={`inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white ${
                    selectedTenant 
                      ? 'bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500' 
                      : 'bg-gray-400 cursor-not-allowed'
                  }`}
                >
                  Create New Plan
                </button>
                <button
                  onClick={() => setShowCreateUserForm(true)}
                  className="ml-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500"
                >
                  Create New User
                </button>
                <button
                  onClick={() => setShowGenerateInvoiceForm(true)}
                  className="ml-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-purple-600 hover:bg-purple-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500"
                >
                  Generate Invoice
                </button>
              </div>
            </div>
          </div>
        </div>

        {/* User Selection Section */}
        <div className="bg-white shadow overflow-hidden sm:rounded-lg mb-6">
          <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
            <h2 className="text-lg leading-6 font-medium text-gray-900">User Management</h2>
            <p className="mt-1 max-w-2xl text-sm text-gray-500">Select a user to manage their subscriptions</p>
          </div>
          <div className="px-4 py-5 sm:px-6">
            <div className="grid grid-cols-1 gap-6">
              <div>
                <label htmlFor="user-select" className="block text-sm font-medium text-gray-700 mb-2">
                  Select User
                </label>
                <select
                  id="user-select"
                  value={selectedUser?.id || ''}
                  onChange={(e) => {
                    const user = users.find(u => u.id === e.target.value);
                    if (user) handleUserSelect(user);
                  }}
                  className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                >
                  <option value="">Choose a user</option>
                  {users.map((user) => (
                    <option key={user.id} value={user.id}>
                      {user.firstName} {user.lastName} ({user.email})
                    </option>
                  ))}
                </select>
                {selectedUser && (
                  <div className="mt-4 p-4 bg-gray-50 rounded-md">
                    <h3 className="text-md font-medium text-gray-900">Selected User</h3>
                    <p className="text-sm text-gray-500">Name: {selectedUser.firstName} {selectedUser.lastName}</p>
                    <p className="text-sm text-gray-500">Email: {selectedUser.email}</p>
                    <p className="text-sm text-gray-500">Status: {selectedUser.isActive ? 'Active' : 'Inactive'}</p>
                    <p className="text-sm text-gray-500">Roles: {selectedUser.roles.join(', ') || 'No roles assigned'}</p>
                  </div>
                )}
              </div>
              
              {/* Subscriptions for selected user */}
              {selectedUser && subscriptions.length > 0 && (
                <div className="mt-4">
                  <h3 className="text-md font-medium text-gray-900 mb-2">Active Subscriptions</h3>
                  <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                      <thead className="bg-gray-50">
                        <tr>
                          <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Plan
                          </th>
                          <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Start Date
                          </th>
                          <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            End Date
                          </th>
                          <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Status
                          </th>
                        </tr>
                      </thead>
                      <tbody className="bg-white divide-y divide-gray-200">
                        {subscriptions
                          .filter(sub => sub.status === 'Active')
                          .map((subscription) => (
                            <tr key={subscription.id}>
                              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                                {/* We would need to fetch plan details to show the plan name */}
                                Plan ID: {subscription.planId}
                              </td>
                              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                {new Date(subscription.startDate).toLocaleDateString()}
                              </td>
                              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                {new Date(subscription.endDate).toLocaleDateString()}
                              </td>
                              <td className="px-6 py-4 whitespace-nowrap">
                                <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                                  subscription.status === 'Active' 
                                    ? 'bg-green-100 text-green-800' 
                                    : 'bg-yellow-100 text-yellow-800'
                                }`}>
                                  {subscription.status}
                                </span>
                              </td>
                            </tr>
                          ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Create Plan Form Modal */}
        {showCreatePlanForm && (
          <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
            <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
              <div className="mt-3">
                <div className="flex items-center justify-between">
                  <h3 className="text-lg font-medium text-gray-900">Create New Plan</h3>
                  <button 
                    onClick={() => setShowCreatePlanForm(false)}
                    className="text-gray-400 hover:text-gray-500"
                  >
                    <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
                <div className="mt-4">
                  <div className="space-y-4">
                    <div>
                      <label htmlFor="plan-name" className="block text-sm font-medium text-gray-700">
                        Plan Name
                      </label>
                      <input
                        type="text"
                        id="plan-name"
                        value={newPlan.name}
                        onChange={(e) => setNewPlan({...newPlan, name: e.target.value})}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                      />
                    </div>
                    <div>
                      <label htmlFor="plan-description" className="block text-sm font-medium text-gray-700">
                        Description
                      </label>
                      <textarea
                        id="plan-description"
                        value={newPlan.description}
                        onChange={(e) => setNewPlan({...newPlan, description: e.target.value})}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                      />
                    </div>
                    <div>
                      <label htmlFor="plan-price" className="block text-sm font-medium text-gray-700">
                        Monthly Price ($)
                      </label>
                      <input
                        type="number"
                        id="plan-price"
                        value={newPlan.monthlyPrice}
                        onChange={(e) => setNewPlan({...newPlan, monthlyPrice: parseFloat(e.target.value) || 0})}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                      />
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label htmlFor="plan-users" className="block text-sm font-medium text-gray-700">
                          Max Users
                        </label>
                        <input
                          type="number"
                          id="plan-users"
                          value={newPlan.maxUsers}
                          onChange={(e) => setNewPlan({...newPlan, maxUsers: parseInt(e.target.value) || 0})}
                          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                        />
                      </div>
                      <div>
                        <label htmlFor="plan-storage" className="block text-sm font-medium text-gray-700">
                          Max Storage (GB)
                        </label>
                        <input
                          type="number"
                          id="plan-storage"
                          value={newPlan.maxStorageGb}
                          onChange={(e) => setNewPlan({...newPlan, maxStorageGb: parseInt(e.target.value) || 0})}
                          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                        />
                      </div>
                    </div>
                    <div className="flex items-center">
                      <input
                        id="plan-active"
                        type="checkbox"
                        checked={newPlan.isActive}
                        onChange={(e) => setNewPlan({...newPlan, isActive: e.target.checked})}
                        className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                      />
                      <label htmlFor="plan-active" className="ml-2 block text-sm text-gray-900">
                        Active
                      </label>
                    </div>
                  </div>
                  <div className="mt-6 flex justify-end space-x-3">
                    <button
                      onClick={() => setShowCreatePlanForm(false)}
                      className="inline-flex justify-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                    >
                      Cancel
                    </button>
                    <button
                      onClick={handleCreatePlan}
                      className="inline-flex justify-center px-4 py-2 text-sm font-medium text-white bg-indigo-600 border border-transparent rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                    >
                      Create Plan
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Create User Form Modal */}
        {showCreateUserForm && (
          <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
            <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
              <div className="mt-3">
                <div className="flex items-center justify-between">
                  <h3 className="text-lg font-medium text-gray-900">Create New User</h3>
                  <button 
                    onClick={() => setShowCreateUserForm(false)}
                    className="text-gray-400 hover:text-gray-500"
                  >
                    <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
                <div className="mt-4">
                  <div className="space-y-4">
                    <div>
                      <label htmlFor="user-first-name" className="block text-sm font-medium text-gray-700">
                        First Name
                      </label>
                      <input
                        type="text"
                        id="user-first-name"
                        value={newUser.firstName}
                        onChange={(e) => setNewUser({...newUser, firstName: e.target.value})}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                      />
                    </div>
                    <div>
                      <label htmlFor="user-last-name" className="block text-sm font-medium text-gray-700">
                        Last Name
                      </label>
                      <input
                        type="text"
                        id="user-last-name"
                        value={newUser.lastName}
                        onChange={(e) => setNewUser({...newUser, lastName: e.target.value})}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                      />
                    </div>
                    <div>
                      <label htmlFor="user-email" className="block text-sm font-medium text-gray-700">
                        Email
                      </label>
                      <input
                        type="email"
                        id="user-email"
                        value={newUser.email}
                        onChange={(e) => setNewUser({...newUser, email: e.target.value})}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                      />
                    </div>
                    <div>
                      <label htmlFor="user-password" className="block text-sm font-medium text-gray-700">
                        Password
                      </label>
                      <input
                        type="password"
                        id="user-password"
                        value={newUser.password}
                        onChange={(e) => setNewUser({...newUser, password: e.target.value})}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                      />
                    </div>
                    <div>
                      <label htmlFor="user-tenant" className="block text-sm font-medium text-gray-700">
                        Tenant
                      </label>
                      <select
                        id="user-tenant"
                        value={newUser.tenantId}
                        onChange={(e) => setNewUser({...newUser, tenantId: e.target.value})}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                      >
                        <option value="">Select a tenant</option>
                        {tenants.map((tenant) => (
                          <option key={tenant.id} value={tenant.id}>
                            {tenant.name} ({tenant.subdomain})
                          </option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700">
                        Roles
                      </label>
                      <div className="mt-2 space-y-2">
                        {availableRoles.map((role) => (
                          <div key={role} className="flex items-center">
                            <input
                              id={`role-${role}`}
                              type="checkbox"
                              checked={newUser.roles.includes(role)}
                              onChange={() => handleRoleChange(role)}
                              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                            />
                            <label htmlFor={`role-${role}`} className="ml-2 block text-sm text-gray-900">
                              {role}
                            </label>
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>
                  <div className="mt-6 flex justify-end space-x-3">
                    <button
                      onClick={() => setShowCreateUserForm(false)}
                      className="inline-flex justify-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                    >
                      Cancel
                    </button>
                    <button
                      onClick={handleCreateUser}
                      className="inline-flex justify-center px-4 py-2 text-sm font-medium text-white bg-green-600 border border-transparent rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500"
                    >
                      Create User
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Generate Invoice Form Modal */}
        {showGenerateInvoiceForm && (
          <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
            <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
              <div className="mt-3">
                <div className="flex items-center justify-between">
                  <h3 className="text-lg font-medium text-gray-900">Generate Manual Invoice</h3>
                  <button 
                    onClick={() => setShowGenerateInvoiceForm(false)}
                    className="text-gray-400 hover:text-gray-500"
                  >
                    <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
                <div className="mt-4">
                  <div className="space-y-4">
                    <div>
                      <label htmlFor="invoice-tenant" className="block text-sm font-medium text-gray-700">
                        Tenant
                      </label>
                      <select
                        id="invoice-tenant"
                        value={newInvoice.tenantId}
                        onChange={(e) => setNewInvoice({...newInvoice, tenantId: e.target.value})}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                      >
                        <option value="">Select a tenant</option>
                        {tenants.map((tenant) => (
                          <option key={tenant.id} value={tenant.id}>
                            {tenant.name} ({tenant.subdomain})
                          </option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label htmlFor="invoice-description" className="block text-sm font-medium text-gray-700">
                        Description
                      </label>
                      <input
                        type="text"
                        id="invoice-description"
                        value={newInvoice.description}
                        onChange={(e) => setNewInvoice({...newInvoice, description: e.target.value})}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                      />
                    </div>
                    <div>
                      <label htmlFor="invoice-amount" className="block text-sm font-medium text-gray-700">
                        Amount ($)
                      </label>
                      <input
                        type="number"
                        id="invoice-amount"
                        value={newInvoice.amount}
                        onChange={(e) => setNewInvoice({...newInvoice, amount: parseFloat(e.target.value) || 0})}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                      />
                    </div>
                  </div>
                  <div className="mt-6 flex justify-end space-x-3">
                    <button
                      onClick={() => setShowGenerateInvoiceForm(false)}
                      className="inline-flex justify-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                    >
                      Cancel
                    </button>
                    <button
                      onClick={handleGenerateInvoice}
                      disabled={!newInvoice.tenantId || newInvoice.amount <= 0}
                      className={`inline-flex justify-center px-4 py-2 text-sm font-medium text-white rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 ${
                        newInvoice.tenantId && newInvoice.amount > 0
                          ? 'bg-purple-600 hover:bg-purple-700 border border-transparent'
                          : 'bg-gray-400 cursor-not-allowed'
                      }`}
                    >
                      Generate Invoice
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* User Management Section */}
        <div className="bg-white shadow overflow-hidden sm:rounded-lg">
          <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
            <h2 className="text-lg leading-6 font-medium text-gray-900">User Management</h2>
            <p className="mt-1 max-w-2xl text-sm text-gray-500">Manage user accounts and their status</p>
          </div>
          <div className="px-4 py-5 sm:px-6">
            {loading ? (
              <div className="flex justify-center items-center h-32">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        User
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Email
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Status
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Roles
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Actions
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {users.map((user) => (
                      <tr key={user.id}>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm font-medium text-gray-900">
                            {user.firstName} {user.lastName}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {user.email}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                            user.isActive 
                              ? 'bg-green-100 text-green-800' 
                              : 'bg-red-100 text-red-800'
                          }`}>
                            {user.isActive ? 'Active' : 'Inactive'}
                          </span>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {user.roles.join(', ') || 'No roles assigned'}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                          {user.isActive ? (
                            <button
                              onClick={() => deactivateUser(user.id)}
                              className="text-red-600 hover:text-red-900"
                            >
                              Deactivate
                            </button>
                          ) : (
                            <button
                              onClick={() => activateUser(user.id)}
                              className="text-green-600 hover:text-green-900"
                            >
                              Activate
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default AdminDashboard;