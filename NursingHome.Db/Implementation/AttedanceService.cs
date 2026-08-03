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

        public bool AddAttendance(Attendance AddAttendenceData)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);

                // Convert double? Time (representing hours) to TimeSpan
                TimeSpan convertedTime = AddAttendenceData.Time.HasValue
                    ? TimeSpan.FromHours(AddAttendenceData.Time.Value)
                    : TimeSpan.Zero; // Default to TimeSpan.Zero if Time is null

                var attendance = new Attendance
                {
                    fkHelperId   = AddAttendenceData.fkHelperId,
                    fkNursingId  = AddAttendenceData.fkNursingId,
                    Date         = AddAttendenceData.Date,
                    Time         = AddAttendenceData.Time,
                    Description  = AddAttendenceData.Description
                };

                Db.Attendance.Add(attendance);
                Db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

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

        public List<object> GetHelperAttendance()
        {
            using var Db = new TaskContext(_dbConn);
            var result = (from a in Db.Attendance
                          join h in Db.Helpers    on a.fkHelperId  equals h.Id
                          join o in Db.HomeNursing on a.fkNursingId equals o.Id
                          select new
                          {
                              HelperName     = h.Name,
                              AttendanceId   = a.Id,
                              Time           = a.Time,
                              fkHelperId     = a.fkHelperId,
                              fknursing      = a.fkNursingId,
                              AttendanceDate = a.Date,
                              AttendanceTime = a.Time,
                              Description    = a.Description,
                              PatientName    = o.PatientName,
                              // GPS check-in fields
                              CheckInTime    = a.CheckInTime,
                              Latitude       = a.Latitude,
                              Longitude      = a.Longitude,
                              GpsAccuracy    = a.GpsAccuracy,
                              Address        = a.Address,
                              Status         = a.Status
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
                {
                    return false;
                }

                existingAttendance.fkHelperId  = updatedAttendanceData.fkHelperId;
                existingAttendance.fkNursing   = updatedAttendanceData.fkNursing;
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
                {
                    return false;
                }

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
