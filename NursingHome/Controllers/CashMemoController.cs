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
        private readonly IHomeService _homeService; // Assuming this contains SaveLog method

        public CashMemoController(ILogger<CashMemoController> logger, ICashMemo Db, IHomeService homeService)
        {
            _logger = logger;
            _DbConn = Db;
            _homeService = homeService; // Inject the service that contains SaveLog
        }

        public IActionResult CashMemo()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(CashMemoController), nameof(CashMemo), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult SaveOldAgeCashMemo(OldAgeCashMemo data)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(CashMemoController), nameof(SaveOldAgeCashMemo), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult SaveNursingCashMemo(HomeNursingCashMemo data)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(CashMemoController), nameof(SaveNursingCashMemo), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult GetCashMemoData(DateTime startDate, DateTime endDate)
        {
            try
            {
                var oldAgeData = _DbConn.GetOldAgePaymentData(startDate, endDate);
                var NursingData = _DbConn.GetNursingPaymentData(startDate, endDate);
                return Json(new { oldAge = oldAgeData, nursing = NursingData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(CashMemoController), nameof(GetCashMemoData), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult DeleteOldAgePayment(int id)
        {
            try
            {
                return Json(_DbConn.DeleteOldAgeCashMemo(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(CashMemoController), nameof(DeleteOldAgePayment), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult DeleteNursingPayment(int id)
        {
            try
            {
                return Json(_DbConn.DeleteNursingCashMemo(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(CashMemoController), nameof(DeleteNursingPayment), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
