using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;
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
            catch
            {
                return false;
            }
        }

        public bool UpdateData(Models.Helpers helperData)
        {
            using var Db = new TaskContext(_dbConn);
            var GetData = Db.Helpers.FirstOrDefault(x => x.Id == helperData.Id);
            if (GetData == null) return false;

            GetData.Name             = helperData.Name;
            GetData.Image            = helperData.Image;
            GetData.DateOfBirth      = helperData.DateOfBirth;
            GetData.ParentName       = helperData.ParentName;
            GetData.MaritalStatus    = helperData.MaritalStatus;
            GetData.PermanentAddress = helperData.PermanentAddress;
            GetData.PresentAddress   = helperData.PresentAddress;
            GetData.IdProof          = helperData.IdProof;
            GetData.Education        = helperData.Education;
            GetData.LanguagesKnown   = helperData.LanguagesKnown;
            GetData.Experience       = helperData.Experience;
            GetData.Salary           = helperData.Salary;
            GetData.Reference        = helperData.Reference;
            GetData.FamilyMembers    = helperData.FamilyMembers;
            GetData.MobileNo         = helperData.MobileNo;
            GetData.Designation      = helperData.Designation;
            GetData.BloodGroup       = helperData.BloodGroup;
            GetData.AadhaarNo        = helperData.AadhaarNo;
            Db.SaveChanges();
            return true;
        }

        public List<Models.Helpers> GetData(string username)
        {
            using var Db = new TaskContext(_dbConn);
            return username == "admin"
                ? Db.Helpers.ToList()
                : Db.Helpers.Where(h => h.suser == username).ToList();
        }

        public bool DeleteData(int id)
        {
            using var Db = new TaskContext(_dbConn);
            var data = Db.Helpers.FirstOrDefault(x => x.Id == id);
            if (data == null) return false;
            Db.Remove(data);
            Db.SaveChanges();
            return true;
        }

        // ── User assignment ───────────────────────────────────────────────────

        public bool AssignUser(int helperId, string userName)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
                var helper = Db.Helpers.FirstOrDefault(x => x.Id == helperId);
                if (helper == null) return false;
                helper.suser = userName;
                Db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveUserAssignment(int helperId)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
                var helper = Db.Helpers.FirstOrDefault(x => x.Id == helperId);
                if (helper == null) return false;
                helper.suser = null;
                Db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ── Assignment history / audit ────────────────────────────────────────

        public void RecordAssignmentHistory(HelperUserAssignmentHistory entry)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
                Db.HelperUserAssignmentHistory.Add(entry);
                Db.SaveChanges();
            }
            catch
            {
                // audit failure must never crash the main operation
            }
        }

        public List<HelperUserAssignmentHistory> GetAssignmentHistory(int helperId)
        {
            using var Db = new TaskContext(_dbConn);
            return Db.HelperUserAssignmentHistory
                     .Where(h => h.HelperId == helperId)
                     .OrderByDescending(h => h.Id)
                     .ToList();
        }
    }
}
