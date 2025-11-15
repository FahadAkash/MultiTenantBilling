import Layout from '../components/Layout';

const Dashboard = () => {
  return (
    <Layout>
      <div className="px-4 py-6 sm:px-0">
        <div className="border-4 border-dashed border-gray-200 rounded-lg h-96 p-4">
          <h1 className="text-2xl font-bold text-gray-900 mb-4">Tenant Dashboard</h1>
          <p className="text-gray-600">
            Welcome to your MultiTenantBilling dashboard. Here you can manage your subscriptions, 
            view invoices, and process payments.
          </p>
          <div className="mt-8 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            <div className="bg-white overflow-hidden shadow rounded-lg">
              <div className="px-4 py-5 sm:p-6">
                <h3 className="text-lg font-medium text-gray-900">Subscriptions</h3>
                <p className="mt-1 text-sm text-gray-500">
                  Manage your active subscriptions and billing plans.
                </p>
                <div className="mt-4">
                  <a 
                    href="/subscriptions" 
                    className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                  >
                    View Subscriptions
                  </a>
                </div>
              </div>
            </div>
            <div className="bg-white overflow-hidden shadow rounded-lg">
              <div className="px-4 py-5 sm:p-6">
                <h3 className="text-lg font-medium text-gray-900">Invoices</h3>
                <p className="mt-1 text-sm text-gray-500">
                  View and download your billing invoices.
                </p>
                <div className="mt-4">
                  <a 
                    href="/invoices" 
                    className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                  >
                    View Invoices
                  </a>
                </div>
              </div>
            </div>
            <div className="bg-white overflow-hidden shadow rounded-lg">
              <div className="px-4 py-5 sm:p-6">
                <h3 className="text-lg font-medium text-gray-900">Payments</h3>
                <p className="mt-1 text-sm text-gray-500">
                  Process payments and view payment history.
                </p>
                <div className="mt-4">
                  <a 
                    href="/payments" 
                    className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                  >
                    Process Payments
                  </a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default Dashboard;