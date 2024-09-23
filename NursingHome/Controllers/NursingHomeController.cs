using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;

namespace NursingHome.Controllers
{
    public class NursingHomeController : Controller
    {
        private readonly ILogger<NursingHomeController> _logger;
        private readonly INursingHome _DbConn;
        public NursingHomeController(ILogger<NursingHomeController> logger,INursingHome Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult NursingHome()
        {
            return View();
        }

        public IActionResult AddorEditHomeNursing(Db.Models.HomeNursing homeNursing)
        {
            if (homeNursing.Id == 0)
            {
                var isAdded = _DbConn.AddData(homeNursing);
                return Json(isAdded);
            }
            else
            {
                var isUpdated = _DbConn.UpdateData(homeNursing);
                return Json(isUpdated);
            }
        }
        public IActionResult GetData(DateTime startDate, DateTime endDate, string username)
        {
            var result = _DbConn.GetData(startDate, endDate,username);
            return Json(new { data = result });
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult GetLatestID()
        {
            return Json(_DbConn.getLatestID());
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}