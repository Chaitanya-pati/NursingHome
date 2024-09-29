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
    public class Config : IConfig
    {
        private readonly DbContextOptions<TaskContext> _dbConn;

        public Config(string DbConn)
        {
            _dbConn = new DbContextOptionsBuilder<TaskContext>().UseSqlServer(DbConn).Options;
        }

        public bool AddCountry(string Country)
        {
            using var Db = new TaskContext(_dbConn);
            Country CountryList = new Country();
            CountryList.Name = Country;
            Db.Add(CountryList);
            Db.SaveChanges();
            return true;


        }
        public List<Country> GetCountryList()
        {
            using var Db = new TaskContext(_dbConn);
            var query = @"
            SELECT *
            FROM Country";
            List<Models.Country> result = new List<Models.Country>();
            result = Db.Country.FromSqlRaw(query).ToList();
            return result;
        }
        public bool UpdateCountry(int Id, string CountryName)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
                var UpdateQuery = Db.Country.Where(x => x.Id == Id).FirstOrDefault();
                UpdateQuery.Name = CountryName;
                Db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteCountry(int Id)
        {
            try
            {
                var Db = new TaskContext(_dbConn);
                var DeleteQuery = Db.Country.Where(x => x.Id == Id).FirstOrDefault();
                Db.Remove(DeleteQuery);
                Db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }

        }
        public bool AddState(string StateName)
        {
            using var Db = new TaskContext(_dbConn);
            State State = new State();
            State.Name = StateName;
            Db.Add(State);
            Db.SaveChanges();
            return true;
        }
        public bool UpdateState(string StateName, int StateId)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
                State State = new State();
                var UpdateStateQuery = Db.State.Where(x => x.Id == StateId).FirstOrDefault();
                UpdateStateQuery.Name = StateName;
                Db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public List<State> GetStateList()
        {
            using var Db = new TaskContext(_dbConn);
            var query = @"
            SELECT *
            FROM State";
            List<Models.State> StatesList = new List<Models.State>();
            StatesList = Db.State.FromSqlRaw(query).ToList();
            return StatesList;
        }


        public List<City> GetCityData()
        {
            using var Db = new TaskContext(_dbConn);
            var query = @"
            SELECT *
            FROM City";
            List<Models.City> StatesList = new List<Models.City>();
            StatesList = Db.City.FromSqlRaw(query).ToList();
            return StatesList;
        }
        public bool AddCity(City userInfo)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);

                var user = new City
                {
                    Name = userInfo.Name,
                    Email = userInfo.Email,
                    OfficeAddress = userInfo.OfficeAddress,
                    MobileNumbers = userInfo.MobileNumbers,
                    ImageBase64 = userInfo.ImageBase64
                };


                Db.City.Add(user);
                Db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                // Console.WriteLine(ex.Message);
                return false;
            }
        }
        public bool EditCity(City Data)
        {
            using var Db = new TaskContext(_dbConn);
            City City = new City();
            var UpdateStateQuery = Db.City.Where(x => x.Id == Data.Id).FirstOrDefault();
            if (UpdateStateQuery != null)
            {
                UpdateStateQuery.Name = Data.Name;
                UpdateStateQuery.Email = Data.Email;
                UpdateStateQuery.OfficeAddress = Data.OfficeAddress;
                UpdateStateQuery.MobileNumbers = Data.MobileNumbers;
                UpdateStateQuery.ImageBase64 = Data.ImageBase64;
                Db.SaveChanges();
                return true;

            }
            else
            {
                return false;
            }

        }
        public bool DeleteCity(int cityId)
        {
            using var Db = new TaskContext(_dbConn);

            // Define the query to delete the city with the specified ID
            var query = "DELETE FROM City WHERE Id = @p0";

            // Execute the raw SQL command to delete the city
            int affectedRows = Db.Database.ExecuteSqlRaw(query, cityId);

            // Check if any rows were affected (i.e., a city was deleted)
            if (affectedRows > 0)
            {
                // Save changes to the database context (though for raw SQL commands, this might be redundant)
                Db.SaveChanges();
                return true; // Deletion successful
            }
            else
            {
                return false; // No city was found with the specified ID
            }
        }
        public City GetCityDetails(string username)
        {
            var db = new TaskContext(_dbConn);
            var data = (from u in db.Users
                        join c in db.City on u.fkCity equals c.Id
                        where u.UserName == username
                        select new City
                        {
                            Name = c.Name,
                            OfficeAddress = c.OfficeAddress,
                            MobileNumbers = c.MobileNumbers,
                            Email = c.Email,
                            ImageBase64 = c.ImageBase64
                        }).FirstOrDefault();
            return data;
        }

    }
}
