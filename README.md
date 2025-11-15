# MultiTenantBilling

MultiTenantBilling is a comprehensive multi-tenant billing system built with a modern technology stack. The application provides subscription management, invoicing, payment processing, and usage tracking capabilities for SaaS businesses with multiple tenants.

## Table of Contents

- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Key Models and Entities](#key-models-and-entities)
- [API Endpoints](#api-endpoints)
- [Installation and Setup](#installation-and-setup)
- [Configuration](#configuration)
- [Development](#development)
- [Testing](#testing)
- [Deployment](#deployment)

## Technology Stack

### Backend (.NET)
- **Framework**: .NET 9
- **Language**: C# 10
- **Architecture**: Clean Architecture with CQRS pattern
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Caching**: Redis
- **Authentication**: JWT Bearer Tokens
- **Background Jobs**: Hangfire with PostgreSQL storage
- **API Documentation**: Swagger/OpenAPI
- **Messaging**: MediatR
- **Testing**: xUnit, Moq

### Frontend (React)
- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite
- **Styling**: Tailwind CSS
- **State Management**: Redux Toolkit
- **Routing**: React Router
- **HTTP Client**: Axios
- **UI Components**: Custom components with Tailwind CSS
- **Notifications**: React Toastify

## Architecture

The application follows Clean Architecture principles with a clear separation of concerns:

```
┌─────────────────┐    ┌──────────────────┐    ┌──────────────────┐
│   Presentation  │    │    Application   │    │     Domain       │
│   (API Layer)   │───▶│   (Use Cases)    │───▶│   (Entities)     │
└─────────────────┘    └──────────────────┘    └──────────────────┘
                              │                        │
                              ▼                        ▼
                       ┌──────────────────┐    ┌──────────────────┐
                       │  Infrastructure  │    │     Shared       │
                       │   (Data Access)  │    │   (Common)       │
                       └──────────────────┘    └──────────────────┘
```

### Key Architectural Patterns

1. **Clean Architecture**: Separation of concerns with domain-centric design
2. **CQRS (Command Query Responsibility Segregation)**: Separate models for reading and writing operations
3. **Mediator Pattern**: Using MediatR for handling requests and notifications
4. **Repository Pattern**: Abstracting data access through repository interfaces
5. **Decorator Pattern**: Caching implementation for services
6. **Background Jobs**: Hangfire for recurring tasks and delayed processing
7. **Multi-Tenancy**: Tenant isolation at the database level with context-based filtering

## Project Structure

### Backend Structure

```
MultiTenantBilling/
├── src/
│   ├── MultiTenantBilling.Api/              # Presentation Layer (Web API)
│   │   ├── Controllers/                     # API Controllers
│   │   ├── Middleware/                      # Custom middleware
│   │   ├── Services/                        # API-specific services
│   │   ├── Program.cs                       # Application entry point
│   │   └── appsettings.json                 # Configuration
│   ├── MultiTenantBilling.Application/      # Application Layer
│   │   ├── Commands/                        # CQRS Commands
│   │   ├── Queries/                         # CQRS Queries
│   │   ├── Handlers/                        # Command/Query Handlers
│   │   ├── DTOs/                            # Data Transfer Objects
│   │   ├── Services/                        # Application services
│   │   └── EventHandlers/                   # Domain event handlers
│   ├── MultiTenantBilling.Domain/           # Domain Layer
│   │   ├── Entities/                        # Domain entities
│   │   ├── Interfaces/                      # Domain interfaces
│   │   └── Common/                          # Base classes
│   ├── MultiTenantBilling.Infrastructure/   # Infrastructure Layer
│   │   ├── Data/                            # Database context
│   │   ├── Repositories/                    # Repository implementations
│   │   └── Services/                        # Infrastructure services
│   ├── MultiTenantBilling.AdminTool/        # Admin CLI tool
│   ├── MultiTenantBilling.Worker/           # Background worker service
│   └── tests/                               # Test projects
├── API_ENDPOINTS.md                         # API documentation
└── MultiTenantBilling.sln                   # Solution file
```

### Frontend Structure

```
client/
├── src/
│   ├── components/                          # Reusable UI components
│   ├── pages/                               # Page components
│   ├── services/                            # API service classes
│   ├── store/                               # Redux store configuration
│   ├── hooks/                               # Custom React hooks
│   ├── features/                            # Redux slices organized by feature
│   ├── utils/                               # Utility functions
│   └── index.css                            # Global styles
├── public/                                  # Static assets
├── tailwind.config.js                       # Tailwind CSS configuration
├── vite.config.ts                           # Vite build configuration
└── package.json                             # Dependencies and scripts
```

## Key Models and Entities

### Core Entities

1. **Tenant**: Represents a customer organization
   - Unique identifier and subdomain
   - Name and contact information

2. **User**: Represents a user within a tenant
   - Authentication information (email, password hash)
   - Personal details (first name, last name)
   - Role assignments

3. **Subscription**: Represents a tenant's subscription to a plan
   - Start and end dates
   - Status (Active, Paused, Canceled)
   - Associated plan and tenant

4. **Plan**: Represents a billing plan with pricing and features
   - Name and description
   - Monthly price
   - Resource limits (users, storage)

5. **Invoice**: Represents a billing invoice
   - Amount and due date
   - Payment status
   - Associated subscription

6. **Payment**: Represents a payment transaction
   - Payment method and amount
   - Status and transaction ID
   - Associated invoice

7. **UsageRecord**: Tracks resource usage for billing
   - Metric name and quantity
   - Timestamp
   - Associated subscription

### Relationships

- Tenants have many Users
- Users can have many Roles through UserRole
- Tenants have many Subscriptions
- Subscriptions belong to one Plan
- Subscriptions have many Invoices
- Invoices have many Payments
- Subscriptions have many UsageRecords

## API Endpoints

The API is organized into several functional areas:

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Authenticate a user
- `POST /api/auth/change-password` - Change user password

### User Management
- `GET /api/user/me` - Get current user information
- `GET /api/user/permissions` - Get user permissions
- `GET /api/user/roles` - Get user roles

### Billing
- `POST /api/billing/subscriptions` - Create a subscription
- `GET /api/billing/subscriptions` - Get all subscriptions
- `POST /api/billing/subscriptions/{id}/usage` - Record usage
- `POST /api/billing/subscriptions/{id}/invoices` - Generate invoice
- `POST /api/billing/invoices/{id}/payments` - Process payment
- `GET /api/billing/plans` - Get available plans
- `GET /api/billing/invoices` - Get all invoices
- `GET /api/billing/payments` - Get all payments

### Admin
- `GET /api/admin/tenants` - Get all tenants
- `POST /api/admin/tenants` - Create a tenant
- `GET /api/admin/users` - Get all users
- `GET /api/admin/roles` - Get all roles
- `POST /api/admin/roles` - Create a role
- `GET /api/admin/plans` - Get all plans
- `POST /api/admin/plans` - Create a plan

### Redis Test
- Various endpoints for testing Redis caching functionality

For detailed API documentation with examples, see [API_ENDPOINTS.md](MultiTenantBilling/src/API_ENDPOINTS.md).

## Installation and Setup

### Prerequisites

1. **.NET 9 SDK**
2. **Node.js 18+**
3. **PostgreSQL 13+**
4. **Redis 6+**

### Backend Setup

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd MultiTenantBilling
   ```

2. Navigate to the backend directory:
   ```bash
   cd MultiTenantBilling
   ```

3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

4. Update the database connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=multitenant;Username=your_username;Password=your_password"
     }
   }
   ```

5. Run database migrations:
   ```bash
   dotnet ef database update --project src/MultiTenantBilling.Infrastructure
   ```

6. Run the application:
   ```bash
   dotnet run --project src/MultiTenantBilling.Api
   ```

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd client
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm run dev
   ```

### Database Setup

1. Create a PostgreSQL database:
   ```sql
   CREATE DATABASE multitenant;
   ```

2. The application will automatically create tables on first run.

3. To initialize the database with seed data, run the AdminTool:
   ```bash
   cd MultiTenantBilling.AdminTool
   dotnet run
   ```

## Configuration

### Backend Configuration

The backend application is configured through `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=multitenant;Username=postgres;Password=your_password"
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "MultiTenantBilling_"
  },
  "JwtSettings": {
    "Secret": "YOUR_VERY_SECURE_JWT_SECRET_KEY",
    "Issuer": "MultiTenantBilling",
    "Audience": "MultiTenantBillingUsers",
    "ExpiryInHours": 1
  }
}
```

### Frontend Configuration

The frontend is configured to proxy API requests to the backend server. The proxy configuration is in `vite.config.ts`:

```typescript
export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5168',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})
```

## Development

### Backend Development

1. **Code Organization**: Follow the Clean Architecture principles
2. **CQRS**: Use separate commands/queries and handlers
3. **Dependency Injection**: Register services in `Program.cs`
4. **Entity Framework**: Use migrations for database changes
5. **Logging**: Use built-in .NET logging infrastructure

### Frontend Development

1. **Component Structure**: Organize components by feature
2. **State Management**: Use Redux Toolkit for global state
3. **API Integration**: Centralize API calls in service classes
4. **Styling**: Use Tailwind CSS utility classes
5. **Routing**: Define routes in `App.tsx`

### Multi-Tenancy

The application implements multi-tenancy through:

1. **Tenant Identification**: Using `X-Tenant-ID` header
2. **Data Isolation**: Filtering data by tenant ID in repositories
3. **Context Propagation**: Passing tenant context through services
4. **JWT Integration**: Embedding tenant ID in authentication tokens

## Testing

### Backend Testing

The backend includes unit tests and integration tests:

1. **Unit Tests**: Test individual components in isolation
2. **Integration Tests**: Test API endpoints and database interactions
3. **Test Framework**: xUnit with Moq for mocking

Run tests with:
```bash
dotnet test
```

### Frontend Testing

The frontend uses Jest for testing:

1. **Unit Tests**: Test individual components and functions
2. **Integration Tests**: Test component interactions
3. **Test Utilities**: React Testing Library for component testing

Run tests with:
```bash
npm test
```

## Deployment

### Backend Deployment

1. **Publish the application**:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Deploy to server**:
   - Copy the published files to your server
   - Configure the web server (IIS, Nginx, etc.)
   - Set environment variables for production configuration

3. **Database Migration**:
   ```bash
   dotnet ef database update --project src/MultiTenantBilling.Infrastructure
   ```

### Frontend Deployment

1. **Build for production**:
   ```bash
   npm run build
   ```

2. **Deploy static files**:
   - Copy the `dist` folder contents to your web server
   - Configure your web server to serve static files

### Environment Variables

Set the following environment variables in production:

```bash
# Database connection
ConnectionStrings__DefaultConnection=your_production_connection_string

# Redis connection
Redis__ConnectionString=your_redis_connection_string

# JWT settings
JwtSettings__Secret=your_production_jwt_secret
JwtSettings__Issuer=your_issuer
JwtSettings__Audience=your_audience
```

## Additional Features

### Background Jobs

The application uses Hangfire for background processing:

1. **Invoice Generation**: Monthly invoice generation for active subscriptions
2. **Payment Retry**: Retry failed payments
3. **Usage Aggregation**: Aggregate usage data
4. **Dunning Process**: Handle overdue invoices

### Caching

Redis is used for caching to improve performance:

1. **Plan Caching**: Cache frequently accessed plans
2. **Subscription Caching**: Cache active subscriptions
3. **User Caching**: Cache user permissions and roles

### Security

1. **Authentication**: JWT-based authentication with role-based access control
2. **Authorization**: Policy-based authorization for API endpoints
3. **Data Protection**: Password hashing with BCrypt
4. **Input Validation**: Model validation and sanitization

### Monitoring

1. **Logging**: Structured logging with different log levels
2. **Health Checks**: Built-in health check endpoints
3. **Metrics**: Performance monitoring through application insights