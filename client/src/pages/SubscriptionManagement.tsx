import Layout from '../components/Layout';
import { useEffect, useState } from 'react';
import { useAppSelector, useAppDispatch } from '../hooks/useAppSelector';
import type { Subscription, Plan, BillingState } from '../features/billing/billingSlice';
import type { RootState } from '../store';
import billingService from '../services/billingService';
import { setPlans, setSubscriptions, setLoading, setError, addSubscription } from '../features/billing/billingSlice';

const SubscriptionManagement = () => {
  const dispatch = useAppDispatch();
  const billing = useAppSelector((state: RootState) => state.billing) as BillingState;
  const [showNewSubscriptionModal, setShowNewSubscriptionModal] = useState(false);
  const [selectedPlan, setSelectedPlan] = useState<string>('');

  useEffect(() => {
    const loadData = async () => {
      try {
        console.log('Loading subscription data...');
        dispatch(setLoading(true));
        
        // Log tenant ID from localStorage
        const tenantId = localStorage.getItem('tenantId');
        console.log('Current tenant ID:', tenantId);
        
        // Load plans
        console.log('Loading plans...');
        const plans = await billingService.getAllPlans();
        console.log('Plans loaded:', plans);
        dispatch(setPlans(plans));
        
        // Load subscriptions
        console.log('Loading subscriptions...');
        const subscriptions = await billingService.getSubscriptions();
        console.log('Subscriptions loaded:', subscriptions);
        dispatch(setSubscriptions(subscriptions));
        
        dispatch(setLoading(false));
        console.log('Data loading completed');
      } catch (error) {
        console.error('Error loading data:', error);
        dispatch(setError('Failed to load subscriptions and plans'));
        // Even if there's an error loading subscriptions, we should still show the plans
        dispatch(setLoading(false));
      }
    };

    loadData();
  }, [dispatch]);

  const handleNewSubscription = () => {
    setShowNewSubscriptionModal(true);
  };

  const handleCloseModal = () => {
    setShowNewSubscriptionModal(false);
    setSelectedPlan('');
  };

  const handleCreateSubscription = async () => {
    if (!selectedPlan) return;
    
    try {
      dispatch(setLoading(true));
      const newSubscription = await billingService.createSubscription({
        planId: selectedPlan
      });
      dispatch(addSubscription(newSubscription));
      dispatch(setLoading(false));
      handleCloseModal();
    } catch (error) {
      console.error('Error creating subscription:', error);
      dispatch(setError('Failed to create subscription'));
      dispatch(setLoading(false));
    }
  };

  return (
    <Layout>
      <div className="px-4 py-6 sm:px-0">
        <div className="border-4 border-dashed border-gray-200 rounded-lg h-full p-4">
          <div className="flex justify-between items-center mb-6">
            <h1 className="text-2xl font-bold text-gray-900">Subscription Management</h1>
            <button 
              onClick={handleNewSubscription}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
            >
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
                            <button 
                              onClick={() => {
                                setSelectedPlan(plan.id);
                                handleNewSubscription();
                              }}
                              className="inline-flex items-center px-3 py-1 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                            >
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
                    <p className="text-gray-400 text-sm mt-2">If you're a regular user, please contact your administrator to set up plans.</p>
                  </div>
                )}
              </div>
            </>
          )}
        </div>
      </div>

      {/* New Subscription Modal */}
      {showNewSubscriptionModal && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="mt-3">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-medium text-gray-900">New Subscription</h3>
                <button 
                  onClick={handleCloseModal}
                  className="text-gray-400 hover:text-gray-500"
                >
                  <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
              <div className="mt-4">
                <div className="mb-4">
                  <label htmlFor="plan" className="block text-sm font-medium text-gray-700">
                    Select Plan
                  </label>
                  <select
                    id="plan"
                    value={selectedPlan}
                    onChange={(e) => setSelectedPlan(e.target.value)}
                    className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                  >
                    <option value="">Choose a plan</option>
                    {billing.plans.map((plan: Plan) => (
                      <option key={plan.id} value={plan.id}>
                        {plan.name} - ${plan.monthlyPrice}/month
                      </option>
                    ))}
                  </select>
                </div>
                <div className="flex justify-end space-x-3">
                  <button
                    onClick={handleCloseModal}
                    className="inline-flex justify-center px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={handleCreateSubscription}
                    disabled={!selectedPlan}
                    className={`inline-flex justify-center px-4 py-2 text-sm font-medium text-white rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 ${
                      selectedPlan 
                        ? 'bg-indigo-600 hover:bg-indigo-700' 
                        : 'bg-gray-400 cursor-not-allowed'
                    }`}
                  >
                    Create Subscription
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
};

export default SubscriptionManagement;