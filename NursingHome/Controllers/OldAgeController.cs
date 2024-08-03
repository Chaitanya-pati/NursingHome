using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;
namespace NursingHome.Controllers
{
    public class OldAgeController : Controller
    {
        private readonly ILogger<OldAgeController> _logger;
        private readonly IOldAge _DbConn;
        public OldAgeController(ILogger<OldAgeController> logger,IOldAge Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        public IActionResult OldAge()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        } 

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult AddOldAge(Db.Models.OldAge oldAge)
        {
            var IsAdded = _DbConn.AddData(oldAge);
            return Json(IsAdded);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}