import billingService from './billingService';

// Mock the api module
jest.mock('./api', () => ({
  get: jest.fn(),
  post: jest.fn(),
}));

describe('BillingService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should fetch all plans', async () => {
    const mockPlans = [
      {
        id: '1',
        name: 'Basic',
        description: 'Basic plan',
        monthlyPrice: 10,
        maxUsers: 5,
        maxStorageGb: 10
      }
    ];

    const api = (await import('./api')).default;
    (api.get as jest.Mock).mockResolvedValue({ data: mockPlans });

    const plans = await billingService.getAllPlans();
    expect(plans).toEqual(mockPlans);
    expect(api.get).toHaveBeenCalledWith('/api/billing/plans');
  });

  it('should create a subscription', async () => {
    const mockSubscription = {
      id: '1',
      tenantId: 'tenant-1',
      tenantName: 'Test Tenant',
      planId: 'plan-1',
      planName: 'Basic',
      startDate: new Date().toString(),
      endDate: new Date().toString(),
      status: 'active'
    };

    const api = (await import('./api')).default;
    (api.post as jest.Mock).mockResolvedValue({ data: mockSubscription });

    const subscription = await billingService.createSubscription({ planId: 'plan-1' });
    expect(subscription).toEqual(mockSubscription);
    expect(api.post).toHaveBeenCalledWith('/api/billing/subscriptions', { planId: 'plan-1' });
  });
});