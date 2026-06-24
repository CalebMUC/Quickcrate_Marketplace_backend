# Unified Authentication System - Minimart API

## Overview

The Minimart API now uses a **unified authentication system** centered around the `IdentityController`. This consolidates all authentication functionality that was previously scattered across multiple controllers.

## Migration Guide

### From AuthenticationController to IdentityController

All authentication endpoints have been moved from `api/authentication` to `api/identity`. The old `AuthenticationController` is now **DEPRECATED** and will return error messages directing you to the new endpoints.

### Endpoint Mapping

| Old Endpoint | New Endpoint | Description |
|--------------|--------------|-------------|
| `POST /api/authentication/Register` | `POST /api/identity/register` | User registration |
| `POST /api/authentication/Login` | `POST /api/identity/login` | Basic user login |
| `POST /api/authentication/login` | `POST /api/identity/merchant/login` | Enhanced merchant login |
| `POST /api/authentication/SendResetCode` | `POST /api/identity/send-password-reset` | Send password reset code |
| `POST /api/authentication/VerifyResetCode` | `POST /api/identity/verify-reset-code` | Verify reset code |
| `POST /api/authentication/ResetPassword` | `POST /api/identity/reset-password` | Reset password |
| `POST /api/authentication/SendEmailVerificationCode` | `POST /api/identity/send-email-verification` | Send email verification |
| `POST /api/authentication/VerifyEmailValidationCode` | `POST /api/identity/verify-email` | Verify email |
| `POST /api/authentication/refresh` | `POST /api/identity/refresh-token` | Refresh tokens |
| `POST /api/authentication/logout` | `POST /api/identity/logout` | Logout user |
| `GET /api/authentication/me` | `GET /api/identity/profile` | Get user profile |
| `PUT /api/identity/update-profile` | `PUT /api/identity/profile` | Update user profile |

## New Features

### 1. Enhanced Merchant Authentication
- **Enhanced Login**: `POST /api/identity/merchant/login` - Includes IP tracking and user agent logging
- **Account Locking**: Automatic account locking after failed login attempts
- **Login Attempt Tracking**: Full audit trail of login attempts

### 2. Improved Password Management
- **Change Password**: `POST /api/identity/change-password` - For users with temporary passwords
- **Update Password**: `POST /api/identity/update-password` - Standard password change

### 3. User Profile Management
- **Get Profile**: `GET /api/identity/profile` or `GET /api/identity/me`
- **Update Profile**: `PUT /api/identity/profile`

### 4. Enhanced Security
- **JWT Token Management**: Proper token refresh and revocation
- **Role-based Authorization**: Admin, Merchant, and User roles
- **Account Locking**: Protection against brute force attacks

## API Routes

### Public Endpoints (No Authentication Required)
```
POST /api/identity/register                 - Register new user
POST /api/identity/login                    - Basic login
POST /api/identity/merchant/login           - Enhanced merchant login
POST /api/identity/send-password-reset      - Send password reset code
POST /api/identity/verify-reset-code        - Verify reset code
POST /api/identity/reset-password           - Reset password
POST /api/identity/send-email-verification  - Send email verification
POST /api/identity/verify-email             - Verify email
POST /api/identity/refresh-token            - Refresh access token
```

### Protected Endpoints (Authentication Required)
```
GET  /api/identity/profile                  - Get user profile
GET  /api/identity/me                       - Get user profile (alternative)
PUT  /api/identity/profile                  - Update user profile  
POST /api/identity/logout                   - Logout user
POST /api/identity/change-password          - Change password (from temporary)
POST /api/identity/update-password          - Update password (standard)
```

### Admin-Only Endpoints
```
GET  /api/identity/users                    - Get all users (paginated)
```

### Role Test Endpoints
```
GET  /api/identity/admin-only               - Test admin role
GET  /api/identity/merchant-only            - Test merchant role
GET  /api/identity/user-only                - Test user role
```

## Authentication Flow

### 1. User Registration
```json
POST /api/identity/register
{
  "userName": "john_doe",
  "email": "john@example.com",
  "phoneNumber": "+1234567890",
  "password": "SecurePass123",
  "reEnteredPassword": "SecurePass123",
  "role": "User"
}
```

### 2. User Login
```json
POST /api/identity/login
{
  "emailorPhone": "john@example.com",
  "password": "SecurePass123"
}
```

### 3. Enhanced Merchant Login
```json
POST /api/identity/merchant/login
{
  "email": "merchant@example.com",
  "password": "SecurePass123",
  "rememberMe": true
}
```

### 4. Password Reset Flow
```json
// Step 1: Send reset code
POST /api/identity/send-password-reset
{
  "email": "john@example.com"
}

// Step 2: Verify reset code
POST /api/identity/verify-reset-code
{
  "email": "john@example.com",
  "code": "123456"
}

// Step 3: Reset password
POST /api/identity/reset-password
{
  "email": "john@example.com",
  "newPassword": "NewSecurePass123"
}
```

## Authorization

### JWT Token
All protected endpoints require a JWT token in the Authorization header:
```
Authorization: Bearer <jwt-token>
```

### Roles
- **Admin**: Full system access
- **Merchant**: Business management access
- **User**: Standard user access

### Policies
- `AdminOnly`: Requires Admin role
- `MerchantOnly`: Requires Merchant role
- `UserOnly`: Requires User role
- `AdminOrMerchant`: Requires Admin or Merchant role

## Error Responses

### Standard Error Format
```json
{
  "responseCode": 400,
  "responseMessage": "Error description",
  "errors": ["Error detail 1", "Error detail 2"]
}
```

### Enhanced Error Format (Merchant System)
```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Error detail 1", "Error detail 2"],
  "data": null
}
```

## Backward Compatibility

### Legacy Support
The `IdentityController` maintains backward compatibility by exposing legacy endpoint names:
- `POST /api/identity/Register` (capital R)
- `POST /api/identity/Login` (capital L)
- etc.

### Deprecation Warnings
The old `AuthenticationController` now returns deprecation warnings with migration instructions for all endpoints.

## Implementation Details

### Services Used
- `IIdentityService`: Core identity operations
- `IAuthentication`: Enhanced authentication features
- `UserManager<ApplicationUser>`: ASP.NET Core Identity management

### Database Integration
- Uses ASP.NET Core Identity tables
- Supports both legacy user IDs and new Identity IDs
- Merchant ID association for business users

### Security Features
- Password strength validation
- Account lockout protection
- Login attempt tracking
- IP and user agent logging
- JWT token management with refresh tokens

## Migration Timeline

1. **Phase 1 (Current)**: Both controllers active, old one deprecated
2. **Phase 2**: Remove deprecated controller after client migration
3. **Phase 3**: Clean up legacy support code

## Support

For issues or questions about the unified authentication system:
1. Check this documentation
2. Review error messages for migration instructions
3. Contact the development team

---

**Note**: This unified system provides better security, enhanced features, and a cleaner API surface while maintaining backward compatibility during the transition period.