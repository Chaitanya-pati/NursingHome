# Nursing Home Management System

An ASP.NET Core 7 MVC web application for managing nursing home / home healthcare operations. Built by Codemind Software Solutions.

## Stack
- **Backend:** ASP.NET Core 7 MVC (C#)
- **Database:** SQL Server (external, hosted at bsite.net)
- **Frontend:** Razor views, jQuery, Bootstrap, Font Awesome, face-api.js

## Modules
- Users / Login
- Old Age patients management
- Nursing Home (home nursing) management
- Attendance tracking
- Cash Memo / billing
- Salary Slip generation
- Helpers management
- Configuration (countries, etc.)
- Reports / Dashboard

## How to run
The workflow **"Start application"** starts the app:
```
cd NursingHome && dotnet run --urls=http://0.0.0.0:5000
```
The app starts on port 5000 and opens at the Login page (`/Users/Login`).

## Database
Connected to an external SQL Server on `sql.bsite.net`. Connection string is in `NursingHome/appsettings.json`.

## Notes
- Face recognition (face-api.js) requires WebGL — not available in Replit's sandboxed preview browser; works in a real browser
- WASM backend for face-api also requires the wasm files to be served correctly
- The app uses session-based login with no ASP.NET Core Identity — custom `UserService`

## User preferences
