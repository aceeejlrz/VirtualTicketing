# Virtual Event Ticketing – Status Report

## Completed Requirements

1. **Project Setup**
   - ASP.NET Core MVC project targeting **.NET 8.0**.
   - EF Core 8 + **PostgreSQL (Npgsql)** configured via `DefaultConnection`.
   - Folders present: `Models`, `Views`, `Controllers`, `Services`, `ViewModels`, `DTOs`, `wwwroot/uploads`, `wwwroot/uploads/tickets`, `wwwroot/logs`.

2. **Identity Implementation**
   - ASP.NET Core Identity with custom `ApplicationUser` (FullName, PhoneNumber, ProfileImagePath).
   - Full auth flow: Register, Login, Logout, Email Confirmation, Forgot/Reset Password.
   - Custom `AccountController` + Bootstrap-styled Razor views.

3. **Roles Setup**
   - Roles **Admin**, **Organizer**, **Attendee** seeded.
   - Default admin user seeded.
   - `[Authorize(Roles="...")]` on protected actions; custom `AccessDenied` page.

4. **Event Model + Database**
   - `Event` model includes all required fields (Title, Description, CategoryId, Start/End dates, Price, TicketsAvailable, OrganizerId, ImagePath, CreatedAt).
   - Migrations created and applied.

5–7. **Admin & Organizer Event Management**
   - Admin can create/edit/delete any event.
   - Organizer can create events tied to their user and only manage their own events.
   - Ownership enforced in `EventsController` (`CanManageEvent`).

8. **Attendee Restrictions**
   - `/Events/Create` and other management actions restricted to Admin/Organizer.
   - Unauthenticated users redirected to login.
   - Authenticated attendees see Access Denied if they hit restricted actions.

9. **Ticket Purchase System**
   - Session-backed cart (AddToCart, UpdateCartItem, CartSummary APIs).
   - Ticket quantity changes via AJAX.
   - Stock validated and deducted.
   - Purchases persisted and linked to logged-in user when available.

10. **QR Code Generation**
   - `TicketQrService` uses **QRCoder**.
   - QR PNGs saved under `wwwroot/uploads/tickets` and shown on Dashboard.

11. **PDF Ticket Download**
   - `TicketPdfService` uses **QuestPDF** with community license.
   - `/Purchases/TicketPdf` endpoint returns PDF tickets (event info + QR).

12. **Dashboard (/Dashboard)**
   - `DashboardController` + view with four sections:
     - *My Tickets* (upcoming events, QR, PDF links).
     - *Purchase History* (past events + 1–5 star rating per ticket).
     - *My Events* (Organizer/Admin: events created + revenue per event).
     - *Profile* (edit name, phone, profile picture upload).

13. **AJAX Features**
   - Live search on home page: `/Events/Search` + `_EventPartial` + loading spinner.
   - Dynamic cart:
     - `AddToCart` / `UpdateCartItem` / `CartSummary` JSON endpoints.
     - Cart badge in navbar; total price in purchase view.
     - "Only X tickets left" alert when stock is low.
   - Purchase modal with confetti:
     - Bootstrap modal on confirmation view.
     - `particles.js` used for confetti animation.

14. **Global Error Handling**
   - `UseExceptionHandler("/Home/Error500")` and `UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}")`.
   - Custom `Error500` and `NotFound (404)` views.
   - No stack traces shown in production views.

15. **Serilog Logging**
   - Serilog configured with rolling daily logs at `wwwroot/logs/log-{date}.txt`.
   - Logs for: auth events, access denied, purchases, and unhandled errors.

16. **Analytics Dashboard (Chart.js)**
   - `/Events/MyAnalytics` view using Chart.js.
   - JSON APIs: `SalesByCategory`, `MonthlyRevenue`, `TopEvents`.

18. **Unit Testing**
   - `VirtualEventTicketing.Tests` xUnit project added to solution.
   - Tests cover:
     - Models: `Event` validation & `IsSoldOut` logic.
     - Controllers: `HomeController.Index` returns view.
     - Services: `TicketPdfService` PDF generation.
   - **Build + test projects compile successfully.**
   - Note: `dotnet test` on this machine fails at runtime because the ASP.NET Core 8.0 runtime is not installed; once installed, tests should execute.

## Partially Completed / Manual Steps Required

17. **Azure Deployment**
   - Code is Azure-ready:
     - Uses `DefaultConnection` (to be configured in Azure Connection Strings).
     - Logging and error handling production-safe.
   - **Manual steps needed:**
     - Create Azure App Service.
     - Publish from IDE/CLI.
     - Configure connection strings and any secrets in Azure App Settings.
     - Verify `ASPNETCORE_ENVIRONMENT=Production` and test live site.

19. **Video Demonstration**
   - Project includes all features required for the demo.
   - **Manual steps needed:**
     - Record ≤10 minute video on Azure deployment URL.
     - Follow professor’s script (see FinalSubmissionChecklist).

20. **Final Submission Items**
   - See `FinalSubmissionChecklist.md` for detailed to-dos (GitHub repo, Azure URL, video, etc.).

## Known Environment Constraint

- `dotnet test` currently fails on this macOS environment because the **Microsoft.AspNetCore.App 8.0.0** runtime is missing. This is an environment issue, not a code issue. Installing the .NET 8 ASP.NET Core runtime should allow all tests to run.
