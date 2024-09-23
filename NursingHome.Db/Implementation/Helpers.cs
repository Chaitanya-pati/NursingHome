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
using System.Data;
using Microsoft.Data.SqlClient;
namespace NursingHome.Db.Implementation
{
    public class Helpers : IHelpers
    {
        private readonly DbContextOptions<TaskContext> _dbConn;

        public Helpers(string DbConn)
        {
            _dbConn = new DbContextOptionsBuilder<TaskContext>().UseSqlServer(DbConn).Options;
        }

        public bool AddData(Models.Helpers helpers)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
                Db.Add(helpers);
                Db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateData(Models.Helpers helperData)
        {
            using var Db = new TaskContext(_dbConn);
            var GetData = Db.Helpers.Where(x => x.Id == helperData.Id).FirstOrDefault();
            if (GetData != null)
            {
                GetData.Name = helperData.Name;
                GetData.Image = helperData.Image;
                GetData.DateOfBirth = helperData.DateOfBirth;
                GetData.ParentName = helperData.ParentName;
                GetData.MaritalStatus = helperData.MaritalStatus;
                GetData.PermanentAddress = helperData.PermanentAddress;
                GetData.PresentAddress = helperData.PresentAddress;
                GetData.IdProof = helperData.IdProof;
                GetData.Education = helperData.Education;
                GetData.LanguagesKnown = helperData.LanguagesKnown;
                GetData.Experience = helperData.Experience;
                GetData.Salary = helperData.Salary;
                GetData.Reference = helperData.Reference;
                GetData.FamilyMembers = helperData.FamilyMembers;
                Db.SaveChanges();
                return true;
            }
            else { return false; }
        }

        public List<Models.Helpers> GetData(string username)
        {
            using var Db = new TaskContext(_dbConn);

            IQueryable<Models.Helpers> query;

            if (string.IsNullOrEmpty(username))
            {
                // If username is empty, select all data
                query = Db.Helpers;
            }
            else
            {
                // Filter by username
                query = Db.Helpers.Where(h => h.suser == username); // Assuming Helpers has a Username field
            }

            // Execute the query and return the result
            return query.ToList();
        }

        public bool DeleteData(int id)
        {
            using var Db = new TaskContext(_dbConn);
            var data = Db.Helpers.Where(x => x.Id == id).FirstOrDefault();
            if (data != null)
            {
                Db.Remove(data);
                Db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
