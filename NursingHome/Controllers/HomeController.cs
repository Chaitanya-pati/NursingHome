using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;

namespace NursingHome.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IService _DbConn;
        public HomeController(ILogger<HomeController> logger,IService Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        public IActionResult Index()
        {
            return View();
        } 
        public IActionResult OldAge()
        {
            _DbConn.AddData();
            return View();
        }
        public IActionResult HomeNursing()
        {
            return View();
        }
        public IActionResult Helpers()
        {
            return View();
        }
        
        public IActionResult Privacy()
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