import { configureStore } from '@reduxjs/toolkit';
// These imports will work once we create the slice files
import authReducer from '../features/auth/authSlice';
import tenantReducer from '../features/tenant/tenantSlice';
import billingReducer from '../features/billing/billingSlice';
import type { User } from '../features/auth/authSlice';

// Load initial auth state from localStorage
const loadInitialAuthState = () => {
  try {
    const token = localStorage.getItem('authToken');
    const user = localStorage.getItem('authUser');
    
    if (token && user) {
      return {
        user: JSON.parse(user) as User,
        token,
        isAuthenticated: true,
        loading: false,
        error: null,
      };
    }
  } catch (error) {
    // If there's an error parsing, return initial state
    console.warn('Error loading auth state from localStorage:', error);
  }
  
  // Return default initial state
  return {
    user: null,
    token: null,
    isAuthenticated: false,
    loading: false,
    error: null,
  };
};

const preloadedAuthState = loadInitialAuthState();

export const store = configureStore({
  reducer: {
    auth: authReducer,
    tenant: tenantReducer,
    billing: billingReducer,
  },
  preloadedState: {
    auth: preloadedAuthState,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;