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
        private readonly IUserService _userService;

        public HelpersController(IHomeService logger, IHelpers Db, IUserService userService)
        {
            _logger      = logger;
            _DbConn      = Db;
            _userService = userService;
        }

        /// <summary>
        /// Verifies that userId maps to a real database user (same pattern as AttendanceController).
        /// Returns null on success; a 401 IActionResult on failure.
        /// </summary>
        private IActionResult? RequireValidUser(int userId)
        {
            if (userId <= 0)
                return StatusCode(401, new { message = "Unauthorized: missing user session." });
            var user = _userService.GetUserDataById(userId);
            if (user == null)
                return StatusCode(401, new { message = "Unauthorized: user not found." });
            return null;
        }

        public IActionResult Helpers()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "Helpers", ex.Message);
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

        public IActionResult GetData(string UserName = "")
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

        /// <summary>
        /// Renders the ID card page. Requires a valid userId from the client session.
        /// </summary>
        public IActionResult IdCard(int id, int userId)
        {
            try
            {
                var authError = RequireValidUser(userId);
                if (authError != null) return authError;

                ViewBag.HelperId = id;
                ViewBag.UserId   = userId;
                return View();
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "IdCard", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Returns a safe DTO with the helper's details for ID card rendering.
        /// Requires a valid userId from the client session.
        /// </summary>
        public IActionResult GetHelperById(int id, int userId)
        {
            try
            {
                var authError = RequireValidUser(userId);
                if (authError != null) return authError;

                var h = _DbConn.GetData("admin").FirstOrDefault(x => x.Id == id);
                if (h == null) return Json(null);

                // Project to a DTO — keeps EF navigation properties out of the
                // JSON response and avoids circular-reference serialisation errors.
                return Json(new {
                    id               = h.Id,
                    name             = h.Name,
                    image            = h.Image,
                    dateOfBirth      = h.DateOfBirth,
                    mobileNo         = h.MobileNo,
                    designation      = h.Designation,
                    bloodGroup       = h.BloodGroup,
                    aadhaarNo        = h.AadhaarNo,
                    admissionDate    = h.admissionDate,
                    permanentAddress = h.PermanentAddress
                });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "GetHelperById", ex.Message);
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
