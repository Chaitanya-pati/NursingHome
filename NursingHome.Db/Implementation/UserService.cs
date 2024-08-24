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
using System.Diagnostics.SymbolStore;

namespace NursingHome.Db.Implementation
{
    public class UserService:IUserService
    {
        private readonly DbContextOptions<TaskContext> _dbConn;

        public UserService(string DbConn)
        {
            _dbConn= new DbContextOptionsBuilder<TaskContext>().UseSqlServer(DbConn).Options;
        }
        public bool AddData(Users users)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
               
                users.IsFaceAdded = users.faceDescriptor != null?true:false;
                Db.Add(users);
                Db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool UpdateData(Users user)
        {
            using var Db = new TaskContext(_dbConn);
            var GetData = Db.Users.Where(x => x.Id == user.Id).FirstOrDefault();
            if (GetData != null)
            {
                GetData.FirstName = user.FirstName;
                GetData.LastName = user.LastName;
                GetData.UserName = user.UserName;
                GetData.Password = user.Password;
                GetData.Roles = user.Roles;
                GetData.fkState = user.fkState;
                GetData.IdProof = user.IdProof;
                GetData.ImageString = user.ImageString;
                GetData.fkCity = user.fkCity;
                GetData.fkCountry = user.fkCountry;
                GetData.faceDescriptor = user.faceDescriptor;
                GetData.IsFaceAdded = user.faceDescriptor !=null?true:false;
                GetData.MobileNo = user.MobileNo;
                GetData.PinCode = user.PinCode;
                GetData.HighestQualification = user.HighestQualification;
                Db.SaveChanges();
                return true;
            }
            else { return false; }
        }

        public List<Users> GetData()
        {
            using var Db = new TaskContext(_dbConn);

            var query = @"
        SELECT *
        FROM Users";
            List<Users> result = new List<Users>();
            result = Db.Users.FromSqlRaw(query).ToList();

            return result;
        }

        public bool DeleteUser(int id)
        {
            using var Db = new TaskContext(_dbConn);
            var IsUserExist = Db.Users.Where(x=>x.Id == id).FirstOrDefault();

            if (IsUserExist != null) {
                Db.Remove(IsUserExist);
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
