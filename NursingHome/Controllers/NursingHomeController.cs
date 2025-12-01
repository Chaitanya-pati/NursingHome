using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;

namespace NursingHome.Controllers
{
    public class NursingHomeController : Controller
    {
        private readonly IHomeService _logger;
        
        private readonly INursingHome _DbConn;

        public NursingHomeController( IHomeService logger, INursingHome Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.SaveLog("NursingHomeController", "Index", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult NursingHome()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.SaveLog("NursingHomeController", "NursingHome", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult AddorEditHomeNursing(Db.Models.HomeNursing homeNursing)
        {
            try
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
            catch (Exception ex)
            {
                _logger.SaveLog("NursingHomeController", "AddorEditHomeNursing", ex.Message);
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
                _logger.SaveLog("NursingHomeController", "GetData", ex.Message);
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
                _logger.SaveLog("NursingHomeController", "Privacy", ex.Message);
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
                _logger.SaveLog("NursingHomeController", "GetLatestID", ex.Message);
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
                _logger.SaveLog("NursingHomeController", "UpdateClientSignature", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public IActionResult ClientSignature(int id)
        {
            try
            {
                var data = _DbConn.GetPatientData(id);
               if (data == null)
               return NotFound();
               return View("ClientSignatureDetails", data);
            }
            catch (Exception ex)
            {
                _logger.SaveLog("NursingHomeController", "ClientSignature", ex.Message);
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
                _logger.SaveLog("NursingHomeController", "Error", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
