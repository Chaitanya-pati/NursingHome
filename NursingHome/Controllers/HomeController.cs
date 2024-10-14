using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;
using NursingHome.Db.ViewModel;

namespace NursingHome.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _DbConn;

        public HomeController(ILogger<HomeController> logger, IHomeService db)
        {
            _logger = logger;
            _DbConn = db;
        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _DbConn.SaveLog("HomeController", "Index", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult Reports()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _DbConn.SaveLog("HomeController", "Reports", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult GetLast30DaysRecords()
        {
            try
            {
                var oldageData = _DbConn.TotalOldAgeAdmissionLast30days();
                var NursingData = _DbConn.TotalHomeNursingAdmissionLast30days();
                var HelperData = _DbConn.TotalHelpersAdded30days();

                return Json(new { oldeAge = oldageData, nursingHome = NursingData, helper = HelperData });
            }
            catch (Exception ex)
            {
                _DbConn.SaveLog("HomeController", "GetLast30DaysRecords", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult GetAdmissionData(DateTime startDate, DateTime endDate)
        {
            try
            {
                var admissionCounts = new AdmissionCounts
                {
                    OldAge = _DbConn.GetOldAgeCounts(startDate, endDate),
                    NursingHome = _DbConn.GetNursingHomeCounts(startDate, endDate),
                    Helpers = _DbConn.GetHelpersCounts(startDate, endDate),
                };

                return Json(admissionCounts);
            }
            catch (Exception ex)
            {
                _DbConn.SaveLog("HomeController", "GetAdmissionData", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            try
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
            catch (Exception ex)
            {
                _DbConn.SaveLog("HomeController", "Error", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
