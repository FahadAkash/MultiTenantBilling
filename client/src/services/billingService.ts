import api from './api';
import type { 
  Plan, 
  Subscription, 
  Invoice, 
  Payment,
  PlanDto,
  SubscriptionDto,
  InvoiceDto,
  PaymentDto
} from '../features/billing/billingSlice';

export interface CreateSubscriptionRequest {
  planId: string;
  paymentMethodId?: string;
}

export interface RecordUsageRequest {
  metricName: string;
  quantity: number;
}

export interface ProcessPaymentRequest {
  paymentMethodId: string;
}

class BillingService {
  // Plans
  async getAllPlans(): Promise<Plan[]> {
    const response = await api.get<PlanDto[]>('/api/billing/plans');
    return response.data.map(plan => ({
      id: plan.id,
      name: plan.name,
      description: plan.description,
      monthlyPrice: plan.monthlyPrice,
      maxUsers: plan.maxUsers,
      maxStorageGb: plan.maxStorageGb
    }));
  }

  // Subscriptions
  async createSubscription(data: CreateSubscriptionRequest): Promise<Subscription> {
    const response = await api.post<SubscriptionDto>('/api/billing/subscriptions', data);
    return {
      id: response.data.id,
      tenantId: response.data.tenantId,
      tenantName: response.data.tenantName,
      planId: response.data.planId,
      planName: response.data.planName,
      startDate: response.data.startDate.toString(),
      endDate: response.data.endDate.toString(),
      status: response.data.status
    };
  }

  async getSubscriptions(): Promise<Subscription[]> {
    const response = await api.get<SubscriptionDto[]>('/api/billing/subscriptions');
    return response.data.map(sub => ({
      id: sub.id,
      tenantId: sub.tenantId,
      tenantName: sub.tenantName,
      planId: sub.planId,
      planName: sub.planName,
      startDate: sub.startDate.toString(),
      endDate: sub.endDate.toString(),
      status: sub.status
    }));
  }

  // Invoices
  async generateInvoice(subscriptionId: string): Promise<Invoice> {
    const response = await api.post<InvoiceDto>(`/api/billing/subscriptions/${subscriptionId}/invoices`);
    return {
      id: response.data.id,
      tenantId: response.data.tenantId,
      subscriptionId: response.data.subscriptionId,
      amount: response.data.amount,
      invoiceDate: response.data.invoiceDate.toString(),
      dueDate: response.data.dueDate.toString(),
      status: response.data.status,
      isPaid: response.data.isPaid
    };
  }

  async getInvoices(): Promise<Invoice[]> {
    const response = await api.get<InvoiceDto[]>('/api/billing/invoices');
    return response.data.map(invoice => ({
      id: invoice.id,
      tenantId: invoice.tenantId,
      subscriptionId: invoice.subscriptionId,
      amount: invoice.amount,
      invoiceDate: invoice.invoiceDate.toString(),
      dueDate: invoice.dueDate.toString(),
      status: invoice.status,
      isPaid: invoice.isPaid
    }));
  }

  // Payments
  async processPayment(invoiceId: string, data: ProcessPaymentRequest): Promise<Payment> {
    const response = await api.post<PaymentDto>(`/api/billing/invoices/${invoiceId}/payments`, data);
    return {
      id: response.data.id,
      invoiceId: response.data.invoiceId,
      amount: response.data.amount,
      paymentDate: response.data.paymentDate.toString(),
      method: response.data.method,
      status: response.data.status,
      transactionId: response.data.transactionId
    };
  }

  async getPayments(): Promise<Payment[]> {
    const response = await api.get<PaymentDto[]>('/api/billing/payments');
    return response.data.map(payment => ({
      id: payment.id,
      invoiceId: payment.invoiceId,
      amount: payment.amount,
      paymentDate: payment.paymentDate.toString(),
      method: payment.method,
      status: payment.status,
      transactionId: payment.transactionId
    }));
  }

  // Usage
  async recordUsage(subscriptionId: string, data: RecordUsageRequest) {
    const response = await api.post(`/api/billing/subscriptions/${subscriptionId}/usage`, data);
    return response.data;
  }
}

export default new BillingService();