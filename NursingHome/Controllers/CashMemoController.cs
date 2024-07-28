using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;

namespace NursingHome.Controllers
{
    public class CashMemoController : Controller
    {
        private readonly ILogger<CashMemoController> _logger;
        private readonly IService _DbConn;
        public CashMemoController(ILogger<CashMemoController> logger,IService Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        public IActionResult CashMemo()
        {
            return View();
        } 


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}