using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;

namespace NursingHome.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly ILogger<ConfigurationController> _logger;
        private readonly IConfig _DbConn;
        public ConfigurationController(ILogger<ConfigurationController> logger,IConfig Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        public IActionResult Configuration()
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