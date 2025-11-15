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
import ProtectedRoute from './components/ProtectedRoute';
import authService from './services/authService';

// Initialize tenant ID if user is authenticated
const initializeApp = () => {
  const token = authService.getAuthToken();
  const existingTenantId = authService.getTenantId();
  
  if (token && !existingTenantId) {
    // Extract tenant ID from JWT token
    try {
      const tokenPayload = JSON.parse(atob(token.split('.')[1]));
      const tenantId = tokenPayload.tenantId;
      if (tenantId) {
        authService.setTenantId(tenantId);
      } else {
        // Fallback to default tenant ID if not found in token
        authService.setTenantId('11111111-1111-1111-1111-111111111111');
      }
    } catch (error) {
      console.error('Error extracting tenant ID from token:', error);
      // Fallback to default tenant ID
      authService.setTenantId('11111111-1111-1111-1111-111111111111');
    }
  }
};

initializeApp();

function App() {
  return (
    <Provider store={store}>
      <Router>
        <div className="min-h-screen bg-gray-50">
          <Routes>
            <Route path="/" element={<ProtectedRoute><Dashboard /></ProtectedRoute>} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/subscriptions" element={<ProtectedRoute><SubscriptionManagement /></ProtectedRoute>} />
            <Route path="/invoices" element={<ProtectedRoute><InvoiceHistory /></ProtectedRoute>} />
            <Route path="/payments" element={<ProtectedRoute><PaymentProcessing /></ProtectedRoute>} />
            <Route path="/admin" element={<ProtectedRoute><AdminDashboard /></ProtectedRoute>} />
          </Routes>
        </div>
      </Router>
    </Provider>
  );
}

export default App;