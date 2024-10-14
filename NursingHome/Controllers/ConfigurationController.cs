using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;
using Microsoft.EntityFrameworkCore;
using NursingHome.Db.Models;

namespace NursingHome.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly ILogger<ConfigurationController> _logger;
        private readonly IConfig _DbConn;
        private readonly IHomeService _homeService; // Assuming this contains SaveLog method

        public ConfigurationController(ILogger<ConfigurationController> logger, IConfig Db, IHomeService homeService)
        {
            _logger = logger;
            _DbConn = Db;
            _homeService = homeService; // Injecting homeService for logging errors
        }

        public IActionResult Configuration()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(ConfigurationController), nameof(Configuration), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult AddCountry(string CountryName, int Id)
        {
            try
            {
                if (Id == 0)
                {
                    var IsAdded = _DbConn.AddCountry(CountryName);
                    return Json(IsAdded);
                }
                else
                {
                    var IsUpdated = _DbConn.UpdateCountry(Id, CountryName);
                    return Json(IsUpdated);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(ConfigurationController), nameof(AddCountry), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult GetCountryData()
        {
            try
            {
                var data = _DbConn.GetCountryList();
                return Json(new { data = data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(ConfigurationController), nameof(GetCountryData), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult GetStateData()
        {
            try
            {
                var data = _DbConn.GetStateList();
                return Json(new { data = data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(ConfigurationController), nameof(GetStateData), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult AddOrEditCity(City model)
        {
            try
            {
                if (model.Id == 0)
                {
                    var IsAdded = _DbConn.AddCity(model);
                    return Json(IsAdded);
                }
                else
                {
                    var IsUpdated = _DbConn.EditCity(model);
                    return Json(IsUpdated);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(ConfigurationController), nameof(AddOrEditCity), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult AddState(string StateName, int StateID)
        {
            try
            {
                if (StateID == 0)
                {
                    var Data = _DbConn.AddState(StateName);
                    return Json(Data);
                }
                else
                {
                    var Result = _DbConn.UpdateState(StateName, StateID);
                    return Json(Result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(ConfigurationController), nameof(AddState), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult GetCityData()
        {
            try
            {
                var data = _DbConn.GetCityData();
                return Json(new { data = data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(ConfigurationController), nameof(GetCityData), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult DeleteCity(int id)
        {
            try
            {
                var isDelete = _DbConn.DeleteCity(id);
                return Json(isDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(ConfigurationController), nameof(DeleteCity), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult CityDetails(string username)
        {
            try
            {
                var data = _DbConn.GetCityDetails(username);
                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(ConfigurationController), nameof(CityDetails), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            try
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _homeService.SaveLog(nameof(ConfigurationController), nameof(Error), ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
