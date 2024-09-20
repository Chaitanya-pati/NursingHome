using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NursingHome.Db.Models;

namespace NursingHome.Db.Interface
{
    public interface ISalarySlipService
    {

        (double totalDaysWorked, double netSalary, double basicSalary, int totalDaysInMonth) GetAttendanceAndNetSalary(int employeeId, int month, int year);

    }
}
