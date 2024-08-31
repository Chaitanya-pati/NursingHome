using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NursingHome.Db.Models;

namespace NursingHome.Db.Interface
{
    public interface IConfig
    {
        bool AddCountry(string Country);
        bool DeleteCountry(int Id);
        bool UpdateCountry(int Id, string CountryName);
        List<Country> GetCountryList();
        bool AddState(string StateName);
        bool AddCity(City userInfo);
        bool EditCity(City Data);
        bool DeleteCity(int cityId);
        bool UpdateState(string StateName, int StateId);
        List<State> GetStateList();
        List<City> GetCityData();


    }
}
