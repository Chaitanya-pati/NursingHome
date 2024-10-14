using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;

namespace NursingHome.Controllers
{
    public class HelpersController : Controller
    {
        private readonly IHomeService _logger;
        private readonly IHelpers _DbConn;

        public HelpersController(IHomeService logger, IHelpers Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        public IActionResult Helpers()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "Helpers",ex.Message); // Assuming IHomeService has a LogError method
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult AddorEditHelper(Db.Models.Helpers helperData)
        {
            try
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
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "AddorEditHelper", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult GetData(string UserName)
        {
            try
            {
                var data = _DbConn.GetData(UserName);
                return Json(new { data = data });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "GetData", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult DeleteData(int id)
        {
            try
            {
                var isDelete = _DbConn.DeleteData(id);
                return Json(isDelete);
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "DeleteData", ex.Message);
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
                _logger.SaveLog("HelpersController", "Error", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
