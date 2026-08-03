# Hospital Management System — Subramanya Home Nursing

## Project Overview

ASP.NET Core MVC application (.NET 7) for managing nursing home operations including:
- Home Nursing patient management
- Old Age care management
- Helper (staff) management
- Attendance tracking with GPS check-in/check-out
- Cash Memo
- Salary Slip generation
- User management with face recognition login

**Stack:** ASP.NET Core 7 MVC · EF Core 7 · Microsoft SQL Server (external) · Bootstrap 4 · jQuery · DataTables

---

## Running the Project

The configured workflow runs:
```
cd NursingHome && dotnet run --urls=http://0.0.0.0:5000
```

---

## Database Configuration

The SQL Server connection string is stored as a Replit Secret named:

```
ConnectionStrings__NursingHome
```

ASP.NET Core picks this up automatically from the environment (double-underscore is the hierarchy separator). **Never commit credentials to `appsettings.json`.**

---

## Pending Database Migrations

Before certain features work, run these migration scripts **once** against your SQL Server database (in order):

| Script | What it adds |
|--------|-------------|
| `DBSchema/Migrations/001_AddGpsCheckIn.sql` | GPS check-in columns (Latitude, Longitude, GpsAccuracy, CheckInTime, Address, Status) |
| `DBSchema/Migrations/002_AddCheckOutAndApproval.sql` | GPS check-out columns + manager approval columns (CheckOutTime, CheckOutLatitude, CheckOutLongitude, CheckOutGpsAccuracy, CheckOutAddress, TotalHours, ManagerRemarks, ApprovedBy, ApprovalTimestamp) |

The full schema reference is at `DBSchema/dbo/Tables/Attendance.sql`.

---

## Attendance Module

### Employee Flow
1. **Check In** — GPS captured by browser; server records timestamp + reverse-geocodes address; status set to *Pending Approval*
2. **Check Out** — Same GPS flow; record updated with checkout time/location; status remains *Pending Approval*

### Manager Flow (admin role only)
- **Pending Approvals tab** is visible only to admin users
- Each pending record shows: employee, patient, both timestamps, calculated hours, both GPS addresses and coordinates
- **Approve** → total hours calculated automatically, status → *Approved*
- **Reject** → optional remarks stored, status → *Rejected*

---

## User Preferences

- Keep changes isolated to the module being modified; do not touch unrelated modules
- Use `msgPopup` (SweetAlert2 toast) for success/error messages throughout the app
- GPS timestamps are always server-side — never trust the client clock
- Connection strings and credentials must use Replit Secrets, never hardcoded files
