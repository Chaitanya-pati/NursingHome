using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Newtonsoft.Json;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace NursingHome.Db.Implementation
{
    public class UserService : IUserService
    {
        private readonly DbContextOptions<TaskContext> _dbConn;
        private readonly DbContextOptions<TaskContext> _DestndbConn;

        public UserService(string DbConn,string DestnDb)
        {
            _dbConn = new DbContextOptionsBuilder<TaskContext>().UseSqlServer(DbConn).Options;
            _DestndbConn = new DbContextOptionsBuilder<TaskContext>().UseSqlServer(DestnDb).Options;
            // CopyAllData_Bulk();
            copydata();
        }
        public bool AddData(Users users)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);

                users.IsFaceAdded = users.faceDescriptor != null ? true : false;
                Db.Add(users);
                Db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public class TempDbData
        {
            public List<Country> Countries { get; set; }
            public List<State> States { get; set; }
            public List<City> Cities { get; set; }
            public List<Users> Users { get; set; }
            public List<Models.Helpers> Helpers { get; set; }
            public List<Models.HomeNursing> HomeNursing { get; set; }
            public List<Models.OldAge> OldAge { get; set; }
            public List<Attendance> Attendance { get; set; }
            public List<Logger> Logger { get; set; }
        }
        public async Task<TempDbData> FetchAllDataAsync()
        {
            using var sourceDb = new TaskContext(_dbConn);

            sourceDb.Database.SetCommandTimeout(120);
            sourceDb.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return new TempDbData
            {
                Countries = await sourceDb.Country.ToListAsync(),
                States = await sourceDb.State.ToListAsync(),
                Cities = await sourceDb.City.ToListAsync(),
                Users = await sourceDb.Users.ToListAsync(),
                Helpers = await sourceDb.Helpers.ToListAsync(),
                HomeNursing = await sourceDb.HomeNursing.ToListAsync(),
                OldAge = await sourceDb.OldAge.ToListAsync(),
                Attendance = await sourceDb.Attendance.ToListAsync(),
                Logger = await sourceDb.Logger.ToListAsync()
            };
        }

        public bool InsertAllData(TempDbData data)
        {
            using var destDb = new TaskContext(_DestndbConn);

            destDb.Database.SetCommandTimeout(0);

            using var transaction = destDb.Database.BeginTransaction();

            try
            {
                const int batchSize = 500;

                Insert(destDb, data.Countries, destDb.Country, "Country", batchSize);
                Insert(destDb, data.States, destDb.State, "State", batchSize);
                Insert(destDb, data.Cities, destDb.City, "City", batchSize);
                Insert(destDb, data.Users, destDb.Users, "Users", batchSize);
                Insert(destDb, data.Helpers, destDb.Helpers, "Helpers", batchSize);
                Insert(destDb, data.HomeNursing, destDb.HomeNursing, "HomeNursing", batchSize);
                Insert(destDb, data.OldAge, destDb.OldAge, "OldAge", batchSize);
                Insert(destDb, data.Attendance, destDb.Attendance, "Attendance", batchSize);
                Insert(destDb, data.Logger, destDb.Logger, "Logger", batchSize);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        private void Insert<T>(
    TaskContext destDb,
    List<T> data,
    DbSet<T> table,
    string tableName,
    int batchSize) where T : class
        {
            destDb.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT dbo.{tableName} ON");

            for (int i = 0; i < data.Count; i += batchSize)
            {
                table.AddRange(data.Skip(i).Take(batchSize));
                destDb.SaveChanges();
                destDb.ChangeTracker.Clear();
            }

            destDb.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT dbo.{tableName} OFF");
        }
        public async Task copydata()
        {
            
           var data=await FetchAllDataAsync();   // step 1 (read only)
           InsertAllData(data);
        }// step 2 (write only)
        public bool CopyAllData()
        {
            using var sourceDb = new TaskContext(_dbConn);
            using var destDb = new TaskContext(_DestndbConn);

            destDb.Database.SetCommandTimeout(0); // 🔥 prevent timeout

            using var transaction = destDb.Database.BeginTransaction();

            try
            {
                const int batchSize = 500;

                CopyCountry(sourceDb, destDb, batchSize);
                CopyState(sourceDb, destDb, batchSize);
                CopyCity(sourceDb, destDb, batchSize);
                CopyUsers(sourceDb, destDb, batchSize);
                CopyHelpers(sourceDb, destDb, batchSize);
                CopyHomeNursing(sourceDb, destDb, batchSize);
                CopyOldAge(sourceDb, destDb, batchSize);
                CopyAttendance(sourceDb, destDb, batchSize);
                CopyLogger(sourceDb, destDb, batchSize);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // ===================== HELPERS =====================

        private void InsertInBatches<T>(
            TaskContext destDb,
            IEnumerable<T> data,
            DbSet<T> table,
            string tableName,
            int batchSize) where T : class
        {
            destDb.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT dbo.{tableName} ON");

            var list = data.ToList();

            for (int i = 0; i < list.Count; i += batchSize)
            {
                var batch = list.Skip(i).Take(batchSize).ToList();
                table.AddRange(batch);
                destDb.SaveChanges();
                destDb.ChangeTracker.Clear();
            }

            destDb.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT dbo.{tableName} OFF");
        }

        // ===================== TABLE METHODS =====================

        private void CopyCountry(TaskContext src, TaskContext dest, int batchSize)
        {
            var data = src.Country.AsNoTracking()
                .Select(x => new Country { Id = x.Id, Name = x.Name });

            InsertInBatches(dest, data, dest.Country, "Country", batchSize);
        }

        private void CopyState(TaskContext src, TaskContext dest, int batchSize)
        {
            var data = src.State.AsNoTracking()
                .Select(x => new State { Id = x.Id, Name = x.Name });

            InsertInBatches(dest, data, dest.State, "State", batchSize);
        }

        private void CopyCity(TaskContext src, TaskContext dest, int batchSize)
        {
            var data = src.City.AsNoTracking()
                .Select(c => new City
                {
                    Id = c.Id,
                    Name = c.Name,
                    OfficeAddress = c.OfficeAddress,
                    MobileNumbers = c.MobileNumbers,
                    Email = c.Email,
                    ImageBase64 = c.ImageBase64
                });

            InsertInBatches(dest, data, dest.City, "City", batchSize);
        }

        private void CopyUsers(TaskContext src, TaskContext dest, int batchSize)
        {
            var data = src.Users.AsNoTracking()
                .Select(u => new Users
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserName = u.UserName,
                    Password = u.Password,
                    Roles = u.Roles,
                    MobileNo = u.MobileNo,
                    ImageString = u.ImageString,
                    IdProof = u.IdProof,
                    fkCountry = u.fkCountry,
                    fkState = u.fkState,
                    fkCity = u.fkCity,
                    PinCode = u.PinCode,
                    HighestQualification = u.HighestQualification,
                    faceDescriptor = u.faceDescriptor,
                    IsFaceAdded = u.IsFaceAdded
                });

            InsertInBatches(dest, data, dest.Users, "Users", batchSize);
        }

        private void CopyHelpers(TaskContext src, TaskContext dest, int batchSize)
        {
            var data = src.Helpers.AsNoTracking()
                .Select(h => new Models.Helpers
                {
                    Id = h.Id,
                    Name = h.Name,
                    Image = h.Image,
                    DateOfBirth = h.DateOfBirth,
                    ParentName = h.ParentName,
                    MaritalStatus = h.MaritalStatus,
                    PermanentAddress = h.PermanentAddress,
                    PresentAddress = h.PresentAddress,
                    Education = h.Education,
                    LanguagesKnown = h.LanguagesKnown,
                    Experience = h.Experience,
                    IdProof = h.IdProof,
                    Salary = h.Salary,
                    Reference = h.Reference,
                    FamilyMembers = h.FamilyMembers,
                    suser = h.suser,
                    candidateSign = h.candidateSign,
                    Place = h.Place,
                    admissionDate = h.admissionDate
                });

            InsertInBatches(dest, data, dest.Helpers, "Helpers", batchSize);
        }

        private void CopyHomeNursing(TaskContext src, TaskContext dest, int batchSize)
        {
            var data = src.HomeNursing.AsNoTracking()
                .Select(h => new Models.HomeNursing
                {
                    Id = h.Id,
                    AdmissionDate = h.AdmissionDate,
                    PatientName = h.PatientName,
                    Address = h.Address,
                    Age = h.Age,
                    Condition = h.Condition,
                    CustomerName = h.CustomerName,
                    MobileNo = h.MobileNo,
                    TypesofServices = h.TypesofServices,
                    RegistrationCharges = h.RegistrationCharges,
                    MonthlyPayment = h.MonthlyPayment,
                    PeriodFrom = h.PeriodFrom,
                    PeriodTo = h.PeriodTo,
                    IdProof = h.IdProof,
                    PaymentStatus = h.PaymentStatus,
                    CreatedDate = h.CreatedDate,
                    UpdatedDate = h.UpdatedDate,
                    SUser = h.SUser,
                    fkHelperId = h.fkHelperId,
                    clientSign = h.clientSign,
                    authorizedSign = h.authorizedSign,
                    isActive = h.isActive
                });

            InsertInBatches(dest, data, dest.HomeNursing, "HomeNursing", batchSize);
        }

        private void CopyOldAge(TaskContext src, TaskContext dest, int batchSize)
        {
            var data = src.OldAge.AsNoTracking()
                .Select(o => new Models.OldAge
                {
                    Id = o.Id,
                    AdmissionDate = o.AdmissionDate,
                    PatientName = o.PatientName,
                    Address = o.Address,
                    Age = o.Age,
                    Condition = o.Condition,
                    CustomerName = o.CustomerName,
                    MobileNo = o.MobileNo,
                    TypesofServices = o.TypesofServices,
                    RegistrationCharges = o.RegistrationCharges,
                    MonthlyPayment = o.MonthlyPayment,
                    PeriodFrom = o.PeriodFrom,
                    PeriodTo = o.PeriodTo,
                    IdProof = o.IdProof,
                    PaymentStatus = o.PaymentStatus,
                    CreatedDate = o.CreatedDate,
                    UpdatedDate = o.UpdatedDate,
                    SUser = o.SUser,
                    clientSign = o.clientSign,
                    authorizedSign = o.authorizedSign
                });

            InsertInBatches(dest, data, dest.OldAge, "OldAge", batchSize);
        }

        private void CopyAttendance(TaskContext src, TaskContext dest, int batchSize)
        {
            var data = src.Attendance.AsNoTracking()
                .Select(a => new Attendance
                {
                    Id = a.Id,
                    fkHelperId = a.fkHelperId,
                    fkNursingId = a.fkNursingId,
                    Date = a.Date,
                    Time = a.Time,
                    Description = a.Description
                });

            InsertInBatches(dest, data, dest.Attendance, "Attendance", batchSize);
        }

        private void CopyLogger(TaskContext src, TaskContext dest, int batchSize)
        {
            var data = src.Logger.AsNoTracking()
                .Select(l => new Logger
                {
                    id = l.id,
                    controllerName = l.controllerName,
                    actionName = l.actionName,
                    errorDescription = l.errorDescription,
                    createdDate = l.createdDate
                });

            InsertInBatches(dest, data, dest.Logger, "Logger", batchSize);
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
               // GetData.IsFaceAdded = GetData.faceDescriptor != null ? true : false;
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
            var IsUserExist = Db.Users.Where(x => x.Id == id).FirstOrDefault();

            if (IsUserExist != null)
            {
                Db.Remove(IsUserExist);
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool SaveFaceDescriptor(string username, string faceid)
        {
            var db = new TaskContext(_dbConn);
            var IsExistUser = db.Users.FirstOrDefault(x => x.UserName == username);
            if (IsExistUser != null)
            {
                string faceDescriptorJson = JsonConvert.SerializeObject(faceid);
                IsExistUser.faceDescriptor = faceid;
                IsExistUser.IsFaceAdded = true;
                db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }


        public dynamic GetFaceDescriptor(string Username)
        {
            var db = new TaskContext (_dbConn);
            return db.Users.Where(x => x.UserName == Username).Select(user => user.faceDescriptor);
        }

        public Users GetUserDataById(int id)
        {
            var db = new TaskContext(_dbConn);
            return db.Users.Where(x => x.Id == id).FirstOrDefault();
        }

        public Users CheckValidUser(string userName, string password)
        {
            var db = new TaskContext(_dbConn);
            return db.Users.Where(x => x.UserName == userName && x.Password == password).FirstOrDefault();
        }
    }
}
