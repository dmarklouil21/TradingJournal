# Trading Journal - Server (Backend API)

This directory contains the backend REST API for the Trading Journal platform, built with **ASP.NET Core Web API**. It securely handles business logic, database transactions, authentication, and external service integrations.

## 🚀 Technologies Used
* **Framework**: ASP.NET Core (.NET 10.0)
* **Database**: PostgreSQL
* **ORM**: Entity Framework Core (Npgsql)
* **Authentication**: ASP.NET Core Identity & JWT (JSON Web Tokens)
* **Storage**: Supabase C# SDK (for chart image hosting)

## 📁 Directory Structure & Categories

### `/Controllers`
The entry points for HTTP requests. They handle routing, input validation (ModelState), and delegate business logic to the appropriate services.
* **`AuthController.cs`**: User registration and JWT token generation.
* **`DashboardController.cs`**: Aggregates net worth and cross-module activity timelines.
* **`TradingJournalController.cs`**: Handles Active Trading features, including multipart form uploads for chart images.

### `/Services`
Contains the core business logic of the application, keeping controllers thin.
* **`AuthService.cs`**: Manages user identity creation and password hashing.
* **`DashboardService.cs`**: Calculates unified metrics across different database domains.
* **`ExchangeRateService.cs`**: Fetches and caches live USD-to-PHP exchange rates using a public API.
* **`TradingJournalService.cs`**: Manages trade entries and uploads files to Supabase Storage.

### `/Models`
Defines the Entity Framework Core entities that map directly to PostgreSQL database tables. Separated logically into domains (e.g., `TradingJournal/`, `InvestingTracker/`).

### `/DTO` (Data Transfer Objects)
Objects used specifically for passing data between the client and server. These prevent over-posting vulnerabilities and decouple the database schema from the API payloads (e.g., `NewTradeDTO`, `DashboardSummaryDTO`).

### `/Data`
Contains the `TradingJournalContext.cs` which inherits from `IdentityDbContext` to configure Entity Framework behaviors, relationships, and DbSet registrations.

## ⚙️ Setup & Installation

1. Navigate to the server directory:
   ```sh
   cd server
   ```
2. Update the `appsettings.json` (or `appsettings.Development.json`) with your specific connection strings and secrets:
   - `ConnectionStrings:DefaultConnection` (PostgreSQL string)
   - `Jwt:Key` (Secret key for signing tokens)
   - `Supabase` URLs and keys (if testing image uploads)
3. Apply database migrations to create the schema:
   ```sh
   dotnet ef database update
   ```
4. Run the API:
   ```sh
   dotnet run
   ```
