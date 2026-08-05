using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;
using System.ComponentModel.Design;

namespace NursingHome.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly IAttedanceService _DbConn;
      
        public AttendanceController(IAttedanceService Db)
        {
            
            _DbConn = Db;
        }

        public IActionResult Attendance()
        {
            return View();
        }



        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult AddandUpdateAttendance(Attendance attendance)
        {
            if (attendance == null)
            {
                return BadRequest("Attendance data cannot be null.");
            }

            var sessionRole    = HttpContext.Session.GetString("UserRole") ?? "";
            var sessionUsername = HttpContext.Session.GetString("Username");
            var isAdmin        = sessionRole.Equals("admin", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(sessionUsername))
                return Unauthorized("Session expired. Please log in again.");

            // Resolve the session-caller's helper ID (null for admin users who are not helpers)
            int? callerHelperId = null;
            if (!isAdmin)
            {
                callerHelperId = _DbConn.GetHelperIdByUsername(sessionUsername);
                if (callerHelperId == null)
                    return BadRequest("No helper record linked to your account.");

                // Always use the server-derived helper ID — ignore whatever the client sent
                attendance.fkHelperId = callerHelperId;
            }

            // New record
            if (attendance.Id == 0 || attendance.Id == null)
            {
                var result = _DbConn.AddAttendance(attendance);
                return result ? Ok("Attendance added successfully.") : StatusCode(500, "Error adding attendance.");
            }
            else
            {
                // Update: non-admin must own the record they are editing
                if (!isAdmin)
                {
                    var ownerHelperId = _DbConn.GetAttendanceOwnerHelperId(attendance.Id);
                    if (ownerHelperId != callerHelperId)
                        return StatusCode(403, "You can only edit your own attendance records.");
                }

                var result = _DbConn.UpdateAttendance(attendance);
                return result ? Ok("Attendance updated successfully.") : NotFound("Attendance record not found for update.");
            }
        }



        public IActionResult GetAttendanceData()
        {
            var data = _DbConn.GetHelperAttendance();

            return Json(new
            {
                data = data,

            });
        }
        public IActionResult DeleteAttendence(int id)
        {
            var sessionRole    = HttpContext.Session.GetString("UserRole") ?? "";
            var sessionUsername = HttpContext.Session.GetString("Username");
            var isAdmin        = sessionRole.Equals("admin", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(sessionUsername))
                return Unauthorized("Session expired. Please log in again.");

            if (!isAdmin)
            {
                var callerHelperId = _DbConn.GetHelperIdByUsername(sessionUsername);
                var ownerHelperId  = _DbConn.GetAttendanceOwnerHelperId(id);
                if (callerHelperId == null || ownerHelperId != callerHelperId)
                    return StatusCode(403, "You can only delete your own attendance records.");
            }

            var IsDeleted = _DbConn.DeleteAttendance(id);
            return Json(IsDeleted);
        }
        public IActionResult GetHelpers()
        {
            var data = _DbConn.GetHelpers();

            return Json(new
            {
                data = data,

            });
        }
        public IActionResult GetPatientDetails()
        {
            var data = _DbConn.PatientDetails();

            return Json(new
            {
                data = data,

            });
        }

        public IActionResult GetHelperIdByUsername()
        {
            var sessionUsername = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(sessionUsername))
                return Unauthorized("Session expired. Please log in again.");

            var helperId = _DbConn.GetHelperIdByUsername(sessionUsername);
            return Json(new { helperId = helperId });
        }



    }
}