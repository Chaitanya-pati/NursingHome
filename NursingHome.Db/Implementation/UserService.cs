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
    public class UserService:IUserService
    {
        private readonly DbContextOptions<TaskContext> _dbConn;

        public UserService(string DbConn)
        {
            _dbConn= new DbContextOptionsBuilder<TaskContext>().UseSqlServer(DbConn).Options;
        }

        public void AddData()
        {
            using var Db = new TaskContext(_dbConn);
            State temp = new State();
            temp.Name = "Goa";
            Db.Add(temp);
            Db.SaveChanges();
        }
    }
}
