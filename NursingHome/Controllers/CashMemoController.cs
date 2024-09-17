using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace NursingHome.Controllers
{
    public class CashMemoController : Controller
    {
        private readonly ILogger<CashMemoController> _logger;
        private readonly ICashMemo _DbConn;
        public CashMemoController(ILogger<CashMemoController> logger, ICashMemo Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        public IActionResult CashMemo()
        {
            return View();
        }

        public IActionResult SaveOldAgeCashMemo(OldAgeCashMemo data)
        {
            if (data.id == 0)
            {
                return Json(_DbConn.AddOldAgeCashMemo(data));
            }
            else
            {
                return Json(_DbConn.UpdateOldAgeCashMemo(data));
            }
        }
        public IActionResult SaveNursingCashMemo(HomeNursingCashMemo data)
        {
            if (data.id == 0)
            {
                return Json(_DbConn.AddNursingCashMemo(data));
            }
            else
            {
                return Json(_DbConn.UpdateNursingCashMemo(data));
            }
        }

        public IActionResult GetCashMemoData(DateTime startDate, DateTime endDate)
        {
            var oldAgeData = _DbConn.GetOldAgePaymentData(startDate, endDate);
            var NursingData = _DbConn.GetNursingPaymentData(startDate, endDate);
            return Json(new { oldAge = oldAgeData, nursing = NursingData });
        }

        public IActionResult DeleteOldAgePayment(int id)
        {
            return Json(_DbConn.DeleteOldAgeCashMemo(id));
        }
        public IActionResult DeleteNursingPayment(int id)
        {
            return Json(_DbConn.DeleteNursingCashMemo(id));
        }

    }
}