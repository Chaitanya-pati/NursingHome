using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;

namespace NursingHome.Controllers
{
    public class UsersController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserService _DbConn;
        public UsersController(ILogger<HomeController> logger, IUserService dbConn)
        {
            _logger = logger;
            _DbConn = dbConn;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Users()
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