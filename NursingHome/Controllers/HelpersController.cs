using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;

namespace NursingHome.Controllers
{
    public class HelpersController : Controller
    {
        private readonly ILogger<HelpersController> _logger;
        private readonly IService _DbConn;
        public HelpersController(ILogger<HelpersController> logger,IService Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        public IActionResult Helpers()
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