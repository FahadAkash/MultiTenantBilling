import { configureStore } from '@reduxjs/toolkit';
// These imports will work once we create the slice files
import authReducer from '../features/auth/authSlice';
import tenantReducer from '../features/tenant/tenantSlice';
import billingReducer from '../features/billing/billingSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    tenant: tenantReducer,
    billing: billingReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;