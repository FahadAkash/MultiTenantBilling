# Multi-Tenant Billing API Endpoints

## Base URL
```
http://localhost:5168
```

## Headers
Most endpoints require the following headers:
```
Content-Type: application/json
X-Tenant-ID: 11111111-1111-1111-1111-111111111111
```

For authenticated endpoints, you also need:
```
Authorization: Bearer {JWT_TOKEN}
```

## Authentication Endpoints

### Register a new user
```bash
curl -X POST "http://localhost:5168/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

### Login
```bash
curl -X POST "http://localhost:5168/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!"
  }'
```

### Change Password
```bash
curl -X POST "http://localhost:5168/api/auth/change-password" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "currentPassword": "SecurePassword123!",
    "newPassword": "NewSecurePassword456!"
  }'
```

## User Endpoints

### Get Current User
```bash
curl -X GET "http://localhost:5168/api/user/me" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111"
```

### Get User Permissions
```bash
curl -X GET "http://localhost:5168/api/user/permissions" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111"
```

### Get User Roles
```bash
curl -X GET "http://localhost:5168/api/user/roles" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111"
```

### Assign Role to User
```bash
curl -X POST "http://localhost:5168/api/user/assign-role" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "userEmail": "user@example.com",
    "roleName": "Admin"
  }'
```

## Billing Endpoints

### Create Subscription
```bash
curl -X POST "http://localhost:5168/api/billing/subscriptions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "planId": "00000000-0000-0000-0000-000000000000",
    "paymentMethodId": "pm_123456"
  }'
```

### Record Usage
```bash
curl -X POST "http://localhost:5168/api/billing/subscriptions/{subscriptionId}/usage" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "metricName": "api_calls",
    "quantity": 100
  }'
```

### Generate Invoice
```bash
curl -X POST "http://localhost:5168/api/billing/subscriptions/{subscriptionId}/invoices" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111"
```

### Process Payment
```bash
curl -X POST "http://localhost:5168/api/billing/invoices/{invoiceId}/payments" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "paymentMethodId": "pm_123456"
  }'
```

### Get All Plans
```bash
curl -X GET "http://localhost:5168/api/billing/plans" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111"
```

## Redis Test Endpoints

### Create Plan (Redis Test)
```bash
curl -X POST "http://localhost:5168/api/redistest/plans" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "name": "Premium Plan",
    "description": "Premium plan with all features",
    "monthlyPrice": 99.99,
    "maxUsers": 100,
    "maxStorageGb": 1000,
    "isActive": true
  }'
```

### Get All Plans (Redis Test)
```bash
curl -X GET "http://localhost:5168/api/redistest/plans" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111"
```

### Get Specific Plan (Redis Test)
```bash
curl -X GET "http://localhost:5168/api/redistest/plans/{planId}" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111"
```

### Set Cache Value
```bash
curl -X POST "http://localhost:5168/api/redistest/cache/test" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "key": "testkey",
    "value": "testvalue"
  }'
```

### Get Cached Value
```bash
curl -X GET "http://localhost:5168/api/redistest/cache/test/testkey" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111"
```

## Admin Endpoints

### Get All Tenants
```bash
curl -X GET "http://localhost:5168/api/admin/tenants" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111"
```

### Create Tenant
```bash
curl -X POST "http://localhost:5168/api/admin/tenants" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "name": "New Tenant",
    "subdomain": "newtenant",
    "email": "admin@newtenant.com"
  }'
```

### Get All Users
```bash
curl -X GET "http://localhost:5168/api/admin/users" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111"
```

### Get All Roles
```bash
curl -X GET "http://localhost:5168/api/admin/roles" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111"
```

### Create Role
```bash
curl -X POST "http://localhost:5168/api/admin/roles" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "name": "NewRole",
    "description": "Description of the new role"
  }'
```

### Get All Plans (Admin)
```bash
curl -X GET "http://localhost:5168/api/admin/plans" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111"
```

### Create Plan (Admin)
```bash
curl -X POST "http://localhost:5168/api/admin/plans" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -H "X-Tenant-ID: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "name": "Enterprise Plan",
    "description": "Enterprise plan with all features",
    "monthlyPrice": 299.99,
    "maxUsers": 1000,
    "maxStorageGb": 10000,
    "isActive": true
  }'
```

## Weather Forecast Endpoint

### Get Weather Forecast
```bash
curl -X GET "http://localhost:5168/weatherforecast"
```