using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;

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


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}