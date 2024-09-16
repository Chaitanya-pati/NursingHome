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
                    fkHelperId = AddAttendenceData.fkHelperId,
                    fkNursingId = AddAttendenceData.fkNursingId, // Ensure this is correctly named
                    Date = AddAttendenceData.Date,
                    Time = AddAttendenceData.Time, // Store as double (total hours) if needed
                    Description = AddAttendenceData.Description
                };

                Db.Attendance.Add(attendance);
                Db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                // Console.WriteLine(ex.Message);
                return false;
            }
        }

        public List<object> GetHelperAttendance()
        {
            using var Db = new TaskContext(_dbConn);
            var result = (from a in Db.Attendance
                          join h in Db.Helpers on a.fkHelperId equals h.Id
                          join o in Db.HomeNursing on a.fkNursingId equals o.Id
                          select new
                          {
                              HelperName = h.Name,
                              AttendanceId = a.Id,
                              Time=a.Time,
                              fkHelperId = a.fkHelperId,
                              fknursing = a.fkNursingId,
                              AttendanceDate = a.Date,
                              AttendanceTime = a.Time,
                              Description = a.Description,
                              PatientName = o.PatientName
                          }).ToList<object>();

            return result;
        }
        public bool UpdateAttendance(Attendance updatedAttendanceData)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);

                // Find the existing attendance record by Id
                var existingAttendance = Db.Attendance.FirstOrDefault(a => a.Id == updatedAttendanceData.Id);

                if (existingAttendance == null)
                {
                    // Attendance record not found
                    return false;
                }

                // Update the fields with the new data
                existingAttendance.fkHelperId = updatedAttendanceData.fkHelperId;
                existingAttendance.fkNursing = updatedAttendanceData.fkNursing;
                existingAttendance.Date = updatedAttendanceData.Date;
                existingAttendance.Time = updatedAttendanceData.Time;
                existingAttendance.Description = updatedAttendanceData.Description;

                // Save the changes to the database
                Db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                // Console.WriteLine(ex.Message);
                return false;
            }
        }
        public bool DeleteAttendance(int id)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);

                // Find the existing attendance record by Id
                var existingAttendance = Db.Attendance.FirstOrDefault(a => a.Id == id);

                if (existingAttendance == null)
                {
                    // Attendance record not found
                    return false;
                }

                // Remove the attendance record
                Db.Attendance.Remove(existingAttendance);

                // Save the changes to the database
                Db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                // Console.WriteLine(ex.Message);
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

            return result.Cast<object>().ToList();  // Returning as anonymous type
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

            return result.Cast<object>().ToList(); // Returns an anonymous type list
        }


    }
}
