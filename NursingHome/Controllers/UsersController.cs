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
        private readonly IHomeService _logger;
        private readonly IUserService _DbConn;

        public UsersController(IHomeService logger, IUserService dbConn)
        {
            _logger = logger;
            _DbConn = dbConn;
        }

        // ── Session auth helpers ───────────────────────────────────────────────

        /// <summary>
        /// Reads the authenticated user ID exclusively from the server-side session.
        /// Returns null on success; an error IActionResult on failure.
        /// </summary>
        private IActionResult? RequireValidUser(out int userId)
        {
            userId = 0;
            var sessionId = HttpContext.Session.GetInt32("UserId");
            if (sessionId == null || sessionId <= 0)
                return StatusCode(401, new { message = "Unauthorized: no active session. Please log in." });
            userId = sessionId.Value;
            var user = _DbConn.GetUserDataById(userId);
            if (user == null)
                return StatusCode(401, new { message = "Unauthorized: session user not found." });
            return null;
        }

        private IActionResult? RequireAdmin(out int userId)
        {
            var err = RequireValidUser(out userId);
            if (err != null) return err;
            var user = _DbConn.GetUserDataById(userId);
            if (user == null || !string.Equals(user.Roles, "admin", StringComparison.OrdinalIgnoreCase))
                return StatusCode(403, new { message = "Forbidden: admin access required." });
            return null;
        }

        // ── Pages ─────────────────────────────────────────────────────────────

        public IActionResult Privacy()
        {
            try { return View(); }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "Privacy", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult Users()
        {
            try { return View(); }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "Users", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // ── Admin-only CRUD ───────────────────────────────────────────────────

        [HttpPost]
        public IActionResult AddorEditUser([FromForm] Users userData)
        {
            try
            {
                var authErr = RequireAdmin(out _);
                if (authErr != null) return authErr;

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
                var authErr = RequireAdmin(out _);
                if (authErr != null) return authErr;

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
                var authErr = RequireAdmin(out _);
                if (authErr != null) return authErr;

                var IsDeleted = _DbConn.DeleteUser(id);
                return Json(IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "DeleteUser", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // ── Face recognition (requires valid session) ─────────────────────────

        public IActionResult GetFaceData(string username)
        {
            try
            {
                var authErr = RequireValidUser(out _);
                if (authErr != null) return authErr;

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
                var authErr = RequireValidUser(out _);
                if (authErr != null) return authErr;

                return Json(_DbConn.SaveFaceDescriptor(username, face));
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "SaveFaceDescriptor", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // ── Login (unauthenticated) ───────────────────────────────────────────

        public IActionResult Login()
        {
            try { return View(); }
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
                    // Establish server-side session — all subsequent authorization
                    // reads from this session, never from client-supplied parameters.
                    HttpContext.Session.SetInt32("UserId", user.Id);
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

        /// <summary>
        /// Returns the currently authenticated user's data derived from the
        /// server-side session. Used by the layout to set role/display info.
        /// No client-supplied ID is trusted.
        /// </summary>
        public IActionResult GetCurrentUser()
        {
            try
            {
                var sessionId = HttpContext.Session.GetInt32("UserId");
                if (sessionId == null)
                    return Json(null);
                var data = _DbConn.GetUserDataById(sessionId.Value);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "GetCurrentUser", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Returns user data for a given ID. Restricted:
        ///   - Admins may look up any user.
        ///   - Non-admins may only look up their own record.
        /// </summary>
        public IActionResult GetUserDataById(int id)
        {
            try
            {
                var authErr = RequireValidUser(out int sessionUserId);
                if (authErr != null) return authErr;

                // Non-admin may only read their own record.
                var caller = _DbConn.GetUserDataById(sessionUserId);
                bool isAdmin = string.Equals(caller?.Roles, "admin", StringComparison.OrdinalIgnoreCase);
                if (!isAdmin && id != sessionUserId)
                    return StatusCode(403, new { message = "Forbidden: you may only access your own user record." });

                var data = _DbConn.GetUserDataById(id);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "GetUserDataById", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Returns a suggested unique username derived from the given first name.
        /// Tries firstName → firstName2 → firstName3 … until one is not taken.
        /// Used by the "New User" panel in the Helpers view to pre-fill the username field.
        /// </summary>
        public IActionResult SuggestUsername(string firstName)
        {
            try
            {
                var authErr = RequireAdmin(out _);
                if (authErr != null) return authErr;

                if (string.IsNullOrWhiteSpace(firstName))
                    return Json(new { suggestion = string.Empty });

                var suggestion = _DbConn.SuggestUsername(firstName.Trim());
                return Json(new { suggestion });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("UsersController", "SuggestUsername", ex.Message);
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
