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

            // Check if attendance.Id is 0 or null (indicating it's a new entry)
            if (attendance.Id == 0 || attendance.Id == null)
            {
                var result = _DbConn.AddAttendance(attendance);

                if (result)
                {
                    return Ok("Attendance added successfully.");
                }
                else
                {
                    return StatusCode(500, "Error adding attendance.");
                }
            }
            else
            {
                var result = _DbConn.UpdateAttendance(attendance);

                if (result)
                {
                    return Ok("Attendance updated successfully.");
                }
                else
                {
                    return NotFound("Attendance record not found for update.");
                }
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



    }
}