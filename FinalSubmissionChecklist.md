# Final Submission Checklist – Virtual Event Ticketing

Use this as a guide before submitting your assignment.

## Project & Repo
- [ ] **Solution builds on .NET 8** without errors.
- [ ] All migrations applied to your PostgreSQL database.
- [ ] `StatusReport.md` updated if you changed anything.
- [ ] Git repository is **private** on GitHub.
- [ ] Professor added as collaborator with read access.
- [ ] Latest code pushed to GitHub before recording video.

## Azure Deployment
- [ ] Azure App Service created (Linux or Windows, .NET 8).
- [ ] Project published to Azure from IDE or `dotnet publish` + deploy.
- [ ] `ASPNETCORE_ENVIRONMENT` set to `Production` in Azure.
- [ ] Connection string `DefaultConnection` configured in Azure App Settings.
- [ ] App loads successfully at Azure URL (not localhost).
- [ ] Test key flows on Azure:
  - [ ] Register, confirm email (check logs or email sink).
  - [ ] Login/Logout.
  - [ ] Event browsing and ticket purchase.

## Identity & Roles Demo
- [ ] Registration, Login, Logout flows tested.
- [ ] Email confirmation + Forgot/Reset password exercised.
- [ ] Admin user exists and can log in.
- [ ] Organizer and Attendee roles created and assigned.
- [ ] AccessDenied page appears when expected (e.g., Attendee hitting /Events/Create).

## Features to Demonstrate in Video
- [ ] **Introduction slide** with:
  - [ ] Profile photos.
  - [ ] Names & Student IDs.
  - [ ] Course code, name, section.
  - [ ] Assignment info.
- [ ] Registration / Login / Logout.
- [ ] Email confirmation.
- [ ] Forgot / Reset password.
- [ ] Admin features (full event CRUD).
- [ ] Organizer features (own events only).
- [ ] Attendee restrictions on event creation.
- [ ] `/Dashboard` sections:
  - [ ] My Tickets (upcoming events, QR, PDF button).
  - [ ] Purchase History (past events, ratings).
  - [ ] My Events (Organizer only, with revenue).
  - [ ] Profile (name, phone, profile photo upload).
- [ ] AJAX Live Search on home page (spinner + dynamic results).
- [ ] AJAX cart badge + total update + "Only X tickets left!" alert.
- [ ] Purchase modal with confetti (particles.js).
- [ ] Custom 404 + custom 500 pages.
- [ ] Serilog log file in `wwwroot/logs` showing real entries.
- [ ] **Azure URL** clearly shown (browser address bar).
- [ ] Unit tests running (after installing ASP.NET Core 8 runtime locally).
- [ ] Status report shown.
- [ ] Each member explains a portion of the code.

## Submission Package
- [ ] **GitHub private repo URL** submitted.
- [ ] Professor added as collaborator.
- [ ] **Working Azure URL** submitted.
- [ ] **StatusReport.md** submitted.
- [ ] **Demo video** uploaded (≤ 10 minutes) using Azure site only.
- [ ] Confirm no code was shared with other groups.
- [ ] Acknowledge late policy (−20% per day if applicable).

> Tip: Before recording the video, rehearse the exact flow using this checklist to avoid missing any required demo step.
