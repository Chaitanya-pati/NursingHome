using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;

namespace NursingHome.Controllers
{
    public class HelpersController : Controller
    {
        private readonly IHomeService _logger;
        private readonly IHelpers    _DbConn;
        private readonly IUserService _userService;

        public HelpersController(IHomeService logger, IHelpers Db, IUserService userService)
        {
            _logger      = logger;
            _DbConn      = Db;
            _userService = userService;
        }

        // ── Auth helpers ───────────────────────────────────────────────────────

        private IActionResult? RequireValidUser(int userId)
        {
            if (userId <= 0)
                return StatusCode(401, new { message = "Unauthorized: missing user session." });
            var user = _userService.GetUserDataById(userId);
            if (user == null)
                return StatusCode(401, new { message = "Unauthorized: user not found." });
            return null;
        }

        private IActionResult? RequireAdmin(int userId)
        {
            if (userId <= 0)
                return StatusCode(401, new { message = "Unauthorized: missing user session." });
            var user = _userService.GetUserDataById(userId);
            if (user == null)
                return StatusCode(401, new { message = "Unauthorized: user not found." });
            if (!string.Equals(user.Roles, "admin", StringComparison.OrdinalIgnoreCase))
                return StatusCode(403, new { message = "Forbidden: admin access required." });
            return null;
        }

        private string AdminUsername(int userId)
            => _userService.GetUserDataById(userId)?.UserName ?? "unknown";

        // ── Standard CRUD ─────────────────────────────────────────────────────

        public IActionResult Helpers()
        {
            try { return View(); }
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
                bool result = helperData.Id == 0
                    ? _DbConn.AddData(helperData)
                    : _DbConn.UpdateData(helperData);
                return Json(result);
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

        // ── ID Card ───────────────────────────────────────────────────────────

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

        public IActionResult GetHelperById(int id, int userId)
        {
            try
            {
                var authError = RequireValidUser(userId);
                if (authError != null) return authError;

                var h = _DbConn.GetData("admin").FirstOrDefault(x => x.Id == id);
                if (h == null) return Json(null);

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

        // ── Assign User: get user list ────────────────────────────────────────

        public IActionResult GetUsersForAssign(int userId, int helperId = 0)
        {
            try
            {
                var authError = RequireAdmin(userId);
                if (authError != null) return authError;

                var allHelpers = _DbConn.GetData("admin");

                // Find the suser currently assigned to THIS helper (allowed to re-select)
                var thisHelperSuser = allHelpers
                    .FirstOrDefault(h => h.Id == helperId)?.suser ?? "";

                // Collect susers assigned to OTHER helpers — these are blocked
                var assignedElsewhere = allHelpers
                    .Where(h => h.Id != helperId && !string.IsNullOrWhiteSpace(h.suser))
                    .Select(h => h.suser)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var users = _userService.GetData().Select(u => new {
                    id                   = u.Id,
                    fullName             = (u.FirstName + " " + u.LastName).Trim(),
                    userName             = u.UserName,
                    role                 = u.Roles,
                    mobileNo             = u.MobileNo,
                    isAssignedElsewhere  = assignedElsewhere.Contains(u.UserName ?? ""),
                    isThisHelper         = string.Equals(u.UserName, thisHelperSuser, StringComparison.OrdinalIgnoreCase),
                    isActive             = u.IsActive,
                    createdDate          = u.CreatedDate
                });

                return Json(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "GetUsersForAssign", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // ── Get assigned user info ────────────────────────────────────────────

        public IActionResult GetAssignedUserInfo(int helperId, int userId)
        {
            try
            {
                var authError = RequireValidUser(userId);
                if (authError != null) return authError;

                var helper = _DbConn.GetData("admin").FirstOrDefault(h => h.Id == helperId);
                if (helper == null)
                    return Json(new { success = false, message = "Helper not found." });

                if (string.IsNullOrWhiteSpace(helper.suser))
                    return Json(new { success = true, assigned = false });

                var user = _userService.GetUserByUsername(helper.suser);
                if (user == null)
                    return Json(new { success = true, assigned = false });

                return Json(new {
                    success    = true,
                    assigned   = true,
                    id         = user.Id,
                    fullName   = (user.FirstName + " " + user.LastName).Trim(),
                    userName   = user.UserName,
                    role       = user.Roles,
                    isActive   = user.IsActive,
                    lastLogin  = user.LastLogin,
                    createdDate = user.CreatedDate,
                    mobileNo   = user.MobileNo,
                    email      = user.Email
                });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "GetAssignedUserInfo", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // ── Assign existing user ──────────────────────────────────────────────

        public IActionResult AssignUserToHelper(int helperId, int targetUserId, int userId)
        {
            try
            {
                var authError = RequireAdmin(userId);
                if (authError != null) return authError;

                var targetUser = _userService.GetUserDataById(targetUserId);
                if (targetUser == null)
                    return Json(new { success = false, message = "Selected user not found." });

                // Prevent assigning a user already assigned to another helper
                var allHelpers = _DbConn.GetData("admin");
                var alreadyAssigned = allHelpers.Any(h =>
                    h.Id != helperId &&
                    string.Equals(h.suser, targetUser.UserName, StringComparison.OrdinalIgnoreCase));

                if (alreadyAssigned)
                    return Json(new { success = false, message = $"User '{targetUser.UserName}' is already assigned to another helper." });

                // Record what the old assignment was (for history)
                var helper = allHelpers.FirstOrDefault(h => h.Id == helperId);
                var oldUser = helper?.suser;
                var action  = string.IsNullOrWhiteSpace(oldUser) ? "Assigned" : "Changed";

                var result = _DbConn.AssignUser(helperId, targetUser.UserName);
                if (!result)
                    return Json(new { success = false, message = "Failed to assign user." });

                // Audit log
                _DbConn.RecordAssignmentHistory(new HelperUserAssignmentHistory
                {
                    HelperId         = helperId,
                    AssignedUserName = targetUser.UserName,
                    Action           = action,
                    AssignedBy       = AdminUsername(userId),
                    AssignedDate     = DateTime.Now,
                    Notes            = oldUser != null ? $"Previous user: {oldUser}" : null
                });

                return Json(new { success = true, message = "User assigned successfully." });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "AssignUserToHelper", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // ── Create new user and assign ────────────────────────────────────────

        public IActionResult CreateAndAssignUser(
            int    helperId,
            string firstName,
            string lastName,
            string userName,
            string password,
            string mobileNo,
            string email,
            string role,
            bool   isActive,
            int    userId)
        {
            try
            {
                var authError = RequireAdmin(userId);
                if (authError != null) return authError;

                // Prevent assigning a user that is already suser of another helper
                var allHelpers     = _DbConn.GetData("admin");
                var allSusers      = allHelpers
                    .Where(h => !string.IsNullOrWhiteSpace(h.suser))
                    .Select(h => h.suser)
                    .ToList();

                // Build user object
                var newUser = new Users
                {
                    FirstName = firstName?.Trim() ?? "",
                    LastName  = lastName?.Trim() ?? "",
                    UserName  = userName?.Trim() ?? "",
                    Password  = password,
                    Roles     = string.IsNullOrWhiteSpace(role) ? "helper" : role.Trim(),
                    MobileNo  = mobileNo?.Trim(),
                    Email     = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                    IsActive  = isActive
                };

                var (success, error) = _userService.CreateUserWithValidation(newUser, null, allSusers);
                if (!success)
                    return Json(new { success = false, message = error });

                // Verify the helper exists before assigning
                var helper = allHelpers.FirstOrDefault(h => h.Id == helperId);
                if (helper == null)
                {
                    // User was created but helper not found — still report failure for assignment
                    return Json(new { success = false, message = $"Helper #{helperId} not found. User was created but not assigned." });
                }

                var oldUser = helper.suser;
                var action  = string.IsNullOrWhiteSpace(oldUser) ? "Assigned" : "Changed";

                // Verify AssignUser actually succeeded
                var assigned = _DbConn.AssignUser(helperId, newUser.UserName);
                if (!assigned)
                    return Json(new { success = false, message = "User was created but could not be assigned to the helper. Please use 'Select Existing User' to assign the newly created user." });

                // Audit
                _DbConn.RecordAssignmentHistory(new HelperUserAssignmentHistory
                {
                    HelperId         = helperId,
                    AssignedUserName = newUser.UserName,
                    Action           = action,
                    AssignedBy       = AdminUsername(userId),
                    AssignedDate     = DateTime.Now,
                    Notes            = !string.IsNullOrWhiteSpace(oldUser) ? $"Created new user; Previous: {oldUser}" : "Created new user"
                });

                return Json(new { success = true, message = "User created and assigned successfully." });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "CreateAndAssignUser", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // ── Clear all user assignments ────────────────────────────────────────

        public IActionResult ClearAllUserAssignments(int userId)
        {
            try
            {
                var authError = RequireAdmin(userId);
                if (authError != null) return authError;

                var result = _DbConn.ClearAllUserAssignments();
                if (!result)
                    return Json(new { success = false, message = "Failed to clear assignments." });

                // Audit — one entry to mark the bulk clear
                _DbConn.RecordAssignmentHistory(new HelperUserAssignmentHistory
                {
                    HelperId         = 0,
                    AssignedUserName = "(all)",
                    Action           = "Removed",
                    RemovedBy        = AdminUsername(userId),
                    RemovedDate      = DateTime.Now,
                    Notes            = "Bulk clear: all helper user assignments removed by admin"
                });

                return Json(new { success = true, message = "All user assignments have been removed from helpers." });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "ClearAllUserAssignments", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // ── Remove assignment ─────────────────────────────────────────────────

        public IActionResult RemoveUserAssignment(int helperId, int userId)
        {
            try
            {
                var authError = RequireAdmin(userId);
                if (authError != null) return authError;

                var helper = _DbConn.GetData("admin").FirstOrDefault(h => h.Id == helperId);
                if (helper == null)
                    return Json(new { success = false, message = "Helper not found." });

                var oldUser = helper.suser;
                if (string.IsNullOrWhiteSpace(oldUser))
                    return Json(new { success = false, message = "No user is currently assigned to this helper." });

                _DbConn.RemoveUserAssignment(helperId);

                // Audit
                _DbConn.RecordAssignmentHistory(new HelperUserAssignmentHistory
                {
                    HelperId         = helperId,
                    AssignedUserName = oldUser,
                    Action           = "Removed",
                    RemovedBy        = AdminUsername(userId),
                    RemovedDate      = DateTime.Now
                });

                return Json(new { success = true, message = "User assignment removed." });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "RemoveUserAssignment", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // ── Reset password ────────────────────────────────────────────────────

        public IActionResult ResetHelperPassword(int helperId, string newPassword, int userId)
        {
            try
            {
                var authError = RequireAdmin(userId);
                if (authError != null) return authError;

                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
                    return Json(new { success = false, message = "Password must be at least 4 characters." });

                var helper = _DbConn.GetData("admin").FirstOrDefault(h => h.Id == helperId);
                if (helper == null)
                    return Json(new { success = false, message = "Helper not found." });

                if (string.IsNullOrWhiteSpace(helper.suser))
                    return Json(new { success = false, message = "No user is assigned to this helper." });

                var result = _userService.UpdatePassword(helper.suser, newPassword);
                if (!result)
                    return Json(new { success = false, message = "Password reset failed." });

                // Audit
                _DbConn.RecordAssignmentHistory(new HelperUserAssignmentHistory
                {
                    HelperId         = helperId,
                    AssignedUserName = helper.suser,
                    Action           = "PasswordReset",
                    AssignedBy       = AdminUsername(userId),
                    AssignedDate     = DateTime.Now,
                    Notes            = "Password reset by admin"
                });

                return Json(new { success = true, message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "ResetHelperPassword", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // ── Get assignment history ────────────────────────────────────────────

        public IActionResult GetAssignmentHistory(int helperId, int userId)
        {
            try
            {
                var authError = RequireAdmin(userId);
                if (authError != null) return authError;

                var history = _DbConn.GetAssignmentHistory(helperId);
                var result  = history.Select(h => new {
                    action          = h.Action,
                    assignedUserName = h.AssignedUserName,
                    assignedBy      = h.AssignedBy,
                    assignedDate    = h.AssignedDate,
                    removedBy       = h.RemovedBy,
                    removedDate     = h.RemovedDate,
                    notes           = h.Notes
                });
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("HelpersController", "GetAssignmentHistory", ex.Message);
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
