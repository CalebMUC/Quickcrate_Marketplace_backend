# Complete Identity System Setup Guide

## ?? Current Status
? **Code Updated**: All ApplicationUser properties restored and IdentityController enhanced  
? **Build Status**: Successful compilation  
?? **Database**: Needs column updates (scripts provided)

## ?? Next Steps to Complete Setup

### Step 1: Update Database Schema
**Run the database scripts I created earlier:**

```sql
-- Option 1: Run Master Script (Recommended)
\i Database/Scripts/00_Master_Update_Script.sql

-- Option 2: Run individual scripts in order
\i Database/Scripts/01_Add_AspNetUsers_CustomColumns.sql
\i Database/Scripts/02_Add_AspNetRoles_CreatedAt.sql
\i Database/Scripts/03_Insert_Default_Roles.sql
\i Database/Scripts/04_Fix_ProductId_ForeignKeys.sql
```

### Step 2: Verify Database Changes
After running the scripts, verify with:

```sql
-- Check AspNetUsers columns
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'AspNetUsers' AND table_schema = 'public'
ORDER BY ordinal_position;

-- Check AspNetRoles columns  
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_name = 'AspNetRoles' AND table_schema = 'public'
ORDER BY ordinal_position;

-- Verify roles exist
SELECT "Id", "Name", "NormalizedName", "Description" 
FROM "AspNetRoles" ORDER BY "Id";
```

### Step 3: Test the Application
Once database is updated, test these endpoints:

## ?? Available API Endpoints

### **Authentication Endpoints**
```http
POST /api/v2/Identity/register
POST /api/v2/Identity/login
POST /api/v2/Identity/logout
```

### **Email Verification**
```http
POST /api/v2/Identity/send-email-verification
POST /api/v2/Identity/verify-email
```

### **Password Reset**
```http
POST /api/v2/Identity/send-password-reset
POST /api/v2/Identity/verify-reset-code
POST /api/v2/Identity/reset-password
```

### **Profile Management**
```http
GET /api/v2/Identity/profile
PUT /api/v2/Identity/update-profile
```

### **Admin Endpoints**
```http
GET /api/v2/Identity/users (Admin only)
GET /api/v2/Identity/admin-only
GET /api/v2/Identity/merchant-only
GET /api/v2/Identity/user-only
```

## ?? Enhanced Features Added

### **1. Complete User Profile**
The `/profile` endpoint now returns:
- Basic info (Id, UserName, Email, Phone)
- Display names (DisplayName, FirstName, LastName)
- Status info (IsLoggedIn, EmailConfirmed, IsEmailVerified)
- Timestamps (CreatedAt, LastLogin)
- Legacy support (LegacyUserId)
- Role information (Role, Roles array)

### **2. Profile Update Endpoint**
```json
PUT /api/v2/Identity/update-profile
{
  "firstName": "John",
  "lastName": "Doe", 
  "displayName": "JohnD",
  "phoneNumber": "+1234567890"
}
```

### **3. Enhanced Registration**
Now properly sets all custom properties:
- DisplayName from UserName
- CreatedAt timestamp
- IsEmailVerified status
- FailedAttempts counter
- IsLoggedIn status

### **4. Enhanced Login**
Updates user status on login:
- Sets IsLoggedIn = true
- Updates LastLogin timestamp
- Resets FailedAttempts counter
- Returns comprehensive user info

### **5. Admin User Management**
```http
GET /api/v2/Identity/users?page=1&pageSize=10
```
Returns paginated user list with full details.

### **6. Proper Logout**
Updates IsLoggedIn status when user logs out.

## ?? Security Features

- **JWT Token Authentication**
- **Role-based Authorization** (Admin, Merchant, User)
- **Email Verification** with Redis-based code storage
- **Password Reset** with time-limited codes
- **Rate Limiting** for verification attempts
- **Proper Error Handling** and logging

## ??? Configuration

### **Required Services** (Already configured in Program.cs):
- ASP.NET Core Identity
- JWT Authentication
- Redis for code storage
- Email service (Brevo)
- PostgreSQL database

### **Required Policies** (Already configured):
- AdminOnly
- MerchantOnly  
- UserOnly
- AdminOrMerchant

## ? Quick Test Commands

### **Register a New User:**
```json
POST /api/v2/Identity/register
{
  "userName": "JohnDoe",
  "email": "john@example.com",
  "phoneNumber": "+1234567890",
  "password": "SecurePass123!",
  "reEnteredpassword": "SecurePass123!"
}
```

### **Login:**
```json
POST /api/v2/Identity/login
{
  "emailorPhone": "john@example.com",
  "password": "SecurePass123!"
}
```

### **Get Profile (with Bearer token):**
```http
GET /api/v2/Identity/profile
Authorization: Bearer {your-jwt-token}
```

## ?? What's Fixed

? **"DisplayName does not exist" error** - All custom properties restored  
? **"CreatedAt does not exist" error** - ApplicationRole updated  
? **Foreign key type mismatches** - Scripts fix ProductId columns  
? **Missing role seeding** - Default roles will be created  
? **Incomplete user management** - Full CRUD operations available  
? **Basic profile endpoints** - Enhanced with comprehensive info  

After running the database scripts, your Identity system will be fully functional with all the enhanced features! ??