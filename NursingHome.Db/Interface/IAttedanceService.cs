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


    }
}
