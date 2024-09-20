using Microsoft.AspNetCore.Mvc;
using NursingHome.Db.Interface;

namespace NursingHome.Controllers
{
    public class SalarySlipController : Controller
    {
        private readonly ISalarySlipService _DbConn;
        

        private readonly ILogger<SalarySlipController> _logger;
        public SalarySlipController(ILogger<SalarySlipController> logger, ISalarySlipService Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        [HttpGet]
        public IActionResult GetAttendanceAndNetSalary(int employeeId, int month, int year)
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


        public IActionResult SalarySlip()
        {
            return View();
        }
    }
}
