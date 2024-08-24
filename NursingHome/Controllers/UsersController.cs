using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;
using Microsoft.EntityFrameworkCore.Query.Internal;

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

        public IActionResult AddorEditUser(Users userData)
        {
            if (userData.Id == 0)
            {
                var IsAdded = _DbConn.AddData(userData);
                return Json(IsAdded);
            }
            else
            {
                var IsUpdated = _DbConn.UpdateData(userData);
                return Json(IsUpdated);
            }
        }
        public IActionResult GetData()
        {
            var Data = _DbConn.GetData();
            return Json(new { data = Data });
        }
        public IActionResult DeleteUser(int id)
        {
            var IsDeleted = _DbConn.DeleteUser(id);
            return Json(IsDeleted);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}