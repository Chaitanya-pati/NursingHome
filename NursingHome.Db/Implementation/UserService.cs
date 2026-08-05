using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;
using Newtonsoft.Json;

namespace NursingHome.Db.Implementation
{
    public class UserService : IUserService
    {
        private readonly DbContextOptions<TaskContext> _dbConn;

        public UserService(string DbConn)
        {
            _dbConn = new DbContextOptionsBuilder<TaskContext>().UseSqlServer(DbConn).Options;
        }

        public bool AddData(Users users)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
                users.IsFaceAdded  = users.faceDescriptor != null;
                users.IsActive     = true;
                users.CreatedDate  = DateTime.Now;
                Db.Add(users);
                Db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"AddData failed: {ex.Message}", ex);
            }
        }

        public bool UpdateData(Users user)
        {
            using var Db = new TaskContext(_dbConn);
            var GetData = Db.Users.FirstOrDefault(x => x.Id == user.Id);
            if (GetData == null) return false;

            GetData.FirstName            = user.FirstName;
            GetData.LastName             = user.LastName;
            GetData.UserName             = user.UserName;
            GetData.Password             = user.Password;
            GetData.Roles                = user.Roles;
            GetData.fkState              = user.fkState;
            GetData.IdProof              = user.IdProof;
            GetData.ImageString          = user.ImageString;
            GetData.fkCity               = user.fkCity;
            GetData.fkCountry            = user.fkCountry;
            GetData.MobileNo             = user.MobileNo;
            GetData.PinCode              = user.PinCode;
            GetData.HighestQualification = user.HighestQualification;
            GetData.Email                = user.Email;
            GetData.IsActive             = user.IsActive;
            Db.SaveChanges();
            return true;
        }

        public List<Users> GetData()
        {
            using var Db = new TaskContext(_dbConn);
            return Db.Users.FromSqlRaw("SELECT * FROM Users").ToList();
        }

        public bool DeleteUser(int id)
        {
            using var Db = new TaskContext(_dbConn);
            var user = Db.Users.FirstOrDefault(x => x.Id == id);
            if (user == null) return false;
            Db.Remove(user);
            Db.SaveChanges();
            return true;
        }

        public bool SaveFaceDescriptor(string username, string faceid)
        {
            using var db = new TaskContext(_dbConn);
            var user = db.Users.FirstOrDefault(x => x.UserName == username);
            if (user == null) return false;
            user.faceDescriptor = faceid;
            user.IsFaceAdded    = true;
            db.SaveChanges();
            return true;
        }

        public dynamic GetFaceDescriptor(string Username)
        {
            using var db = new TaskContext(_dbConn);
            return db.Users.Where(x => x.UserName == Username).Select(u => u.faceDescriptor);
        }

        public Users GetUserDataById(int id)
        {
            using var db = new TaskContext(_dbConn);
            return db.Users.FirstOrDefault(x => x.Id == id);
        }

        public Users GetUserByUsername(string username)
        {
            using var db = new TaskContext(_dbConn);
            return db.Users.FirstOrDefault(x => x.UserName == username);
        }

        public Users CheckValidUser(string userName, string password)
        {
            using var db = new TaskContext(_dbConn);
            return db.Users.FirstOrDefault(x => x.UserName == userName && x.Password == password);
        }

        // ── Extended methods ──────────────────────────────────────────────────

        /// <summary>
        /// Creates a new user with full uniqueness validation.
        /// allHelperSusers = current list of suser values from all helpers (to prevent dual assignment).
        /// </summary>
        public (bool success, string error) CreateUserWithValidation(Users user, string checkNotAssignedToOtherHelper, List<string> allHelperSusers)
        {
            try
            {
                using var db = new TaskContext(_dbConn);

                // 1. Username uniqueness
                if (db.Users.Any(u => u.UserName == user.UserName))
                    return (false, $"Username '{user.UserName}' is already taken.");

                // 2. Mobile uniqueness (if provided)
                if (!string.IsNullOrWhiteSpace(user.MobileNo) &&
                    db.Users.Any(u => u.MobileNo == user.MobileNo))
                    return (false, $"Mobile number '{user.MobileNo}' is already registered.");

                // 3. Email uniqueness (if provided)
                if (!string.IsNullOrWhiteSpace(user.Email) &&
                    db.Users.Any(u => u.Email == user.Email))
                    return (false, $"Email '{user.Email}' is already registered.");

                // 4. This user's username must not already be assigned to another helper
                if (allHelperSusers != null &&
                    allHelperSusers.Any(s => string.Equals(s, user.UserName, StringComparison.OrdinalIgnoreCase)))
                    return (false, "This user is already assigned to another helper.");

                // Keep caller-supplied IsActive; only default to true if not explicitly set
                // (IsActive is a bool, so it will be false if not set — default to true in that case only
                //  when the caller passes true or simply omits it; false is an explicit active=inactive choice)
                user.CreatedDate = DateTime.Now;
                user.IsFaceAdded = user.faceDescriptor != null;

                db.Users.Add(user);
                db.SaveChanges();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public bool IsUserAlreadyAssigned(string userName, List<string> allHelperSusers)
        {
            if (string.IsNullOrWhiteSpace(userName) || allHelperSusers == null) return false;
            return allHelperSusers.Any(s => string.Equals(s, userName, StringComparison.OrdinalIgnoreCase));
        }

        public bool UpdatePassword(string userName, string newPassword)
        {
            try
            {
                using var db = new TaskContext(_dbConn);
                var user = db.Users.FirstOrDefault(x => x.UserName == userName);
                if (user == null) return false;
                user.Password = newPassword;
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void RecordLogin(string userName)
        {
            try
            {
                using var db = new TaskContext(_dbConn);
                var user = db.Users.FirstOrDefault(x => x.UserName == userName);
                if (user != null)
                {
                    user.LastLogin = DateTime.Now;
                    db.SaveChanges();
                }
            }
            catch { /* non-critical */ }
        }
    }
}
