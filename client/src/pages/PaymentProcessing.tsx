import Layout from '../components/Layout';
import { useState, useEffect } from 'react';
import { useAppSelector, useAppDispatch } from '../hooks/useAppSelector';
import type { Invoice, Payment } from '../features/billing/billingSlice';
import type { RootState } from '../store';
import type { BillingState } from '../features/billing/billingSlice';
import billingService from '../services/billingService';
import { setInvoices, setPayments, addPayment, setLoading, setError } from '../features/billing/billingSlice';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const PaymentProcessing = () => {
  const dispatch = useAppDispatch();
  const billing = useAppSelector((state: RootState) => state.billing) as BillingState;
  const [selectedInvoice, setSelectedInvoice] = useState<string>('');
  const [paymentMethod, setPaymentMethod] = useState<string>('');
  const [isProcessing, setIsProcessing] = useState<boolean>(false);

  useEffect(() => {
    const loadData = async () => {
      try {
        dispatch(setLoading(true));
        // Load invoices
        const invoices = await billingService.getInvoices();
        dispatch(setInvoices(invoices));
        
        // Load payments
        const payments = await billingService.getPayments();
        dispatch(setPayments(payments));
        
        dispatch(setLoading(false));
      } catch (error) {
        console.error('Error loading data:', error);
        dispatch(setError('Failed to load invoices and payments'));
        dispatch(setLoading(false));
        toast.error('Failed to load data');
      }
    };

    loadData();
  }, [dispatch]);

  const handlePaymentSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!selectedInvoice || !paymentMethod) {
      toast.error('Please select an invoice and payment method');
      return;
    }

    setIsProcessing(true);
    dispatch(setLoading(true));
    
    try {
      // Process the payment
      const payment = await billingService.processPayment(selectedInvoice, { paymentMethodId: paymentMethod });
      
      // Add the payment to the store
      dispatch(addPayment(payment));
      
      // Update the invoice status in the store
      const updatedInvoices = billing.invoices.map(invoice => 
        invoice.id === selectedInvoice ? { ...invoice, isPaid: true, status: 'Paid' } : invoice
      );
      dispatch(setInvoices(updatedInvoices));
      
      // Reset form
      setSelectedInvoice('');
      setPaymentMethod('');
      
      // Show success message
      toast.success('Payment processed successfully!');
    } catch (error: any) {
      console.error('Error processing payment:', error);
      const errorMessage = error.response?.data?.message || error.message || 'Failed to process payment';
      dispatch(setError(errorMessage));
      toast.error(`Payment failed: ${errorMessage}`);
    } finally {
      setIsProcessing(false);
      dispatch(setLoading(false));
    }
  };

  return (
    <Layout>
      <div className="px-4 py-6 sm:px-0">
        <div className="border-4 border-dashed border-gray-200 rounded-lg h-full p-4">
          <div className="flex justify-between items-center mb-6">
            <h1 className="text-2xl font-bold text-gray-900">Payment Processing</h1>
          </div>

          <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
            <div className="bg-white shadow sm:rounded-lg">
              <div className="px-4 py-5 sm:p-6">
                <h3 className="text-lg font-medium leading-6 text-gray-900">Process Payment</h3>
                <div className="mt-5">
                  <form onSubmit={handlePaymentSubmit} className="space-y-6">
                    <div>
                      <label htmlFor="invoice" className="block text-sm font-medium text-gray-700">
                        Select Invoice
                      </label>
                      <select
                        id="invoice"
                        name="invoice"
                        value={selectedInvoice}
                        onChange={(e) => setSelectedInvoice(e.target.value)}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                        disabled={isProcessing}
                      >
                        <option value="">Choose an invoice</option>
                        {billing.invoices
                          .filter(invoice => !invoice.isPaid)
                          .map((invoice: Invoice) => (
                            <option key={invoice.id} value={invoice.id}>
                              {invoice.id.substring(0, 8)} - ${invoice.amount.toFixed(2)} - {new Date(invoice.invoiceDate).toLocaleDateString()}
                            </option>
                          ))}
                      </select>
                    </div>

                    <div>
                      <label htmlFor="payment-method" className="block text-sm font-medium text-gray-700">
                        Payment Method
                      </label>
                      <select
                        id="payment-method"
                        name="payment-method"
                        value={paymentMethod}
                        onChange={(e) => setPaymentMethod(e.target.value)}
                        className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
                        disabled={isProcessing}
                      >
                        <option value="">Choose a payment method</option>
                        <option value="credit-card">Credit Card</option>
                        <option value="bank-transfer">Bank Transfer</option>
                        <option value="paypal">PayPal</option>
                      </select>
                    </div>

                    <div>
                      <button
                        type="submit"
                        disabled={!selectedInvoice || !paymentMethod || isProcessing}
                        className={`w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white ${
                          selectedInvoice && paymentMethod && !isProcessing
                            ? 'bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500'
                            : 'bg-gray-400 cursor-not-allowed'
                        }`}
                      >
                        {isProcessing ? 'Processing...' : 'Process Payment'}
                      </button>
                    </div>
                  </form>
                </div>
              </div>
            </div>

            <div className="bg-white shadow sm:rounded-lg">
              <div className="px-4 py-5 sm:p-6">
                <h3 className="text-lg font-medium leading-6 text-gray-900">Payment Information</h3>
                <div className="mt-5">
                  <div className="border border-gray-200 rounded-md">
                    <div className="bg-gray-50 px-4 py-5 sm:px-6">
                      <h4 className="text-md font-medium text-gray-900">Payment Methods</h4>
                      <p className="mt-1 text-sm text-gray-500">
                        We accept various payment methods for your convenience.
                      </p>
                    </div>
                    <div className="px-4 py-5 sm:p-6">
                      <ul className="space-y-4">
                        <li className="flex items-start">
                          <div className="flex-shrink-0">
                            <div className="flex items-center justify-center h-10 w-10 rounded-md bg-indigo-500 text-white">
                              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
                              </svg>
                            </div>
                          </div>
                          <div className="ml-4">
                            <h5 className="text-base font-medium text-gray-900">Credit Card</h5>
                            <p className="mt-1 text-sm text-gray-500">
                              Pay with Visa, Mastercard, or American Express.
                            </p>
                          </div>
                        </li>
                        <li className="flex items-start">
                          <div className="flex-shrink-0">
                            <div className="flex items-center justify-center h-10 w-10 rounded-md bg-indigo-500 text-white">
                              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 9V7a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2m2 4h10a2 2 0 002-2v-6a2 2 0 00-2-2H9a2 2 0 00-2 2v6a2 2 0 002 2zm7-5a2 2 0 11-4 0 2 2 0 014 0z" />
                              </svg>
                            </div>
                          </div>
                          <div className="ml-4">
                            <h5 className="text-base font-medium text-gray-900">Bank Transfer</h5>
                            <p className="mt-1 text-sm text-gray-500">
                              Direct bank transfer for secure payments.
                            </p>
                          </div>
                        </li>
                        <li className="flex items-start">
                          <div className="flex-shrink-0">
                            <div className="flex items-center justify-center h-10 w-10 rounded-md bg-indigo-500 text-white">
                              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                              </svg>
                            </div>
                          </div>
                          <div className="ml-4">
                            <h5 className="text-base font-medium text-gray-900">PayPal</h5>
                            <p className="mt-1 text-sm text-gray-500">
                              Quick and secure payments with PayPal.
                            </p>
                          </div>
                        </li>
                      </ul>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <ToastContainer position="top-right" autoClose={5000} />
    </Layout>
  );
};

export default PaymentProcessing;