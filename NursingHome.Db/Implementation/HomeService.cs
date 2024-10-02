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

        public int TotalOldAgeAdmissionLast30days()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var db = new TaskContext(_dbConn);
            return db.OldAge.Count(x => x.AdmissionDate >= thirtyDaysAgo);
        }
        public int TotalHomeNursingAdmissionLast30days()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var db = new TaskContext(_dbConn);
            return db.HomeNursing.Count(x => x.AdmissionDate >= thirtyDaysAgo);
        }
        public int TotalHelpersAdded30days()
        {
            var db = new TaskContext(_dbConn);
            return db.Helpers.Count();
        }

        public Dictionary<DateTime, int> GetOldAgeCounts(DateTime startDate, DateTime endDate)
        {
            var db = new TaskContext(_dbConn);
            return db.OldAge
                .Where(x => x.AdmissionDate >= startDate && x.AdmissionDate <= endDate)
                .GroupBy(x => x.AdmissionDate.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionary(g => g.Date, g => g.Count);
        }

        // Get counts for Nursing Home admissions
        public Dictionary<DateTime, int> GetNursingHomeCounts(DateTime startDate, DateTime endDate)
        {
            var db = new TaskContext(_dbConn);
            return db.HomeNursing
                .Where(x => x.AdmissionDate >= startDate && x.AdmissionDate <= endDate)
                .GroupBy(x => x.AdmissionDate.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionary(g => g.Date, g => g.Count);
        }

        // Get counts for Helpers admissions
        public Dictionary<DateTime, int> GetHelpersCounts(DateTime startDate, DateTime endDate)
        {
            var db = new TaskContext(_dbConn);
            return db.Helpers
                .Where(x => x.admissionDate >= startDate && x.admissionDate <= endDate)
                .GroupBy(x => x.admissionDate.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionary(g => g.Date, g => g.Count);
        }
    }
}
