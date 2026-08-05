using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;
using System.ComponentModel.Design;
using System.Text.Json;

namespace NursingHome.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly IAttedanceService _DbConn;
        private readonly IUserService      _userService;
        private readonly IHelpers          _helpers;

        // Shared HttpClient for reverse-geocoding (Nominatim). Static to avoid
        // socket exhaustion — HttpClient is thread-safe.
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

        public AttendanceController(IAttedanceService Db, IUserService userService, IHelpers helpers)
        {
            _DbConn      = Db;
            _userService = userService;
            _helpers     = helpers;
        }

        // ── Page ─────────────────────────────────────────────────────────────
        public IActionResult Attendance() => View();

        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        // ── Existing admin add/edit (unchanged) ───────────────────────────────
        public IActionResult AddandUpdateAttendance(Attendance attendance)
        {
            if (attendance == null)
                return BadRequest("Attendance data cannot be null.");

            if (attendance.Id == 0 || attendance.Id == null)
            {
                var result = _DbConn.AddAttendance(attendance);
                return result ? Ok("Attendance added successfully.") : StatusCode(500, "Error adding attendance.");
            }
            else
            {
                var result = _DbConn.UpdateAttendance(attendance);
                return result ? Ok("Attendance updated successfully.") : NotFound("Attendance record not found for update.");
            }
        }

        public IActionResult GetAttendanceData(int userId = 0)
        {
            int? helperIdFilter = null;
            if (userId > 0)
            {
                var user = _userService.GetUserDataById(userId);
                if (user != null && !string.Equals(user.Roles, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    // Non-admin: only show attendance for the helper assigned to them
                    var myHelpers = _helpers.GetData(user.UserName ?? "");
                    if (myHelpers.Count > 0)
                        helperIdFilter = myHelpers[0].Id;
                    else
                        return Json(new { data = new List<object>() }); // no helper assigned — empty
                }
            }
            var data = _DbConn.GetHelperAttendance(helperIdFilter);
            return Json(new { data });
        }

        public IActionResult DeleteAttendence(int id)
        {
            var IsDeleted = _DbConn.DeleteAttendance(id);
            return Json(IsDeleted);
        }

        public IActionResult GetHelpers(int userId = 0)
        {
            if (userId > 0)
            {
                var user = _userService.GetUserDataById(userId);
                if (user != null && !string.Equals(user.Roles, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    // Non-admin: only return the helper assigned to them
                    var myHelpers = _helpers.GetData(user.UserName ?? "")
                                            .Select(h => new { id = h.Id, name = h.Name })
                                            .ToList<object>();
                    return Json(new { data = myHelpers, isAdmin = false });
                }
            }
            return Json(new { data = _DbConn.GetHelpers(), isAdmin = true });
        }
        public IActionResult GetPatientDetails()  => Json(new { data = _DbConn.PatientDetails() });

        // ── GPS Check-In ──────────────────────────────────────────────────────
        /// <summary>
        /// Server records the timestamp; GPS coords come from the browser.
        /// Validates that the caller is a known user. Status is set to
        /// 'Pending Approval' — no manual time entry allowed.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CheckIn(
            int    userId,
            int    fkHelperId,
            int    fkNursingId,
            double latitude,
            double longitude,
            double gpsAccuracy,
            string description = "")
        {
            // Server-side: verify the caller is a valid user.
            var authError = RequireValidUser(userId, out _);
            if (authError != null) return authError;

            // Non-admin helpers may only check in under their own assigned helper record.
            var callerUser = _userService.GetUserDataById(userId);
            if (callerUser != null && !string.Equals(callerUser.Roles, "admin", StringComparison.OrdinalIgnoreCase))
            {
                var assignedHelpers = _helpers.GetData(callerUser.UserName ?? "");
                var assignedIds     = assignedHelpers.Select(h => h.Id).ToHashSet();
                if (!assignedIds.Contains(fkHelperId))
                    return StatusCode(403, new { message = "You are not authorised to check in on behalf of another helper." });
            }

            var checkInTime = DateTime.Now;
            var address     = await ReverseGeocodeAsync(latitude, longitude);

            var checkIn = new Attendance
            {
                fkHelperId  = fkHelperId,
                fkNursingId = fkNursingId,
                CheckInTime = checkInTime,
                Date        = checkInTime.Date,
                Latitude    = latitude,
                Longitude   = longitude,
                GpsAccuracy = gpsAccuracy,
                Address     = address,
                Status      = "Pending Approval",
                Description = description
            };

            var success = _DbConn.RecordCheckIn(checkIn);
            if (!success)
                return StatusCode(500, new { message = "Error recording check-in. Please try again." });

            return Ok(new
            {
                message     = "Checked in successfully.",
                checkInTime = checkInTime.ToString("dd MMM yyyy, hh:mm:ss tt"),
                address,
                status      = "Pending Approval"
            });
        }

        // ── GPS Check-Out ─────────────────────────────────────────────────────
        /// <summary>
        /// Server records the timestamp. Validates that the caller is a known
        /// user. Status remains 'Pending Approval' until manager review.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CheckOut(
            int    userId,
            int    attendanceId,
            double latitude,
            double longitude,
            double gpsAccuracy)
        {
            // Verify the caller is a valid user.
            var authError = RequireValidUser(userId, out _);
            if (authError != null) return authError;

            var checkOutTime = DateTime.Now;
            var address      = await ReverseGeocodeAsync(latitude, longitude);

            var success = _DbConn.RecordCheckOut(attendanceId, checkOutTime,
                                                 latitude, longitude, gpsAccuracy, address);
            if (!success)
                return StatusCode(500, new { message = "Error recording check-out. The record may already have a check-out or could not be found." });

            return Ok(new
            {
                message      = "Checked out successfully.",
                checkOutTime = checkOutTime.ToString("dd MMM yyyy, hh:mm:ss tt"),
                address
            });
        }

        // ── Manager Approval ──────────────────────────────────────────────────

        /// <summary>
        /// Returns pending records for manager review.
        /// Requires the caller to be an admin user (validated server-side).
        /// </summary>
        [HttpGet]
        public IActionResult GetPendingAttendance(int userId)
        {
            var authError = RequireAdmin(userId, out _);
            if (authError != null) return authError;

            var data = _DbConn.GetPendingAttendance();
            return Json(new { data });
        }

        /// <summary>
        /// Approves a pending record. Calculates total working hours.
        /// Requires admin role. approvedBy is resolved server-side from userId.
        /// </summary>
        [HttpPost]
        public IActionResult ApproveAttendance(int id, int userId, string remarks = "")
        {
            var authError = RequireAdmin(userId, out var approvedBy);
            if (authError != null) return authError;

            var success = _DbConn.ApproveAttendance(id, approvedBy, remarks);
            return Json(new
            {
                success,
                message = success
                    ? "Attendance approved and working hours calculated."
                    : "Failed to approve. Record may already be reviewed or not found."
            });
        }

        /// <summary>
        /// Rejects a pending record with optional remarks.
        /// Requires admin role. approvedBy is resolved server-side from userId.
        /// </summary>
        [HttpPost]
        public IActionResult RejectAttendance(int id, int userId, string remarks = "")
        {
            var authError = RequireAdmin(userId, out var approvedBy);
            if (authError != null) return authError;

            var success = _DbConn.RejectAttendance(id, approvedBy, remarks);
            return Json(new
            {
                success,
                message = success
                    ? "Attendance rejected."
                    : "Failed to reject. Record may already be reviewed or not found."
            });
        }

        // ── Server-side authorization helpers ─────────────────────────────────

        /// <summary>
        /// Verifies that the userId maps to an existing user.
        /// Returns null on success; an error IActionResult on failure.
        /// </summary>
        private IActionResult? RequireValidUser(int userId, out string userName)
        {
            userName = string.Empty;
            if (userId <= 0)
                return StatusCode(401, new { message = "Unauthorized: missing user session." });

            var user = _userService.GetUserDataById(userId);
            if (user == null)
                return StatusCode(401, new { message = "Unauthorized: user not found." });

            userName = user.UserName ?? "user";
            return null;
        }

        /// <summary>
        /// Verifies that the userId maps to an existing user with the 'admin' role.
        /// Returns null on success; an error IActionResult on failure.
        /// </summary>
        private IActionResult? RequireAdmin(int userId, out string userName)
        {
            var baseError = RequireValidUser(userId, out userName);
            if (baseError != null) return baseError;

            var user = _userService.GetUserDataById(userId);
            if (user == null || !string.Equals(user.Roles, "admin", StringComparison.OrdinalIgnoreCase))
                return StatusCode(403, new { message = "Forbidden: admin access required." });

            userName = user.UserName ?? "admin";
            return null;
        }

        // ── Nominatim reverse geocoding ───────────────────────────────────────
        private static async Task<string> ReverseGeocodeAsync(double lat, double lon)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"https://nominatim.openstreetmap.org/reverse?lat={lat:F6}&lon={lon:F6}&format=json");
                request.Headers.TryAddWithoutValidation("User-Agent", "NursingHomeApp/1.0");

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return CoordFallback(lat, lon);

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("display_name", out var displayName))
                {
                    var addr = displayName.GetString();
                    return string.IsNullOrWhiteSpace(addr) ? CoordFallback(lat, lon) : addr;
                }
                return CoordFallback(lat, lon);
            }
            catch
            {
                return CoordFallback(lat, lon);
            }
        }

        private static string CoordFallback(double lat, double lon) => $"Lat: {lat:F5}, Lon: {lon:F5}";
    }
}
