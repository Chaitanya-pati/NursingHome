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
    }
}
