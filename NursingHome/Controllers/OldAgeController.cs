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
        private readonly IHomeService _logger;  // Updated to IHomeService for SaveLog
        private readonly IOldAge _DbConn;

        public OldAgeController(IHomeService logger, IOldAge Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        public IActionResult OldAge()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.SaveLog("OldAgeController", "OldAge", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.SaveLog("OldAgeController", "Index", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult Privacy()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.SaveLog("OldAgeController", "Privacy", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult AddorEditOldAge(Db.Models.OldAge oldAge)
        {
            try
            {
                if (oldAge.Id == 0)
                {
                    var IsAdded = _DbConn.AddData(oldAge);
                    return Json(IsAdded);
                }
                else
                {
                    var IsUpdated = _DbConn.UpdateData(oldAge);
                    return Json(IsUpdated);
                }
            }
            catch (Exception ex)
            {
                _logger.SaveLog("OldAgeController", "AddorEditOldAge", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult GetData(DateTime startDate, DateTime endDate, string username)
        {
            try
            {
                var result = _DbConn.GetData(startDate, endDate, username);
                return Json(new { data = result });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("OldAgeController", "GetData", ex.Message);
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
                _logger.SaveLog("OldAgeController", "DeleteData", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult GetLatestID()
        {
            try
            {
                return Json(_DbConn.getLatestID());
            }
            catch (Exception ex)
            {
                _logger.SaveLog("OldAgeController", "GetLatestID", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public IActionResult UpdateClientSignature(int id, string signature)
        {
            try
            {
                var result = _DbConn.UpdateClientSignature(id, signature);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.SaveLog("OldAgeController", "UpdateClientSignature", ex.Message);
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
                _logger.SaveLog("OldAgeController", "Error", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
