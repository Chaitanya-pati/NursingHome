using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NursingHome.Db.Models;

namespace NursingHome.Db.Interface
{
    public interface IAttedanceService
    {
        // ── Existing methods ────────────────────────────────────────────────
        List<object> GetHelperAttendance();
        bool AddAttendance(Attendance dto);
        bool UpdateAttendance(Attendance updatedAttendanceData);
        bool DeleteAttendance(int id);
        List<object> GetHelpers();
        List<object> PatientDetails();

        /// <summary>
        /// Records a GPS-captured check-in. The caller must populate
        /// CheckInTime (server time), Latitude, Longitude, GpsAccuracy,
        /// Address, and Status before calling this method.
        /// </summary>
        bool RecordCheckIn(Attendance checkIn);

        // ── New methods ─────────────────────────────────────────────────────

        /// <summary>
        /// Records a GPS-captured check-out for the given attendance record.
        /// CheckOutTime is the server-recorded timestamp supplied by the caller.
        /// </summary>
        bool RecordCheckOut(int attendanceId, DateTime checkOutTime,
                            double lat, double lon, double gpsAccuracy, string address);

        /// <summary>
        /// Approves a pending attendance record, calculates total hours, and
        /// stores the approver's username, timestamp, and optional remarks.
        /// </summary>
        bool ApproveAttendance(int id, string approvedBy, string remarks);

        /// <summary>
        /// Rejects a pending attendance record, stores the approver's username,
        /// timestamp, and optional remarks.
        /// </summary>
        bool RejectAttendance(int id, string approvedBy, string remarks);

        /// <summary>
        /// Returns all attendance records that have status 'Pending Approval'
        /// and have both a check-in and a check-out time recorded.
        /// </summary>
        List<object> GetPendingAttendance();
    }
}
