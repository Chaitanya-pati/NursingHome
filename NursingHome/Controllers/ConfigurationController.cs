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
        public ConfigurationController(ILogger<ConfigurationController> logger,IConfig Db)
        {
            _logger = logger;
            _DbConn = Db;
        }

        public IActionResult Configuration()
        {
            return View();
        } 
       
       public IActionResult AddCountry(string CountryName,int Id)

       {
            if(Id==0)
            {
                var IsAdded = _DbConn.AddCountry(CountryName);
                return Json(IsAdded);

            }
            else
            {
                var IsUpdated=_DbConn.UpdateCountry(Id,CountryName);
                return Json(IsUpdated);
            }
       
       }
        public IActionResult GetCountryData()
        {
            var data =_DbConn.GetCountryList();
            return Json(new
            {
                data=data,
            });
        }
        public IActionResult GetStateData()
        {
            var data = _DbConn.GetStateList();
            return Json(new
            {
                data=data,

            });
        }
     
        public IActionResult AddOrEditCity(City model)
        {
            if (model.Id==0)
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


        public IActionResult AddState(string StateName,int StateID)
        {
            if (StateID == 0)
            {
                var Data = _DbConn.AddState(StateName);
                return Json(Data);
            }
            else
            {
                var Result=_DbConn.UpdateState(StateName, StateID);
                return Json(Result);
            }
        }
        public IActionResult GetCityData()
        {
            var data = _DbConn.GetCityData();
            return Json(new
            {
                data = data,

            });
        }


        public IActionResult DeleteCity(int id)
        {
            var isDelete = _DbConn.DeleteCity(id);
            return Json(isDelete);
        }






        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}