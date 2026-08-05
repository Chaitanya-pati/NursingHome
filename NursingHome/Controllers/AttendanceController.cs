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

        // ── Server-side authorization helpers ─────────────────────────────────

        /// <summary>
        /// Reads the authenticated user ID from the server-side session.
        /// Returns null + populates userId/userName on success;
        /// returns an error IActionResult on failure.
        /// Client-supplied userId parameters are NEVER trusted for authorization.
        /// </summary>
        private IActionResult? RequireValidUser(out int userId, out string userName)
        {
            userId   = 0;
            userName = string.Empty;

            var sessionId = HttpContext.Session.GetInt32("UserId");
            if (sessionId == null || sessionId <= 0)
                return StatusCode(401, new { message = "Unauthorized: no active session. Please log in." });

            userId = sessionId.Value;
            var user = _userService.GetUserDataById(userId);
            if (user == null)
                return StatusCode(401, new { message = "Unauthorized: session user not found." });

            userName = user.UserName ?? "user";
            return null;
        }

        /// <summary>
        /// Verifies that the session user has the 'admin' role.
        /// Returns null on success; an error IActionResult on failure.
        /// </summary>
        private IActionResult? RequireAdmin(out int userId, out string userName)
        {
            var baseError = RequireValidUser(out userId, out userName);
            if (baseError != null) return baseError;

            var user = _userService.GetUserDataById(userId);
            if (user == null || !string.Equals(user.Roles, "admin", StringComparison.OrdinalIgnoreCase))
                return StatusCode(403, new { message = "Forbidden: admin access required." });

            userName = user.UserName ?? "admin";
            return null;
        }

        /// <summary>
        /// Returns true if the session user is an admin.
        /// Requires a valid session (call RequireValidUser first).
        /// </summary>
        private bool SessionUserIsAdmin(int userId)
        {
            var user = _userService.GetUserDataById(userId);
            return user != null && string.Equals(user.Roles, "admin", StringComparison.OrdinalIgnoreCase);
        }

        // ── Existing admin add/edit ────────────────────────────────────────────
        public IActionResult AddandUpdateAttendance(Attendance attendance)
        {
            // Only admins may add or edit attendance records manually.
            var authError = RequireAdmin(out _, out _);
            if (authError != null) return authError;

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

        public IActionResult GetAttendanceData(string? startDate = null, string? endDate = null, int? filterHelperId = null)
        {
            // Identity comes from session — never from a client-supplied userId parameter.
            var authError = RequireValidUser(out int userId, out _);
            if (authError != null) return authError;

            int? helperIdFilter = null;

            if (!SessionUserIsAdmin(userId))
            {
                // Non-admin: only show attendance for the helper assigned to them.
                // The client-supplied filterHelperId is ignored for safety.
                var user      = _userService.GetUserDataById(userId);
                var myHelpers = _helpers.GetData(user?.UserName ?? "");
                if (myHelpers.Count > 0)
                    helperIdFilter = myHelpers[0].Id;
                else
                    return Json(new { data = new List<object>() }); // no helper assigned — empty
            }
            else if (filterHelperId.HasValue && filterHelperId.Value > 0)
            {
                // Admin with an explicit helper filter selected in the search bar.
                helperIdFilter = filterHelperId.Value;
            }

            DateTime? start = string.IsNullOrWhiteSpace(startDate) ? null : DateTime.TryParse(startDate, out var sd) ? sd : (DateTime?)null;
            DateTime? end   = string.IsNullOrWhiteSpace(endDate)   ? null : DateTime.TryParse(endDate,   out var ed) ? ed : (DateTime?)null;

            var data = _DbConn.GetHelperAttendance(helperIdFilter, start, end);
            return Json(new { data });
        }

        public IActionResult DeleteAttendence(int id)
        {
            // Only admins may delete attendance records.
            var authError = RequireAdmin(out _, out _);
            if (authError != null) return authError;

            var IsDeleted = _DbConn.DeleteAttendance(id);
            return Json(IsDeleted);
        }

        public IActionResult GetHelpers()
        {
            // Identity comes from session.
            var authError = RequireValidUser(out int userId, out _);
            if (authError != null) return authError;

            if (!SessionUserIsAdmin(userId))
            {
                // Non-admin: only return the helper assigned to them.
                var user      = _userService.GetUserDataById(userId);
                var myHelpers = _helpers.GetData(user?.UserName ?? "")
                                        .Select(h => new { id = h.Id, name = h.Name })
                                        .ToList<object>();
                return Json(new { data = myHelpers, isAdmin = false });
            }

            return Json(new { data = _DbConn.GetHelpers(), isAdmin = true });
        }

        public IActionResult GetPatientDetails(int? helperId = null)
        {
            var authError = RequireValidUser(out int userId, out _);
            if (authError != null) return authError;

            int? resolvedHelperId = null;

            if (!SessionUserIsAdmin(userId))
            {
                // Non-admin: always filter to the helper assigned to their account.
                // Client-supplied helperId is ignored for safety.
                var user      = _userService.GetUserDataById(userId);
                var myHelpers = _helpers.GetData(user?.UserName ?? "");
                if (myHelpers.Count == 0)
                    return Json(new { data = new List<object>() }); // no helper assigned — empty list

                resolvedHelperId = myHelpers[0].Id;
            }
            else if (helperId.HasValue && helperId.Value > 0)
            {
                // Admin selected a specific helper in the dropdown.
                resolvedHelperId = helperId;
            }
            // else admin with no filter → resolvedHelperId stays null → all patients returned

            return Json(new { data = _DbConn.PatientDetails(resolvedHelperId) });
        }

        // ── GPS Check-In ──────────────────────────────────────────────────────
        /// <summary>
        /// Server records the timestamp; GPS coords come from the browser.
        /// Identity is resolved from the server-side session.
        /// Status is set to 'Pending Approval' — no manual time entry allowed.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CheckIn(
            int    fkHelperId,
            int    fkNursingId,
            double latitude,
            double longitude,
            double gpsAccuracy,
            string description = "")
        {
            // Resolve identity from session — never from a client-supplied userId.
            var authError = RequireValidUser(out int userId, out _);
            if (authError != null) return authError;

            // Non-admin helpers may only check in under their own assigned helper record.
            if (!SessionUserIsAdmin(userId))
            {
                var callerUser     = _userService.GetUserDataById(userId);
                var assignedHelpers = _helpers.GetData(callerUser?.UserName ?? "");
                var assignedIds    = assignedHelpers.Select(h => h.Id).ToHashSet();
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
        /// Server records the timestamp. Identity is resolved from session.
        /// Verifies that the attendance record belongs to the caller's assigned helper.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CheckOut(
            int    attendanceId,
            double latitude,
            double longitude,
            double gpsAccuracy)
        {
            // Resolve identity from session.
            var authError = RequireValidUser(out int userId, out _);
            if (authError != null) return authError;

            // Non-admin: verify the attendance record belongs to their assigned helper.
            if (!SessionUserIsAdmin(userId))
            {
                var record = _DbConn.GetAttendanceById(attendanceId);
                if (record == null)
                    return NotFound(new { message = "Attendance record not found." });

                var callerUser      = _userService.GetUserDataById(userId);
                var assignedHelpers = _helpers.GetData(callerUser?.UserName ?? "");
                var assignedIds     = assignedHelpers.Select(h => h.Id).ToHashSet();

                if (!assignedIds.Contains(record.fkHelperId ?? 0))
                    return StatusCode(403, new { message = "You are not authorised to check out on behalf of another helper." });
            }

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
        /// Requires the caller to be an admin (validated server-side via session).
        /// </summary>
        [HttpGet]
        public IActionResult GetPendingAttendance()
        {
            var authError = RequireAdmin(out _, out _);
            if (authError != null) return authError;

            var data = _DbConn.GetPendingAttendance();
            return Json(new { data });
        }

        /// <summary>
        /// Approves a pending record. Calculates total working hours.
        /// Requires admin role. approvedBy is resolved server-side from session.
        /// </summary>
        [HttpPost]
        public IActionResult ApproveAttendance(int id, string remarks = "")
        {
            var authError = RequireAdmin(out _, out string approvedBy);
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
        /// Requires admin role. approvedBy is resolved server-side from session.
        /// </summary>
        [HttpPost]
        public IActionResult RejectAttendance(int id, string remarks = "")
        {
            var authError = RequireAdmin(out _, out string approvedBy);
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
