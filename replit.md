# Nursing Home / Hospital Management System

## Overview
An ASP.NET Core MVC (.NET 7) web application for managing nursing home operations — helpers, patients, attendance, payroll, cash memos, and configuration.

## Stack
- **Backend:** ASP.NET Core MVC, .NET 7
- **Data access:** Entity Framework Core 7, Microsoft.Data.SqlClient
- **Database:** External SQL Server (bsite.net — see connection string in `appsettings.json`)
- **Frontend:** Bootstrap, jQuery, DataTables, Select2, FontAwesome, Tailwind (CDN), html2canvas, jsPDF

## Project structure
```
NursingHome/          — Main web app (controllers, views, wwwroot)
NursingHome.Db/       — Data layer (models, interfaces, EF implementations)
DBSchema/             — SQL project for schema
```

## How to run
The configured workflow starts the app with:
```
cd NursingHome && dotnet run --urls=http://0.0.0.0:5000
```

## Important notes
- The database connection string in `appsettings.json` points to an external SQL Server on `sql.bsite.net`. If that server is unreachable from Replit, the app will start but all database operations will fail.
- No README or default credentials were included in the original repository.
- The Helper module includes a full **User Assignment** feature: admins can assign/change/remove a user account linked to each helper, or create a new user account directly from the Helper page.

## User preferences
