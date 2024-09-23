using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace NursingHome.Db.Implementation
{
    public class SalarySlipService : ISalarySlipService
    {
        private readonly DbContextOptions<TaskContext> _dbConn;

        public SalarySlipService(string DbConn)
        {
            _dbConn = new DbContextOptionsBuilder<TaskContext>().UseSqlServer(DbConn).Options;
        }

       

        public (double totalDaysWorked, double netSalary, double basicSalary, int totalDaysInMonth) GetAttendanceAndNetSalary(int employeeId, int month, int year)
        {
            using var Db = new TaskContext(_dbConn);

            // Calculate total number of days in the given month
            int totalDaysInMonth = DateTime.DaysInMonth(year, month);

            // Calculate total days worked (based on attendance)
            var totalDaysWorked = Db.Attendance
                .Where(a => a.fkHelperId == employeeId
                            && a.Date.HasValue
                            && a.Date.Value.Month == month
                            && a.Date.Value.Year == year)
                .Sum(a => a.Time >= 9 ? 1.0 : a.Time >= 4.5 ? 0.5 : 0.0);

            // Get the helper's salary
            var salary = Db.Helpers
                .Where(h => h.Id == employeeId)
                .Select(h => h.Salary)
                .FirstOrDefault();

            if (salary.HasValue)
            {
               
                double basicSalary = (double)salary.Value;

                // Calculate the net salary (based on attendance and working days)
                double netSalary = (basicSalary / totalDaysInMonth) * totalDaysWorked;
                netSalary = Math.Round(netSalary, 2);

                return (totalDaysWorked, netSalary, basicSalary, totalDaysInMonth); // Returning total days worked, net salary, basic salary, and total days in month
            }

            return (totalDaysWorked, 0, 0, totalDaysInMonth); // Return 0 for net salary and basic salary if salary is not found
        }

    }
}
