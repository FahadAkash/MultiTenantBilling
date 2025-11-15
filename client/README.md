# MultiTenantBilling Frontend

This is the React frontend application for the MultiTenantBilling system.

## Technologies Used

- React with TypeScript
- Vite as the build tool
- Tailwind CSS for styling
- Redux Toolkit for state management
- React Router for routing
- Axios for HTTP requests

## Project Structure

```
client/
├── src/
│   ├── components/     # Reusable UI components
│   ├── pages/          # Page components
│   ├── services/       # API service classes
│   ├── store/          # Redux store configuration
│   ├── hooks/          # Custom React hooks
│   ├── features/       # Redux slices organized by feature
│   └── utils/          # Utility functions
├── public/             # Static assets
└── ...
```

## Getting Started

1. Install dependencies:
   ```bash
   npm install
   ```

2. Start the development server:
   ```bash
   npm run dev
   ```

3. Build for production:
   ```bash
   npm run build
   ```

## Development

The frontend is configured to proxy API requests to the backend server running on `http://localhost:5168`. This allows the frontend to communicate with the backend during development without CORS issues.

## Testing

To run tests, you can use the following command:

```bash
npm test
```

Note: You may need to install additional testing dependencies like Jest and React Testing Library for full testing capabilities.

## Features

- User authentication (login/register)
- Tenant dashboard
- Subscription management
- Invoice history
- Payment processing
- Responsive design with Tailwind CSS

## State Management

The application uses Redux Toolkit for state management, organized into slices:
- `auth` - Authentication state
- `tenant` - Tenant information
- `billing` - Billing-related data (plans, subscriptions, invoices, payments)

## API Integration

API services are located in the `src/services` directory and handle communication with the backend API endpoints. All API calls include the required `X-Tenant-ID` header for multi-tenancy support.