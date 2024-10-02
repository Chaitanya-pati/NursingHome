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
        public HomeController(ILogger<HomeController> logger,IHomeService db)
        {
            _logger = logger;
            _DbConn = db;
        }

        public IActionResult Index()
        {
            return View();
        } 

        public IActionResult Reports()
        {
            return View();  
        }
        public IActionResult GetLast30DaysRecords()
        {
            var oldageData = _DbConn.TotalOldAgeAdmissionLast30days();
            var NursingData = _DbConn.TotalHomeNursingAdmissionLast30days();
            var HelperData = _DbConn.TotalHelpersAdded30days();

            return Json(new { oldeAge = oldageData, nursingHome = NursingData, helper = HelperData });
        }
        public IActionResult GetAdmissionData(DateTime startDate, DateTime endDate)
        {
            var admissionCounts = new AdmissionCounts
            {
                OldAge = _DbConn.GetOldAgeCounts(startDate, endDate),
                NursingHome = _DbConn.GetNursingHomeCounts(startDate, endDate),
                Helpers = _DbConn.GetHelpersCounts(startDate, endDate),
            };

            return Json(admissionCounts);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}