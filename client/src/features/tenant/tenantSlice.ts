import { createSlice } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';

export interface Tenant {
  id: string;
  name: string;
  subdomain: string;
  email: string;
  status: string;
}

interface TenantState {
  currentTenant: Tenant | null;
  tenants: Tenant[];
  loading: boolean;
  error: string | null;
}

const initialState: TenantState = {
  currentTenant: null,
  tenants: [],
  loading: false,
  error: null,
};

export const tenantSlice = createSlice({
  name: 'tenant',
  initialState,
  reducers: {
    setCurrentTenant: (state, action: PayloadAction<Tenant>) => {
      state.currentTenant = action.payload;
    },
    clearCurrentTenant: (state) => {
      state.currentTenant = null;
    },
    setTenants: (state, action: PayloadAction<Tenant[]>) => {
      state.tenants = action.payload;
    },
    addTenant: (state, action: PayloadAction<Tenant>) => {
      state.tenants.push(action.payload);
    },
  },
});

export const {
  setCurrentTenant,
  clearCurrentTenant,
  setTenants,
  addTenant,
} = tenantSlice.actions;

export default tenantSlice.reducer;