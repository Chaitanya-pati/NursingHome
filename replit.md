# NursingHome

An ASP.NET Core MVC (.NET 7) nursing home management web application.

## Project structure

- `NursingHome/` — main web app (controllers, views, static assets)
- `NursingHome.Db/` — data-access library (interfaces + EF Core implementations)

## How to run

The workflow **"Start application"** runs:
```
cd NursingHome && dotnet run --urls=http://0.0.0.0:5000
```

The app starts on port 5000. Default route goes to the Login page (`/Users/Login`).

## Database

Connected to an external MS SQL Server hosted at `sql.bsite.net` (bsite.net free hosting).  
Connection string is in `NursingHome/appsettings.json` under `"NursingHome"`.  
No local database — all reads/writes go to the live remote database.

## Notes

- `app.UseHttpsRedirection()` is commented out in `Program.cs` — Replit's reverse proxy handles TLS, so enabling it would cause redirect loops.
- The face-api.js library requires WebGL; WebGL warnings in the browser console are expected in headless/preview environments and do not affect core functionality.
- The app uses session-based authentication managed in JavaScript (stored in `sessionStorage` on the client side).
