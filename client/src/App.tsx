import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Provider } from 'react-redux';
import { store } from './store';
import Dashboard from './pages/Dashboard';
import Login from './pages/Login';
import Register from './pages/Register';
import SubscriptionManagement from './pages/SubscriptionManagement';
import InvoiceHistory from './pages/InvoiceHistory';
import PaymentProcessing from './pages/PaymentProcessing';
import AdminDashboard from './pages/AdminDashboard';

function App() {
  return (
    <Provider store={store}>
      <Router>
        <div className="min-h-screen bg-gray-50">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/subscriptions" element={<SubscriptionManagement />} />
            <Route path="/invoices" element={<InvoiceHistory />} />
            <Route path="/payments" element={<PaymentProcessing />} />
            <Route path="/admin" element={<AdminDashboard />} />
          </Routes>
        </div>
      </Router>
    </Provider>
  );
}

export default App;