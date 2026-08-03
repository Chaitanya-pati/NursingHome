using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;

namespace NursingHome.Controllers
{
    public class UsersController : Controller
    {
        private readonly IHomeService _logger; // Changed to IHomeService for SaveLog functionality
        private readonly IUserService _DbConn;

        public UsersController(IHomeService logger, IUserService dbConn)
        {
            _logger = logger;
            _DbConn = dbConn;
        }

        public IActionResult Privacy()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "Privacy", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult Users()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "Users", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public IActionResult AddorEditUser([FromForm] Users userData)
        {
            try
            {
                _logger.SaveLog("UsersController", "AddorEditUser",
                    $"Request received — Id={userData.Id}, UserName={userData.UserName}, FirstName={userData.FirstName}");

                if (userData.Id == 0)
                {
                    _logger.SaveLog("UsersController", "AddorEditUser", "Mode: ADD new user");
                    var isAdded = _DbConn.AddData(userData);
                    _logger.SaveLog("UsersController", "AddorEditUser", $"AddData result: {isAdded}");
                    return Json(new { success = isAdded, message = isAdded ? "User added successfully." : "Failed to add user." });
                }
                else
                {
                    _logger.SaveLog("UsersController", "AddorEditUser", $"Mode: UPDATE user id={userData.Id}");
                    var isUpdated = _DbConn.UpdateData(userData);
                    _logger.SaveLog("UsersController", "AddorEditUser", $"UpdateData result: {isUpdated}");
                    return Json(new { success = isUpdated, message = isUpdated ? "User updated successfully." : "User not found or update failed." });
                }
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "AddorEditUser", $"EXCEPTION: {ex.Message} | StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "An error occurred while saving. Please try again." });
            }
        }

        public IActionResult GetData()
        {
            try
            {
                var Data = _DbConn.GetData();
                return Json(new { data = Data });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "GetData", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult DeleteUser(int id)
        {
            try
            {
                var IsDeleted = _DbConn.DeleteUser(id);
                return Json(IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "DeleteUser", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult GetFaceData(string username)
        {
            try
            {
                return Json(_DbConn.GetFaceDescriptor(username));
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "GetFaceData", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult SaveFaceDescriptor(string username, string face)
        {
            try
            {
                return Json(_DbConn.SaveFaceDescriptor(username, face));
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "SaveFaceDescriptor", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult Login()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "Login", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult LoginUser(string userName, string password)
        {
            try
            {
                var user = _DbConn.CheckValidUser(userName, password);

                if (user != null)
                {
                    return Json(new { success = true, userID = user.Id, isFaceAdded = user.IsFaceAdded });
                }
                else
                {
                    return Json(new { success = false, message = "Invalid username or password." });
                }
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "LoginUser", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult GetUserDataById(int id)
        {
            try
            {
                var data = _DbConn.GetUserDataById(id);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "GetUserDataById", ex.Message);
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
                _logger.SaveLog("UsersController", "Error", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
