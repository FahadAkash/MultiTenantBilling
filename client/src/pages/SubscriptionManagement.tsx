import Layout from '../components/Layout';
import { useAppSelector } from '../hooks/useAppSelector';
import type { Subscription, Plan, BillingState } from '../features/billing/billingSlice';
import type { RootState } from '../store';

const SubscriptionManagement = () => {
  const billing = useAppSelector((state: RootState) => state.billing) as BillingState;

  return (
    <Layout>
      <div className="px-4 py-6 sm:px-0">
        <div className="border-4 border-dashed border-gray-200 rounded-lg h-full p-4">
          <div className="flex justify-between items-center mb-6">
            <h1 className="text-2xl font-bold text-gray-900">Subscription Management</h1>
            <button className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
              New Subscription
            </button>
          </div>

          {billing.loading ? (
            <div className="flex justify-center items-center h-64">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
            </div>
          ) : (
            <>
              <div className="mb-8">
                <h2 className="text-lg font-medium text-gray-900 mb-4">Active Subscriptions</h2>
                {billing.subscriptions.length > 0 ? (
                  <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
                    {billing.subscriptions.map((subscription: Subscription) => (
                      <div key={subscription.id} className="bg-white overflow-hidden shadow rounded-lg">
                        <div className="px-4 py-5 sm:p-6">
                          <h3 className="text-lg font-medium text-gray-900">{subscription.planName}</h3>
                          <div className="mt-2 text-sm text-gray-500">
                            <p>Start Date: {new Date(subscription.startDate).toLocaleDateString()}</p>
                            <p>End Date: {new Date(subscription.endDate).toLocaleDateString()}</p>
                            <p>Status: <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                              {subscription.status}
                            </span></p>
                          </div>
                          <div className="mt-4">
                            <button className="inline-flex items-center px-3 py-1 border border-transparent text-sm font-medium rounded-md text-indigo-700 bg-indigo-100 hover:bg-indigo-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
                              View Details
                            </button>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="text-center py-12">
                    <p className="text-gray-500">No active subscriptions found.</p>
                  </div>
                )}
              </div>

              <div>
                <h2 className="text-lg font-medium text-gray-900 mb-4">Available Plans</h2>
                {billing.plans.length > 0 ? (
                  <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
                    {billing.plans.map((plan: Plan) => (
                      <div key={plan.id} className="bg-white overflow-hidden shadow rounded-lg">
                        <div className="px-4 py-5 sm:p-6">
                          <h3 className="text-lg font-medium text-gray-900">{plan.name}</h3>
                          <p className="mt-1 text-sm text-gray-500">{plan.description}</p>
                          <div className="mt-2 text-sm text-gray-500">
                            <p>Price: ${plan.monthlyPrice}/month</p>
                            <p>Max Users: {plan.maxUsers}</p>
                            <p>Max Storage: {plan.maxStorageGb} GB</p>
                          </div>
                          <div className="mt-4">
                            <button className="inline-flex items-center px-3 py-1 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
                              Subscribe
                            </button>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="text-center py-12">
                    <p className="text-gray-500">No plans available.</p>
                  </div>
                )}
              </div>
            </>
          )}
        </div>
      </div>
    </Layout>
  );
};

export default SubscriptionManagement;