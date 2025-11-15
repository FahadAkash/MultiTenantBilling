import Layout from '../components/Layout';
import { useEffect, useState } from 'react';
import { useAppSelector, useAppDispatch } from '../hooks/useAppSelector';
import type { Invoice, Payment } from '../features/billing/billingSlice';
import type { RootState } from '../store';
import type { BillingState } from '../features/billing/billingSlice';
import billingService from '../services/billingService';
import { setInvoices, setPayments, setLoading, setError } from '../features/billing/billingSlice';

const InvoiceHistory = () => {
  const dispatch = useAppDispatch();
  const billing = useAppSelector((state: RootState) => state.billing) as BillingState;
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null);
  const [showInvoiceModal, setShowInvoiceModal] = useState(false);

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
      }
    };

    loadData();
  }, [dispatch]);

  const handleGenerateInvoice = () => {
    // TODO: Implement generate invoice functionality
    console.log('Generate invoice button clicked');
  };

  const handleViewInvoice = async (invoiceId: string) => {
    try {
      const invoice = await billingService.getInvoiceDetails(invoiceId);
      setSelectedInvoice(invoice);
      setShowInvoiceModal(true);
    } catch (error) {
      console.error('Error loading invoice details:', error);
      dispatch(setError('Failed to load invoice details'));
    }
  };

  const handleCloseModal = () => {
    setShowInvoiceModal(false);
    setSelectedInvoice(null);
  };

  const handleProcessPayment = async (invoiceId: string) => {
    try {
      // TODO: Implement payment processing
      console.log('Processing payment for invoice:', invoiceId);
    } catch (error) {
      console.error('Error processing payment:', error);
      dispatch(setError('Failed to process payment'));
    }
  };

  return (
    <Layout>
      <div className="px-4 py-6 sm:px-0">
        <div className="border-4 border-dashed border-gray-200 rounded-lg h-full p-4">
          <div className="flex justify-between items-center mb-6">
            <h1 className="text-2xl font-bold text-gray-900">Invoice History</h1>
            <button 
              onClick={handleGenerateInvoice}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
            >
              Generate Invoice
            </button>
          </div>

          {billing.loading ? (
            <div className="flex justify-center items-center h-64">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
            </div>
          ) : (
            <>
              <div className="mb-8">
                <h2 className="text-lg font-medium text-gray-900 mb-4">Recent Invoices</h2>
                {billing.invoices.length > 0 ? (
                  <div className="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
                    <table className="min-w-full divide-y divide-gray-300">
                      <thead className="bg-gray-50">
                        <tr>
                          <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                            Invoice ID
                          </th>
                          <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                            Date
                          </th>
                          <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                            Amount
                          </th>
                          <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                            Status
                          </th>
                          <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                            Actions
                          </th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-200 bg-white">
                        {billing.invoices.map((invoice: Invoice) => (
                          <tr key={invoice.id}>
                            <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">
                              {invoice.id.substring(0, 8)}
                            </td>
                            <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                              {new Date(invoice.invoiceDate).toLocaleDateString()}
                            </td>
                            <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                              ${invoice.amount.toFixed(2)}
                            </td>
                            <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                              <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                                invoice.isPaid 
                                  ? 'bg-green-100 text-green-800' 
                                  : 'bg-yellow-100 text-yellow-800'
                              }`}>
                                {invoice.isPaid ? 'Paid' : 'Pending'}
                              </span>
                            </td>
                            <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                              <button 
                                onClick={() => handleViewInvoice(invoice.id)}
                                className="text-indigo-600 hover:text-indigo-900 mr-4"
                              >
                                View
                              </button>
                              {!invoice.isPaid && (
                                <button 
                                  onClick={() => handleProcessPayment(invoice.id)}
                                  className="text-green-600 hover:text-green-900"
                                >
                                  Pay
                                </button>
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                ) : (
                  <div className="text-center py-12">
                    <p className="text-gray-500">No invoices found.</p>
                  </div>
                )}
              </div>

              <div>
                <h2 className="text-lg font-medium text-gray-900 mb-4">Payment History</h2>
                {billing.payments.length > 0 ? (
                  <div className="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
                    <table className="min-w-full divide-y divide-gray-300">
                      <thead className="bg-gray-50">
                        <tr>
                          <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                            Payment ID
                          </th>
                          <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                            Date
                          </th>
                          <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                            Amount
                          </th>
                          <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                            Method
                          </th>
                          <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                            Status
                          </th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-200 bg-white">
                        {billing.payments.map((payment: Payment) => (
                          <tr key={payment.id}>
                            <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">
                              {payment.id.substring(0, 8)}
                            </td>
                            <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                              {new Date(payment.paymentDate).toLocaleDateString()}
                            </td>
                            <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                              ${payment.amount.toFixed(2)}
                            </td>
                            <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                              {payment.method}
                            </td>
                            <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                              <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                                payment.status === 'Success' 
                                  ? 'bg-green-100 text-green-800' 
                                  : 'bg-red-100 text-red-800'
                              }`}>
                                {payment.status}
                              </span>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                ) : (
                  <div className="text-center py-12">
                    <p className="text-gray-500">No payments found.</p>
                  </div>
                )}
              </div>
            </>
          )}
        </div>
      </div>

      {/* Invoice Details Modal */}
      {showInvoiceModal && selectedInvoice && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="mt-3">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-medium text-gray-900">Invoice Details</h3>
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
                <div className="border-b border-gray-200 pb-4">
                  <h4 className="text-md font-medium text-gray-900">Invoice #{selectedInvoice.id.substring(0, 8)}</h4>
                  <div className="mt-2 text-sm text-gray-500">
                    <p>Date: {new Date(selectedInvoice.invoiceDate).toLocaleDateString()}</p>
                    <p>Due Date: {new Date(selectedInvoice.dueDate).toLocaleDateString()}</p>
                    <p className="mt-2">Status: <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                      selectedInvoice.isPaid 
                        ? 'bg-green-100 text-green-800' 
                        : 'bg-yellow-100 text-yellow-800'
                    }`}>
                      {selectedInvoice.isPaid ? 'Paid' : 'Pending'}
                    </span></p>
                  </div>
                </div>
                <div className="mt-4">
                  <div className="flex justify-between">
                    <span className="text-gray-600">Amount:</span>
                    <span className="font-medium">${selectedInvoice.amount.toFixed(2)}</span>
                  </div>
                  <div className="mt-4">
                    {!selectedInvoice.isPaid && (
                      <button
                        onClick={() => handleProcessPayment(selectedInvoice.id)}
                        className="w-full inline-flex justify-center px-4 py-2 text-sm font-medium text-white bg-green-600 border border-transparent rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500"
                      >
                        Process Payment
                      </button>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
};

export default InvoiceHistory;