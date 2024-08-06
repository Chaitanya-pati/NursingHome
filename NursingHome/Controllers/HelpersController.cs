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
        private readonly IHelpers _DbConn;
        public HelpersController(ILogger<HelpersController> logger, IHelpers Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        public IActionResult Helpers()
        {
            return View();
        }

        public IActionResult AddorEditHelper(Db.Models.Helpers helperData)
        {
            if (helperData.Id == 0)
            {
                var isAdded = _DbConn.AddData(helperData);
                return Json(isAdded);
            }
            else
            {
                var isUpdated = _DbConn.UpdateData(helperData);
                return Json(isUpdated);
            }

        }

        public IActionResult GetData()
        {
            var data = _DbConn.GetData();
            return Json(new {data = data});
        }

        public IActionResult DeleteData(int id)
        {
            var isDelete = _DbConn.DeleteData(id);
            return Json(isDelete);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}