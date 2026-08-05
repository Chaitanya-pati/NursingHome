using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace NursingHome.Db.Implementation
{
    public class AttedanceService : IAttedanceService
    {
        private readonly DbContextOptions<TaskContext> _dbConn;

        public AttedanceService(string DbConn)
        {
            _dbConn = new DbContextOptionsBuilder<TaskContext>().UseSqlServer(DbConn).Options;
        }

        // ────────────────────────────────────────────────────────────────────
        // Admin add (manual / legacy records)
        // ────────────────────────────────────────────────────────────────────
        public bool AddAttendance(Attendance AddAttendenceData)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);

                var attendance = new Attendance
                {
                    fkHelperId  = AddAttendenceData.fkHelperId,
                    fkNursingId = AddAttendenceData.fkNursingId,
                    Date        = AddAttendenceData.Date,
                    Time        = AddAttendenceData.Time,
                    Description = AddAttendenceData.Description,
                    Status      = "Approved"   // admin-added records are pre-approved
                };

                Db.Attendance.Add(attendance);
                Db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // GPS Check-In
        // ────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Records a GPS check-in. All GPS and timestamp fields must be
        /// pre-populated by the caller (controller).
        /// </summary>
        public bool RecordCheckIn(Attendance checkIn)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);

                var record = new Attendance
                {
                    fkHelperId  = checkIn.fkHelperId,
                    fkNursingId = checkIn.fkNursingId,
                    Date        = checkIn.CheckInTime.HasValue
                                      ? checkIn.CheckInTime.Value.Date
                                      : (DateTime?)null,
                    CheckInTime = checkIn.CheckInTime,
                    Latitude    = checkIn.Latitude,
                    Longitude   = checkIn.Longitude,
                    GpsAccuracy = checkIn.GpsAccuracy,
                    Address     = checkIn.Address,
                    Status      = checkIn.Status,
                    Description = checkIn.Description
                };

                Db.Attendance.Add(record);
                Db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // GPS Check-Out
        // ────────────────────────────────────────────────────────────────────
        public bool RecordCheckOut(int attendanceId, DateTime checkOutTime,
                                   double lat, double lon, double gpsAccuracy, string address)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
                var record = Db.Attendance.FirstOrDefault(a => a.Id == attendanceId);

                if (record == null)
                    return false;

                // Guard: don't overwrite an existing check-out
                if (record.CheckOutTime.HasValue)
                    return false;

                record.CheckOutTime        = checkOutTime;
                record.CheckOutLatitude    = lat;
                record.CheckOutLongitude   = lon;
                record.CheckOutGpsAccuracy = gpsAccuracy;
                record.CheckOutAddress     = address;

                Db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // Manager Approval
        // ────────────────────────────────────────────────────────────────────
        public bool ApproveAttendance(int id, string approvedBy, string remarks)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
                var record = Db.Attendance.FirstOrDefault(a => a.Id == id);

                if (record == null || record.Status != "Pending Approval")
                    return false;

                // Calculate total working hours on approval.
                // TotalHours is the canonical field (FLOAT). The legacy Time column
                // is no longer written here — migration 002 converts it to FLOAT and
                // it is populated only by the admin add/edit form path.
                if (record.CheckInTime.HasValue && record.CheckOutTime.HasValue)
                {
                    var span = record.CheckOutTime.Value - record.CheckInTime.Value;
                    record.TotalHours = Math.Round(span.TotalHours, 2);
                }

                record.Status            = "Approved";
                record.ApprovedBy        = approvedBy;
                record.ApprovalTimestamp = DateTime.Now;
                record.ManagerRemarks    = remarks;

                Db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RejectAttendance(int id, string approvedBy, string remarks)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
                var record = Db.Attendance.FirstOrDefault(a => a.Id == id);

                if (record == null || record.Status != "Pending Approval")
                    return false;

                record.Status            = "Rejected";
                record.ApprovedBy        = approvedBy;
                record.ApprovalTimestamp = DateTime.Now;
                record.ManagerRemarks    = remarks;

                Db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // Data queries
        // ────────────────────────────────────────────────────────────────────
        public List<object> GetHelperAttendance(int? helperIdFilter = null)
        {
            using var Db = new TaskContext(_dbConn);
            var result = (from a in Db.Attendance
                          join h in Db.Helpers     on a.fkHelperId  equals h.Id
                          join o in Db.HomeNursing on a.fkNursingId equals o.Id
                          where helperIdFilter == null || a.fkHelperId == helperIdFilter
                          orderby a.CheckInTime descending, a.Date descending
                          select new
                          {
                              HelperName          = h.Name,
                              AttendanceId        = a.Id,
                              Time                = a.Time,
                              fkHelperId          = a.fkHelperId,
                              fknursing           = a.fkNursingId,
                              AttendanceDate      = a.Date,
                              Description         = a.Description,
                              PatientName         = o.PatientName,
                              // Check-In
                              CheckInTime         = a.CheckInTime,
                              Latitude            = a.Latitude,
                              Longitude           = a.Longitude,
                              GpsAccuracy         = a.GpsAccuracy,
                              Address             = a.Address,
                              Status              = a.Status,
                              // Check-Out
                              CheckOutTime        = a.CheckOutTime,
                              CheckOutLatitude    = a.CheckOutLatitude,
                              CheckOutLongitude   = a.CheckOutLongitude,
                              CheckOutGpsAccuracy = a.CheckOutGpsAccuracy,
                              CheckOutAddress     = a.CheckOutAddress,
                              // Approval
                              TotalHours          = a.TotalHours,
                              ManagerRemarks      = a.ManagerRemarks,
                              ApprovedBy          = a.ApprovedBy,
                              ApprovalTimestamp   = a.ApprovalTimestamp
                          }).ToList<object>();

            return result;
        }

        public List<object> GetPendingAttendance()
        {
            using var Db = new TaskContext(_dbConn);
            var result = (from a in Db.Attendance
                          join h in Db.Helpers     on a.fkHelperId  equals h.Id
                          join o in Db.HomeNursing on a.fkNursingId equals o.Id
                          where a.Status == "Pending Approval" && a.CheckOutTime != null
                          orderby a.CheckInTime descending
                          select new
                          {
                              HelperName          = h.Name,
                              AttendanceId        = a.Id,
                              AttendanceDate      = a.Date,
                              PatientName         = o.PatientName,
                              Description         = a.Description,
                              // Check-In
                              CheckInTime         = a.CheckInTime,
                              Latitude            = a.Latitude,
                              Longitude           = a.Longitude,
                              GpsAccuracy         = a.GpsAccuracy,
                              Address             = a.Address,
                              Status              = a.Status,
                              // Check-Out
                              CheckOutTime        = a.CheckOutTime,
                              CheckOutLatitude    = a.CheckOutLatitude,
                              CheckOutLongitude   = a.CheckOutLongitude,
                              CheckOutGpsAccuracy = a.CheckOutGpsAccuracy,
                              CheckOutAddress     = a.CheckOutAddress
                          }).ToList<object>();

            return result;
        }

        public bool UpdateAttendance(Attendance updatedAttendanceData)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);

                var existingAttendance = Db.Attendance.FirstOrDefault(a => a.Id == updatedAttendanceData.Id);

                if (existingAttendance == null)
                    return false;

                existingAttendance.fkHelperId  = updatedAttendanceData.fkHelperId;
                existingAttendance.fkNursingId = updatedAttendanceData.fkNursingId;
                existingAttendance.Date        = updatedAttendanceData.Date;
                existingAttendance.Time        = updatedAttendanceData.Time;
                existingAttendance.Description = updatedAttendanceData.Description;

                Db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteAttendance(int id)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);

                var existingAttendance = Db.Attendance.FirstOrDefault(a => a.Id == id);

                if (existingAttendance == null)
                    return false;

                Db.Attendance.Remove(existingAttendance);
                Db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<object> PatientDetails()
        {
            using var Db = new TaskContext(_dbConn);
            var query = @"
        SELECT 
            Id,
            PatientName
        FROM HomeNursing";

            var result = Db.HomeNursing
                           .FromSqlRaw(query)
                           .Select(p => new { p.Id, p.PatientName })
                           .ToList();

            return result.Cast<object>().ToList();
        }

        public List<object> GetHelpers()
        {
            using var Db = new TaskContext(_dbConn);
            var query1 = @"
        SELECT 
            Id,
            Name
        FROM Helpers";

            var result = Db.Helpers
                           .FromSqlRaw(query1)
                           .Select(h => new { h.Id, h.Name })
                           .ToList();

            return result.Cast<object>().ToList();
        }
    }
}
