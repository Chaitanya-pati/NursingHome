using Microsoft.AspNetCore.Mvc;
using NursingHome.Db.Interface;

namespace NursingHome.Controllers
{
    public class SalarySlipController : Controller
    {
        private readonly ISalarySlipService _DbConn;
        private readonly IHomeService _logger; // Changed to IHomeService for SaveLog functionality

        public SalarySlipController(IHomeService logger, ISalarySlipService Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        [HttpGet]
        public IActionResult GetAttendanceAndNetSalary(int employeeId, int month, int year)
        {
            try
            {
                var (totalDaysWorked, netSalary, basicSalary, totalDaysInMonth) = _DbConn.GetAttendanceAndNetSalary(employeeId, month, year);

                return Json(new
                {
                    totalDaysWorked,
                    netSalary,
                    basicSalary,
                    totalDaysInMonth
                });
            }
            catch (Exception ex)
            {
                _logger.SaveLog("SalarySlipController", "GetAttendanceAndNetSalary", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult SalarySlip()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.SaveLog("SalarySlipController", "SalarySlip", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
