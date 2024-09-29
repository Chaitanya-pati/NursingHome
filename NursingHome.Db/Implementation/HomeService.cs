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
    public class HomeService : IHomeService
    {
        private readonly DbContextOptions<TaskContext> _dbConn;

        public HomeService(string DbConn)
        {
            _dbConn = new DbContextOptionsBuilder<TaskContext>().UseSqlServer(DbConn).Options;
        }
        public void SaveLog(string ControllerName,string ActionName,string ErrorDescription)
        {
            var db = new TaskContext(_dbConn);
            Logger log = new Logger();
            log.controllerName = ControllerName;
            log.actionName = ActionName;
            log.errorDescription=ErrorDescription;
            log.createdDate= DateTime.Now;
            db.Add(log);
            db.SaveChanges();
        }
    }
}
