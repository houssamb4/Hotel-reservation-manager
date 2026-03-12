# Hotel Reservation Manager

A full-stack hotel management system split into two integrated applications: a desktop **back-office** for staff and a web-based **front-office** for customers.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Back-Office (Admin Desktop App)](#back-office-admin-desktop-app)
- [Front-Office (Customer Web App)](#front-office-customer-web-app)
- [Database Schema](#database-schema)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Security Notes](#security-notes)

---

## Overview

| | Back-Office | Front-Office |
|---|---|---|
| **Target Users** | Hotel staff & administrators | Hotel guests |
| **Tech** | C# WinForms (.NET Framework 4.8) | ASP.NET Core 8.0 (Razor Pages + REST API) |
| **Purpose** | Manage rooms, reservations, clients, employees, payments | Browse rooms & submit booking requests |
| **Authentication** | Staff login (email + password) | Public — no authentication required |
| **Database** | SQL Server (shared `hotel` database) | SQL Server (same shared database) |

---

## Architecture

```
Hotel-reservation-manager/
├── Partie Back-office/        # C# WinForms desktop application
│   ├── Login/                 # Main project source (forms, logic, assets)
│   └── packages/              # NuGet packages
└── Partie-Font-Office/        # ASP.NET Core web application
    └── Partie-Font-Office/    # Main project source (pages, API, helpers)
```

Both applications connect to the **same SQL Server database** (`hotel`) and share data in real time.

---

## Back-Office (Admin Desktop App)

### Tech Stack

| Technology | Detail |
|---|---|
| **Language** | C# |
| **Framework** | .NET Framework 4.8 |
| **UI** | Windows Forms (WinForms) |
| **Database Access** | ADO.NET — `SqlConnection`, `SqlCommand`, `SqlDataAdapter` |
| **Database** | SQL Server (`HP\SQLEXPRESS`, database: `hotel`) |

### NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `iTextSharp` | 5.5.13.4 | PDF report generation |
| `MailKit` | 4.9.0 | Sending emails (SMTP) |
| `MimeKit` | 4.9.0 | Building MIME email messages |
| `Newtonsoft.Json` | 13.0.3 | JSON serialization |
| `RestSharp` | 110.2.0 | HTTP client for REST API calls |
| `BouncyCastle.Cryptography` | 2.5.0 | Cryptographic operations |
| `Syncfusion.Licensing` | 27.2.5 | Syncfusion UI component licensing |
| `System.Management` | 8.0.0 | Windows system info access |

### Modules & Screens

#### Authentication
| Form | Description |
|---|---|
| `Form1` | Main login screen — staff enter credentials to access the dashboard |
| `Form3` | Forgot password — sends a reset link via email |
| `Form2` | Sign-up form — register a new staff account |

#### Dashboard (`Dashboard.cs`)
- Overview cards: total reservations, total clients, pending reservations, available rooms
- Activity log table: who did what and when (from `ActivityLog` table)

#### Reservations (`Reservation.cs`, `ReservationDetailsForm.cs`, `EditReservationForm.cs`, `Reservationform.cs`)
- List all reservations with search/filter (by client name or status: *En attente*, *Confirmée*, *Payé*)
- View detailed reservation info
- Edit or cancel an existing reservation
- Create a new reservation manually

#### Rooms (`Rooms.cs`, `RoomDetails.cs`, `RoomEditForm.cs`)
- Room inventory with stats: Available / Occupied / Under Maintenance
- Filter by floor, status, and type (Single, Double, Lux, VIP)
- Edit room details and status

#### Clients (`Client.cs`, `Account.cs`)
- Full CRUD for hotel guest records (name, email, phone, address)
- All changes are recorded in the `ActivityLog`

#### Payments (`Payments.cs`, `PaymentForm.cs`, `PaymentDetails.cs`)
- Summary of total paid vs. pending amounts
- Payment grid: Payment ID, Client, Reservation, Amount, Date, Status
- Confirm payment received and update records

#### Employees (`Employe.cs`)
- List all staff members with online/offline status, last login, creation date
- Search staff by name

#### Utilities
| File | Description |
|---|---|
| `Database.cs` | Centralised SQL Server connection management (`OpenConnexion`, `CloseConnexion`, `IsConnectionAvailable`, `ExecuteScalar`) |
| `Login.cs` | Authentication logic — verifies credentials, sets session (`LoggedInUserFullName`, `LoggedInUserID`), marks user online/offline |
| `SignUp.cs` | Business logic for staff registration (email uniqueness check, password hashing) |
| `FormTransition.cs` | Smooth slide-in/slide-out animation helper for form transitions |
| `LoadingAnimation.cs` | Loading spinner form shown during long operations |
| `home.cs` | Quick reservation creation form — accessible from multiple screens |

---

## Front-Office (Customer Web App)

### Tech Stack

| Technology | Detail |
|---|---|
| **Language** | C# |
| **Framework** | ASP.NET Core 8.0 |
| **UI** | Razor Pages + static HTML/CSS/JS |
| **API** | ASP.NET Core Web API (REST) |
| **Database Access** | ADO.NET via `System.Data.SqlClient` |
| **Database** | SQL Server (same `hotel` database) |
| **Email** | MailKit via Gmail SMTP |
| **Logging** | Serilog |

### NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `itext7` | 9.0.0 | PDF booking confirmation generation |
| `PDFsharp` | 6.1.1 | Supplementary PDF document creation |
| `MailKit` | 4.9.0 | Sending booking confirmation emails |
| `MimeKit` | 4.9.0 | MIME message construction |
| `Serilog.AspNetCore` | 9.0.0 | Structured application logging |
| `System.Data.SqlClient` | 4.9.0 | SQL Server connectivity |

### Pages

| Page | Description |
|---|---|
| `Index.cshtml` | Home page |
| `rooms/` | Room listing pages |
| `room-details.cshtml` | Detailed view of a specific room type |
| `reservation.cshtml` | Booking form — customer fills in personal info and stay dates |
| `sent.cshtml` | Confirmation page shown after a successful booking |
| `Privacy.cshtml` | Privacy policy |
| `Error.cshtml` | Error handling page |

### REST API Endpoints

#### `POST /api/reservation`
Processes a booking request submitted from the front-end form.

**Request Body**
```json
{
  "fullName": "string",
  "email": "string",
  "phone": "string",
  "address": "string",
  "roomType": "single | double | lux | vip",
  "checkInDate": "YYYY-MM-DD",
  "checkOutDate": "YYYY-MM-DD",
  "guests": 2
}
```

**What it does**
1. Validates that check-in date is before check-out date
2. Inserts or retrieves the client record from the `Client` table
3. Creates a `reservation` record inside a SQL transaction
4. Generates a PDF booking confirmation
5. Sends the confirmation by email via Gmail SMTP
6. Returns a success or error JSON response

**Responses**
- `200 OK` — reservation created, email sent
- `400 Bad Request` — validation error (e.g., invalid dates)
- `500 Internal Server Error` — unexpected failure (transaction rolled back)

#### `GET /api/database/test`
Health check endpoint — verifies the database connection is alive.

### Helpers

| File | Description |
|---|---|
| `DatabaseHelper.cs` | SQL Server connection factory; `TestConnection()` called on startup |
| `PdfGenerator.cs` | Generates an in-memory PDF confirmation from reservation data; returns a `byte[]` |

---

## Database Schema

```
Utilisateur          — Staff / admin accounts
├─ id                PK
├─ nom_complet
├─ email             UNIQUE
├─ password
├─ status            online | offline
├─ created_at
└─ last_login

Client               — Hotel guests
├─ id                PK
├─ nom_complet
├─ email
├─ telephone
└─ Adresse

type_chambre         — Room categories
├─ id                PK
├─ type_name         Single | Double | Lux | VIP
└─ description

chambre              — Physical rooms
├─ id                PK
├─ num_chambre       Room number
├─ type_chambre_id   FK → type_chambre
└─ status            Available | Occupied | Under Maintenance

reservation          — Bookings
├─ id                PK
├─ client_id         FK → Client
├─ chambre_type_id   FK → type_chambre
├─ date_arrivee      Check-in date
├─ date_depart       Check-out date
├─ status            En attente | Confirmée | Payé
└─ TotalAmount

paiement             — Payment records
├─ id                PK
├─ reservation_id    FK → reservation
├─ montant           Amount
└─ date

ActivityLog          — Audit trail
├─ UserID            FK → Utilisateur
├─ ActivityType      e.g. "New Client Created"
└─ created_at
```

---

## Prerequisites

| Requirement | Back-Office | Front-Office |
|---|---|---|
| OS | Windows | Windows (dev), any (hosting) |
| .NET | .NET Framework 4.8 | .NET 8.0 SDK |
| Database | SQL Server (Express or higher) | Same SQL Server instance |
| IDE | Visual Studio 2019+ | Visual Studio 2022 / VS Code |

---

## Getting Started

### 1. Database Setup

Create a SQL Server database named `hotel` and run the schema scripts to create the tables listed in the [Database Schema](#database-schema) section.

### 2. Back-Office

```bash
# Open solution
Partie Back-office/Login.sln

# Restore NuGet packages, then build and run in Visual Studio
```

Update the connection string in `App.config` if your SQL Server instance name differs from `HP\SQLEXPRESS`.

### 3. Front-Office

```bash
cd "Partie-Font-Office/Partie-Font-Office"

# Restore packages
dotnet restore

# Run the web application
dotnet run
```

The app will start on `https://localhost:5001` (or the port shown in the terminal).

Update `appsettings.json` with your own SQL Server connection string and SMTP credentials before running.

---

## Security Notes

> **Important:** The following should be addressed before deploying to any shared or production environment.

- **SMTP credentials** in `appsettings.json` are stored in plain text. Move them to environment variables or [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):
  ```bash
  dotnet user-secrets set "Smtp:Password" "<your-app-password>"
  ```
- **SQL Server connection** uses Windows Integrated Authentication (`Trusted_Connection=True`), which is safe for local development but requires proper service account configuration in production.
- **Back-office passwords** — ensure passwords are hashed (e.g., BCrypt via BouncyCastle) and never stored in plain text.
- **Input validation** — all user inputs going into SQL queries should use parameterised commands (already done in most places via `@param` syntax; verify all code paths).
