<div align="center">

# Quickcrate Marketplace

**Multi-vendor e-commerce platform built for the Kenyan market**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-316192?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-Upstash-DC382D?style=flat&logo=redis&logoColor=white)](https://upstash.com/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Messaging-FF6600?style=flat&logo=rabbitmq&logoColor=white)](https://www.rabbitmq.com/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat&logo=docker&logoColor=white)](https://www.docker.com/)
[![AWS](https://img.shields.io/badge/AWS-S3-FF9900?style=flat&logo=amazonaws&logoColor=white)](https://aws.amazon.com/s3/)

[Business Context](#-business-context) •
[Architecture](#-architecture) •
[Business Flow](#-business-flow) •
[Tech Stack](#-tech-stack) •
[Quick Start](#-quick-start) •
[API Reference](#-api-reference) •
[Roadmap](#-roadmap)

</div>

---

## 🛒 Business Context

Quickcrate Marketplace is a **multi-vendor e-commerce platform** that allows 
independent merchants across Kenya to sell products online through a single 
unified storefront — similar to Jumia or Kilimall, but built specifically for 
the Kenyan SME market.

### The Problem

Kenyan small and medium businesses face three barriers to e-commerce:

| Problem | Reality | This Platform Solves It With |
|---------|---------|------------------------------|
| **Setup cost** | Building a custom online store costs KES 200k+ | Merchants onboard in minutes via a guided registration flow |
| **Payments** | Card payments exclude the majority of Kenyan buyers | Native M-Pesa STK Push checkout at the product level |
| **Discovery** | Individual merchant websites have no traffic | Unified storefront with SEO-optimised product slug URLs |

### Who Uses This Platform

**Merchants** — small businesses, wholesalers, and retailers who register, list 
products, manage inventory, and receive payouts via M-Pesa.

**Customers** — shoppers who browse the unified catalogue, add to cart, checkout 
via M-Pesa, and track deliveries to their door.

**Admins** — platform operators who approve merchants, manage categories, 
monitor orders, and handle disputes.

---

## 🏗️ Architecture

┌─────────────────────────────────────────────────────────────┐
│ CLIENT TIER │
│ Next.js Web App · React Native Mobile App │
└──────────────────────────┬──────────────────────────────────┘
│ HTTPS / WebSocket (SignalR)
┌──────────────────────────▼──────────────────────────────────┐
│ ASP.NET Core 8 REST API │
│ JWT Auth · Role-based Access · API Versioning │
│ Serilog Logging · AutoMapper · Global Error Handler │
└──┬────┬────┬────┬────┬────┬────┬────┬────┬────┬────────────┘
│ │ │ │ │ │ │ │ │ │
[Auth][Products][Orders][Cart][Merchants][Deliveries][Payments]
[Categories][Reports][Dashboard][Search][Recommendations]
│ │ │ │ │ │ │ │ │ │
└────┴────┴────┴────┴────┴────┴────┴────┴────┘
│
┌─────────────┼─────────────┐
│ │ │
┌──────▼──────┐ ┌────▼────┐ ┌─────▼──────┐
│ PostgreSQL │ │ Redis │ │ RabbitMQ │
│ (Primary) │ │ (Cache) │ │ (Events) │
└─────────────┘ └─────────┘ └────────────┘
│
┌──────▼──────┐
│ AWS S3 │
│ (Images) │
└─────────────┘

### Architecture Pattern — Layered Monolith

This project uses a single-project ASP.NET Core API with clear internal layering:

Quickcrate_Marketplace_backend/
├── Controllers/ # HTTP layer — routes, request binding, responses
├── Services/ # Business logic — all use case implementations
├── Repositories/ # Data access — EF Core queries, DB operations
├── Models/ # Domain entities — EF Core mapped classes
├── DTOS/ # Data transfer objects — request/response shapes
├── Mappings/ # AutoMapper profiles (Entity ↔ DTO)
├── Core/ # Cross-cutting — JWT helpers, shared libraries
├── Configuration/ # App settings binding (JWT, M-Pesa, Brevo, AWS)
├── Exceptions/ # Custom exception types + global handler
├── Data/ # EF Core DbContext (MinimartDBContext)
└── Database/Scripts/ # SQL migration scripts

**Why a layered monolith (not microservices)?** The marketplace needs ACID 
transactions across cart, order, and payment in a single request — splitting 
these into separate services would require distributed transaction management, 
adding significant complexity with no scalability benefit at current scale.

---

## 📦 Business Flow

### Customer Purchase Flow

CUSTOMER PLATFORM MERCHANT
│ │ │
│── Browse catalogue ─────────►│ │
│ (SEO slug URLs) │── Query products + cache ─────►│
│◄── Product listings ─────────│ (Redis, 5-min TTL) │
│ │ │
│── Add to cart ──────────────►│ │
│ POST /api/cart/add │── Validate stock ─────────────►│
│◄── Cart updated ─────────────│ │
│ │ │
│── Checkout ─────────────────►│ │
│ POST /api/orders │── Create order record ─────────►│
│ │── M-Pesa STK Push ─────────────►│ (Customer's phone)
│◄── "Check your phone" ───────│ │
│ │ │
│── Confirm M-Pesa OTP ───────►│ │
│ (on phone) │── Payment callback received ───►│
│ │── Order status → PAID ─────────►│
│ │── [Event] OrderPaid ───────────►│ → Notify merchant
│◄── Order confirmation SMS ───│ (RabbitMQ) │
│ │ │
│◄── Real-time status update ──│◄── Merchant updates status ────│
│ (SignalR ActivityHub) │ PATCH /api/orders/{id} │

### Merchant Onboarding Flow
Register Admin review Account activated
(business (KYC check) M-Pesa payout
details) │ method configured
│ │ │
▼ ▼ ▼
POST /auth/ PATCH /merchants/ List products →
register {id}/approve Receive orders →
(Merchant) Payouts settled

### Payout Flow

Order delivered → Merchant balance credited →
Merchant requests payout → M-Pesa B2C transfer →
Payout record created + confirmation SMS

---

## 🔐 Authentication & Authorization

ASP.NET Core Identity with JWT — three roles with distinct access levels:

ROLE_ADMIN → Full platform access: approve merchants, manage categories,
view all reports, handle disputes, configure platform settings

ROLE_MERCHANT → Own store management: list products, view own orders,
manage inventory, configure payout methods, view own dashboard

ROLE_USER → Customer access: browse, add to cart, place orders,
track deliveries, view own order history

### Auth Flow

Register → Identity creates user, assigns role, sends welcome email (Brevo)
Login → Returns JWT (15-min access) + Refresh token
Refresh → Issues new JWT pair, rotates refresh token
Logout → Refresh token invalidated in DB
Lockout → 5 failed attempts → 5-min account lockout (ASP.NET Identity built-in)

```bash
# Register as merchant
POST /api/auth/register
{
  "email": "jane@kijabifresh.co.ke",
  "password": "Secure123!",
  "displayName": "Jane Wanjiku",
  "role": "Merchant",
  "businessName": "Kijabi Fresh Produce"
}

# Login
POST /api/auth/login
{
  "email": "jane@kijabifresh.co.ke",
  "password": "Secure123!"
}
# → { "accessToken": "eyJ...", "refreshToken": "..." }

# All protected requests
Authorization: Bearer {accessToken}
```

---

## 🛠️ Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Framework** | ASP.NET Core 8 | REST API, middleware pipeline |
| **Auth** | ASP.NET Core Identity + JWT | User management, role-based access |
| **ORM** | Entity Framework Core 8 | Database access, migrations |
| **Database** | PostgreSQL 16 | Primary data store (Npgsql with retry) |
| **Cache** | Redis (Upstash) | Product listings, session data, rate limiting |
| **Messaging** | RabbitMQ | Async: order events, notification dispatch |
| **Real-time** | SignalR (ActivityHub) | Live order status updates to customers |
| **Email** | Brevo (Sendinblue) | Transactional emails (order confirm, receipts) |
| **SMS** | Celcom Africa | OTP, order notifications, delivery updates |
| **Payments** | M-Pesa Daraja (Sandbox + Live) | STK Push checkout, B2C merchant payouts |
| **Storage** | AWS S3 | Product images, merchant documents |
| **Search** | OpenSearch | Product search with relevance ranking |
| **Logging** | Serilog | Structured logs (console + rolling file) |
| **Mapping** | AutoMapper | Entity ↔ DTO conversion |
| **Docs** | Swagger / OpenAPI | Interactive API documentation |
| **Containers** | Docker + Docker Compose | Local development environment |

---

## 🗃️ Database Design

### Core Entities

ApplicationUser (ASP.NET Identity)
│
├── Products → title, slug, price, stock, images, merchant
│ ├── ProductImages → AWS S3 URLs
│ └── Features → product attributes (size, colour, weight)
│
├── Categories → hierarchical (Category → SubCategory)
│
├── Orders → customer orders with lifecycle tracking
│ └── OrderProducts → line items (product snapshot at order time)
│
├── Cart → session cart per customer
│ └── CartItems → products + quantities
│
├── Payments → M-Pesa transaction records + callbacks
│
├── Deliveries → delivery assignments + status tracking
│ └── Addresses → customer delivery addresses
│
├── MerchantPaymentMethods → M-Pesa till/paybill for payouts
│
├── Recommendations → personalised product suggestions per user
│
└── Reports → pre-aggregated merchant analytics

### Key Design Decisions

**SEO slug URLs** — every product and category has a URL-safe slug 
(`/products/samsung-galaxy-a55-midnight-black`) generated on creation, 
enabling SEO-friendly product pages and shareable links.

**Product snapshot on order** — `OrderProducts` stores a copy of the 
product price and name at order time. This means merchant price changes 
don't retroactively alter historical orders — critical for billing integrity.

**Soft deletes** — products and orders are never hard-deleted. 
A `DeletedAt` timestamp preserves audit history.

---

## 🇰🇪 Kenya-Specific Features

### M-Pesa STK Push (Customer Checkout)

```csharp
// Customer initiates checkout → STK Push sent to their phone
var stkResponse = await _mpesaService.InitiateSTKPush(new STKPushRequest
{
    PhoneNumber = customer.PhoneNumber,        // +254712345678
    Amount = order.TotalAmountKES,
    AccountReference = order.OrderNumber,
    TransactionDesc = $"Quickcrate order {order.OrderNumber}"
});

// M-Pesa callback hits our webhook → order status updated
POST /api/mpesa/callback
→ Validate CheckoutRequestID → Mark order PAID → Notify merchant via RabbitMQ
```

### M-Pesa B2C (Merchant Payouts)

```csharp
// Merchant requests payout of their settled balance
await _mpesaService.InitiateB2C(new B2CRequest
{
    PhoneNumber = merchant.MpesaPhone,
    Amount = payoutAmount,
    CommandID = "BusinessPayment",
    Remarks = $"Quickcrate payout - {DateTime.UtcNow:MMM yyyy}"
});
```

### Brevo Transactional Email

```csharp
// Order confirmation email after successful M-Pesa payment
await _brevoEmailService.SendOrderConfirmationAsync(
    toEmail: customer.Email,
    toName: customer.DisplayName,
    orderNumber: order.OrderNumber,
    totalAmount: order.TotalAmountKES
);
```

### Celcom Africa SMS

```csharp
// Delivery status SMS to customer
await _smsService.SendAsync(
    customer.PhoneNumber,
    $"Your Quickcrate order {order.OrderNumber} has been dispatched. " +
    $"Estimated delivery: {delivery.EstimatedDate:dd MMM}."
);
```

---

## 🚀 Quick Start

### Prerequisites

- .NET 8 SDK
- Docker & Docker Compose

### Run locally

```bash
git clone https://github.com/CalebMUC/Quickcrate_Marketplace_backend.git
cd Quickcrate_Marketplace_backend

# Start PostgreSQL + Redis + RabbitMQ
docker compose up -d

# Configure secrets (copy template and fill in values)
cp appsettings.Template.json appsettings.Development.json

# Run user secrets setup script (PowerShell)
./setup-user-secrets.ps1

# Run the API
dotnet run --project .
```

### Access the API

https://localhost:7001/swagger ← Interactive API docs
http://localhost:15672 ← RabbitMQ Management (guest/guest)

### Key configuration (appsettings.Template.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=quickcrate_dev;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "JwtSettings": {
    "Secret": "your-256-bit-secret-minimum-32-chars",
    "Issuer": "quickcrate-api",
    "Audience": "quickcrate-client",
    "ExpiryMinutes": 15
  },
  "MpesaSandBox": {
    "ConsumerKey": "your-daraja-consumer-key",
    "ConsumerSecret": "your-daraja-consumer-secret",
    "ShortCode": "174379",
    "PassKey": "your-passkey",
    "CallbackUrl": "https://your-ngrok-url/api/mpesa/callback"
  },
  "AwsConfig": {
    "AccessKey": "your-aws-access-key",
    "SecretKey": "your-aws-secret-key",
    "BucketName": "quickcrate-uploads",
    "Region": "af-south-1"
  }
}
```

---

## 📡 API Reference

Full interactive docs available at `/swagger`.

### Authentication

POST /api/auth/register Register (User / Merchant / Admin)
POST /api/auth/login Login → JWT pair
POST /api/auth/refresh Rotate refresh token
POST /api/auth/logout Invalidate refresh token
GET /api/auth/me Current user profile

### Products

GET /api/products Paginated product catalogue
GET /api/products/{slug} Product by SEO slug
GET /api/products/{id} Product by ID
POST /api/products Create product (Merchant)
PUT /api/products/{id} Update product (Merchant, own only)
DELETE /api/products/{id} Soft-delete product (Merchant/Admin)
GET /api/products/merchant/{id} Merchant's product listings
POST /api/products/{id}/images Upload product images to AWS S3

### Categories

GET /api/categories All categories + subcategories
GET /api/categories/{slug} Category by slug
POST /api/categories Create category (Admin)
PUT /api/categories/{id} Update category (Admin)
DELETE /api/categories/{id} Delete subcategory (Admin)

### Cart

GET /api/cart Current customer's cart
POST /api/cart/add Add item to cart
PUT /api/cart/update Update item quantity
DELETE /api/cart/remove/{itemId} Remove item from cart
DELETE /api/cart/clear Empty cart

### Orders

POST /api/orders Place order + trigger M-Pesa STK Push
GET /api/orders List orders (Admin: all; Merchant: own; User: own)
GET /api/orders/{id} Order details + line items
PATCH /api/orders/{id}/status Update order status (Merchant/Admin)
GET /api/orders/merchant/{id} Merchant order history

### Payments (M-Pesa)

POST /api/mpesa/stk-push Initiate STK Push payment
POST /api/mpesa/callback M-Pesa payment callback (webhook)
GET /api/mpesa/status/{checkoutId} Check STK Push status
POST /api/mpesa/b2c Initiate B2C payout (Admin)

GET /api/merchants List merchants (Admin)
GET /api/merchants/{id} Merchant profile + store details
PUT /api/merchants/{id} Update merchant profile
PATCH /api/merchants/{id}/approve Approve merchant (Admin)
GET /api/merchants/{id}/dashboard Merchant sales dashboard
POST /api/merchants/{id}/payment-methods Add M-Pesa payout method

### Deliveries & Addresses

GET /api/deliveries List deliveries (Admin/Merchant)
PATCH /api/deliveries/{id}/status Update delivery status
GET /api/addresses Customer's saved addresses
POST /api/addresses Save delivery address
PUT /api/addresses/{id} Update address
DELETE /api/addresses/{id} Remove address

### Reports & Dashboard

GET /api/reports/merchant/{id} Merchant sales report (date range)
GET /api/reports/orders Order summary report (Admin)
GET /api/dashboard/merchant Merchant KPIs (revenue, orders, top products)
GET /api/dashboard/admin Platform-wide metrics

### Search & Recommendations

GET /api/search?q={term} Full-text product search (OpenSearch)
GET /api/recommendations/{userId} Personalised product recommendations
GET /api/products/{id}/similar Similar products by category + tags

### Payouts

GET /api/payouts/merchant/{id} Payout history
POST /api/payouts/request Request payout (Merchant)
GET /api/payouts/{id} Payout details + M-Pesa reference

### Common Query Parameters

?page=0&size=20&sort=createdAt,desc Pagination + sorting
?startDate=2025-01-01&endDate=2025-12-31 Date range filter
?status=PAID Status filter
?merchantId={id} Filter by merchant
?categorySlug={slug} Filter by category
?minPrice=100&maxPrice=5000 Price range filter

---

## 🔒 Security

| Feature | Implementation |
|---------|---------------|
| Password hashing | ASP.NET Identity (PBKDF2, 10,000 iterations) |
| JWT tokens | HMAC-SHA256, 15-min access + 7-day refresh |
| Account lockout | 5 failed attempts → 5-min lockout (Identity built-in) |
| Input validation | DataAnnotations + FluentValidation on all request DTOs |
| SQL injection | EF Core parameterised queries — no raw SQL |
| CORS | Configured per environment (strict in production) |
| HTTPS | Enforced via `UseHttpsRedirection()` |
| Secret management | .NET User Secrets (dev) + environment variables (prod) |
| Forwarded headers | `UseForwardedHeaders()` for reverse proxy support |

---

## 📊 Caching Strategy (Redis)

| Data | TTL | Rationale |
|------|-----|-----------|
| Product catalogue listings | 5 min | High read volume, low write frequency |
| Category tree | 15 min | Almost static — changes only on admin action |
| Product by slug | 5 min | Burst traffic on viral products |
| Merchant dashboard KPIs | 10 min | Expensive aggregation query |
| Recommendation results | 30 min | ML inference is expensive |
| Cart | Session-scoped | Per-user, invalidated on checkout |

---

## ⚡ Event-Driven Flows (RabbitMQ)

| Event | Publisher | Consumer | Action |
|-------|-----------|----------|--------|
| `order.paid` | Payment callback | Notification service | Order confirmation email + SMS |
| `order.paid` | Payment callback | Merchant dashboard | Update sales totals in real time |
| `order.status.updated` | Order service | Notification service | Status update SMS to customer |
| `order.status.updated` | Order service | SignalR hub | Push live update to customer browser |
| `merchant.approved` | Admin service | Notification service | Welcome email + SMS to merchant |
| `payout.processed` | Payout service | Notification service | Payout confirmation SMS to merchant |

---

## 🚢 Deployment

### Docker Compose (local)

```bash
docker compose up -d
```

```yaml
# docker-compose.yml
services:
  api:
    build: .
    ports: ["7001:80"]
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;...
      - ConnectionStrings__Redis=redis:6379

  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: quickcrate
      POSTGRES_PASSWORD: postgres
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine

  rabbitmq:
    image: rabbitmq:3-management
    ports: ["15672:15672"]
```

### Production Dockerfile (multi-stage)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MinimartApi.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MinimartApi.dll"]
```

### Production Environment Variables

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection="Host=prod-db;Database=quickcrate;Username=app;Password=..."
ConnectionStrings__Redis="rediss://default:password@prod-redis:6380"
REDISURL="rediss://default:password@prod-redis:6380"
JwtSettings__Secret="your-256-bit-production-secret"
MpesaGoLive__ConsumerKey="your-live-consumer-key"
MpesaGoLive__ConsumerSecret="your-live-consumer-secret"
AwsConfig__AccessKey="your-aws-access-key"
AwsConfig__SecretKey="your-aws-secret-key"
```

---

## 🗺️ Roadmap

- [x] Authentication — JWT, ASP.NET Identity, RBAC (Admin/Merchant/User)
- [x] Product management — CRUD, SEO slugs, AWS S3 image uploads
- [x] Category management — hierarchical categories with slug routing
- [x] Cart system — session cart with stock validation
- [x] Order management — full lifecycle with status tracking
- [x] M-Pesa integration — STK Push checkout + B2C merchant payouts
- [x] Merchant onboarding — registration, admin approval, dashboard
- [x] Merchant payout system — balance tracking + M-Pesa B2C settlement
- [x] Delivery management — assignment, status tracking, address management
- [x] Redis caching — product listings, dashboard KPIs
- [x] RabbitMQ events — async order notifications
- [x] SignalR — real-time order status updates
- [x] Brevo email — transactional emails (order confirm, receipts)
- [x] Celcom Africa SMS — order notifications, delivery updates
- [x] OpenSearch — full-text product search
- [x] Product recommendations — personalised suggestions
- [x] Serilog structured logging — rolling daily log files
- [x] Docker + Docker Compose — containerised local development
- [ ] GitHub Actions CI/CD pipeline
- [ ] Kubernetes deployment manifests
- [ ] Admin dispute resolution panel
- [ ] Merchant inventory alerts (low stock notifications)
- [ ] Scheduled payout settlement (automated weekly payouts)

---

## 📁 Project Structure

Quickcrate_Marketplace_backend/
├── Controllers/ # API controllers (one per domain)
├── Services/
│ ├── AuthService/ # Login, registration, JWT
│ ├── ProductService/ # Product CRUD + slug generation
│ ├── CategoryService/ # Category + subcategory management
│ ├── OrderService/ # Order placement + validation
│ ├── Cart/ # Cart operations
│ ├── Mpesa/ # STK Push + B2C + callback handling
│ ├── EmailServices/ # Brevo transactional email
│ ├── NotificationService/ # SMS via Celcom Africa
│ ├── RabbitMQ/ # Event publisher + consumer
│ ├── SignalR/ # ActivityHub (real-time updates)
│ ├── Dashboard/ # Merchant + admin analytics
│ ├── Payouts/ # Merchant payout processing
│ ├── Deliveries/ # Delivery tracking
│ ├── Address/ # Customer address management
│ ├── Recommedation/ # Product recommendation engine
│ ├── SearchService/ # OpenSearch integration
│ └── SlugService/ # SEO URL slug generation
├── Repositories/ # Data access layer (mirrors Services)
├── Models/ # EF Core entity classes
├── DTOS/ # Request + response contracts
│ ├── Authorization/ # Auth request/response DTOs
│ ├── Payments/ # M-Pesa DTOs
│ ├── Notification/ # SMS/Email DTOs
│ ├── AWS/ # S3 upload DTOs
│ └── Configuration/ # Settings binding DTOs
├── Mappings/ # AutoMapper profiles
├── Core/ # JWT helpers, shared utilities
├── Configuration/ # App configuration classes
├── Exceptions/ # Custom exceptions + global handler
├── Data/
│ └── MinimartDBContext.cs # EF Core DbContext
├── Database/Scripts/ # SQL migration scripts
├── Uploads/ # Local file upload directory (dev)
├── Logs/ # Serilog rolling log files
├── Program.cs # DI registration + middleware pipeline
├── Dockerfile # Multi-stage production build
├── docker-compose.yml # Local development stack
└── appsettings.Template.json # Config template (no secrets)

---

## 👤 Author

**Caleb Muchiri**
Full-Stack Software Engineer · Nairobi, Kenya

[![LinkedIn](https://img.shields.io/badge/LinkedIn-Connect-0077B5?style=flat&logo=linkedin)](https://linkedin.com/in/caleb-muchiri-909ba6266)
[![GitHub](https://img.shields.io/badge/GitHub-Follow-181717?style=flat&logo=github)](https://github.com/CalebMUC)

---

## 📄 License

Proprietary — Quickcrate © 2026
