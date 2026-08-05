using System;
using System.Runtime.InteropServices;

namespace NursingHome.Db.Utils
{
    /// <summary>
    /// Returns the current date/time in Indian Standard Time (IST = UTC +5:30).
    /// Works on both Windows ("India Standard Time") and Linux ("Asia/Kolkata").
    /// Use <see cref="Now"/> everywhere a server-recorded timestamp is needed so
    /// that all attendance data is consistently in IST regardless of where the
    /// server is physically hosted.
    /// </summary>
    public static class IndianTime
    {
        private static readonly TimeZoneInfo _ist = TimeZoneInfo.FindSystemTimeZoneById(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "India Standard Time"
                : "Asia/Kolkata");

        /// <summary>Current date and time in IST (UTC +5:30).</summary>
        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _ist);
    }
}
