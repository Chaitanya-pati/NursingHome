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

        // Shared HttpClient for reverse-geocoding calls (Nominatim).
        // A single static instance is intentional — HttpClient is thread-safe
        // and reusing it avoids socket exhaustion.
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

        public AttendanceController(IAttedanceService Db)
        {
            _DbConn = Db;
        }

        public IActionResult Attendance()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AddandUpdateAttendance(Attendance attendance)
        {
            if (attendance == null)
            {
                return BadRequest("Attendance data cannot be null.");
            }

            if (attendance.Id == 0 || attendance.Id == null)
            {
                var result = _DbConn.AddAttendance(attendance);
                if (result)
                    return Ok("Attendance added successfully.");
                else
                    return StatusCode(500, "Error adding attendance.");
            }
            else
            {
                var result = _DbConn.UpdateAttendance(attendance);
                if (result)
                    return Ok("Attendance updated successfully.");
                else
                    return NotFound("Attendance record not found for update.");
            }
        }

        public IActionResult GetAttendanceData()
        {
            var data = _DbConn.GetHelperAttendance();
            return Json(new { data });
        }

        public IActionResult DeleteAttendence(int id)
        {
            var IsDeleted = _DbConn.DeleteAttendance(id);
            return Json(IsDeleted);
        }

        public IActionResult GetHelpers()
        {
            var data = _DbConn.GetHelpers();
            return Json(new { data });
        }

        public IActionResult GetPatientDetails()
        {
            var data = _DbConn.PatientDetails();
            return Json(new { data });
        }

        // ──────────────────────────────────────────────────────────────────────
        // GPS Check-In endpoint
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Receives GPS coordinates captured by the browser, records a server-
        /// side timestamp, performs reverse geocoding via Nominatim, and saves
        /// the attendance record with Status = "Pending Approval".
        /// No manual time entry: the timestamp is always set here on the server.
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
            // 1. Server-recorded timestamp — never trust the client for this.
            var checkInTime = DateTime.Now;

            // 2. Reverse geocode (best-effort; falls back to coordinates).
            var address = await ReverseGeocodeAsync(latitude, longitude);

            // 3. Build the attendance record.
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

        // ──────────────────────────────────────────────────────────────────────
        // Nominatim reverse geocoding helper
        // ──────────────────────────────────────────────────────────────────────

        private static async Task<string> ReverseGeocodeAsync(double lat, double lon)
        {
            try
            {
                // Nominatim requires a descriptive User-Agent header.
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"https://nominatim.openstreetmap.org/reverse?lat={lat:F6}&lon={lon:F6}&format=json");
                request.Headers.TryAddWithoutValidation("User-Agent", "NursingHomeApp/1.0");

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return CoordFallback(lat, lon);

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
                // Network error, timeout, or parse failure — fall back gracefully.
                return CoordFallback(lat, lon);
            }
        }

        private static string CoordFallback(double lat, double lon)
            => $"Lat: {lat:F5}, Lon: {lon:F5}";
    }
}
