import { createSlice } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';

export interface Plan {
  id: string;
  name: string;
  description: string;
  monthlyPrice: number;
  maxUsers: number;
  maxStorageGb: number;
}

export interface Subscription {
  id: string;
  tenantId: string;
  tenantName: string;
  planId: string;
  planName: string;
  startDate: string;
  endDate: string;
  status: string;
}

export interface Invoice {
  id: string;
  tenantId: string;
  subscriptionId: string;
  amount: number;
  invoiceDate: string;
  dueDate: string;
  status: string;
  isPaid: boolean;
}

export interface Payment {
  id: string;
  invoiceId: string;
  amount: number;
  paymentDate: string;
  method: string;
  status: string;
  transactionId: string | null;
  isRetry: boolean;
  retryAttempt: number;
  failureReason: string | null;
}

// DTOs that match the backend
export interface PlanDto {
  id: string;
  name: string;
  description: string;
  monthlyPrice: number;
  maxUsers: number;
  maxStorageGb: number;
}

export interface SubscriptionDto {
  id: string;
  tenantId: string;
  tenantName: string;
  planId: string;
  planName: string;
  startDate: string;
  endDate: string;
  status: string;
  invoices?: InvoiceDto[];
}

export interface InvoiceDto {
  id: string;
  tenantId: string;
  subscriptionId: string;
  amount: number;
  invoiceDate: string;
  dueDate: string;
  status: string;
  isPaid: boolean;
  payments?: PaymentDto[];
}

export interface PaymentDto {
  id: string;
  invoiceId: string;
  amount: number;
  paymentDate: string;
  method: string;
  status: string;
  transactionId: string | null;
  isRetry: boolean;
  retryAttempt: number;
  failureReason: string | null;
}

export interface BillingState {
  plans: Plan[];
  subscriptions: Subscription[];
  invoices: Invoice[];
  payments: Payment[];
  loading: boolean;
  error: string | null;
}

const initialState: BillingState = {
  plans: [],
  subscriptions: [],
  invoices: [],
  payments: [],
  loading: false,
  error: null,
};

export const billingSlice = createSlice({
  name: 'billing',
  initialState,
  reducers: {
    setPlans: (state, action: PayloadAction<Plan[]>) => {
      state.plans = action.payload;
    },
    setSubscriptions: (state, action: PayloadAction<Subscription[]>) => {
      state.subscriptions = action.payload;
    },
    setInvoices: (state, action: PayloadAction<Invoice[]>) => {
      state.invoices = action.payload;
    },
    setPayments: (state, action: PayloadAction<Payment[]>) => {
      state.payments = action.payload;
    },
    addSubscription: (state, action: PayloadAction<Subscription>) => {
      state.subscriptions.push(action.payload);
    },
    addInvoice: (state, action: PayloadAction<Invoice>) => {
      state.invoices.push(action.payload);
    },
    addPayment: (state, action: PayloadAction<Payment>) => {
      state.payments.push(action.payload);
    },
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.loading = action.payload;
    },
    setError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload;
    },
  },
});

export const {
  setPlans,
  setSubscriptions,
  setInvoices,
  setPayments,
  addSubscription,
  addInvoice,
  addPayment,
  setLoading,
  setError,
} = billingSlice.actions;

export default billingSlice.reducer;