using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;
using NursingHome.Db.ViewModel;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System.Data;
using Microsoft.Data.SqlClient;
namespace NursingHome.Db.Implementation
{
    public class HomeNursing : INursingHome
    {
        private readonly DbContextOptions<TaskContext> _dbConn;

        public HomeNursing(string DbConn)
        {
            _dbConn = new DbContextOptionsBuilder<TaskContext>().UseSqlServer(DbConn).Options;
        }

        public int AddData(Models.HomeNursing homeNursing)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
                homeNursing.CreatedDate = DateTime.Now;
                homeNursing.UpdatedDate = DateTime.Now;

                Db.Add(homeNursing);
                Db.SaveChanges();
                return homeNursing.Id;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public List<HomeNursingView> GetData(DateTime startDate, DateTime endDate,string username)
        {
            
            using var Db = new TaskContext(_dbConn);
           
            var query = @"
    SELECT *
    FROM HomeNursing
    WHERE AdmissionDate BETWEEN @startDate AND @endDate 
    AND (@username = '' OR SUSer = @username)";
            List<Models.HomeNursing> result = new List<Models.HomeNursing>();
            result = Db.HomeNursing.FromSqlRaw(query,
                 new SqlParameter("@startDate", startDate),
                 new SqlParameter("@endDate", endDate),
                 new SqlParameter("@username", string.IsNullOrWhiteSpace(username) ? "" : username)
             ).ToList();
            var data = new List<HomeNursingView>();
            foreach (var item in result)
            {
                var dt = new HomeNursingView
                {
                    Id = item.Id,
                    PatientName = item.PatientName,
                    AdmissionDate = item.AdmissionDate,
                    PaymentStatus = item.PaymentStatus,
                    PeriodFrom = item.PeriodFrom,
                    PeriodTo = item.PeriodTo,
                    HelperName = item.fkHelperId.HasValue
    ? Db.Helpers.Where(x => x.Id == item.fkHelperId.Value)
                .Select(x => x.Name)
                .FirstOrDefault().ToString()
    : null,
                Age = item.Age,
                    Condition= item.Condition,
                    fkHelperId = item.fkHelperId,
                    Address = item.Address,
                    MonthlyPayment = item.MonthlyPayment,
                    RegistrationCharges = item.RegistrationCharges,
                    CustomerName = item.CustomerName,
                    IdProof = item.IdProof,
                    MobileNo = item.MobileNo,
                    SUser = item.SUser,
                    TypesofServices = item.TypesofServices,
                    clientSign = item.clientSign,
                    authorizedSign = item.authorizedSign,
                };
                data.Add(dt);
            }
            return data;
        }

        public bool UpdateData(Models.HomeNursing obj)
        {
            using var Db = new TaskContext(_dbConn);
            var GetData = Db.HomeNursing.Where(x => x.Id == obj.Id).FirstOrDefault();
            if (GetData != null)
            {
                GetData.PatientName = obj.PatientName;
                GetData.UpdatedDate = DateTime.Now;
                GetData.AdmissionDate = obj.AdmissionDate;
                GetData.Age = obj.Age;
                GetData.CustomerName = obj.CustomerName;
                GetData.Address = obj.Address;
                GetData.Condition = obj.Condition;
                GetData.IdProof = obj.IdProof;
                GetData.MonthlyPayment = obj.MonthlyPayment;
                GetData.RegistrationCharges = obj.RegistrationCharges;
                GetData.PeriodFrom = obj.PeriodFrom;
                GetData.PeriodTo = obj.PeriodTo;
                GetData.PaymentStatus = obj.PaymentStatus;
                GetData.SUser = obj.SUser;
                GetData.TypesofServices = obj.TypesofServices;
                GetData.MobileNo = obj.MobileNo;
                GetData.fkHelperId = obj.fkHelperId;
                Db.SaveChanges();
                return true;
            }
            else { return false; }
        }

        public bool DeleteData(int id)
        {
            using var Db = new TaskContext(_dbConn);
            var data = Db.HomeNursing.Where(x => x.Id == id).FirstOrDefault();
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
        public int getLatestID()
        {
            var Db = new TaskContext(_dbConn);
            int maxId = Db.HomeNursing.AsEnumerable().Select(o => o.Id).DefaultIfEmpty(0).Max();
            return maxId == 0 ? 1 : maxId + 1;
        }

        public bool UpdateClientSignature(int id, string signature)
        {
            using var Db = new TaskContext(_dbConn);
            var record = Db.HomeNursing.FirstOrDefault(x => x.Id == id);
            if (record != null)
            {
                record.clientSign = signature;
                Db.SaveChanges();
                return true;
            }
            return false;
        }

        public Models.HomeNursing GetPatientData(int patientId)
        {
            using var Db = new TaskContext(_dbConn);
            var data = Db.HomeNursing.Where(x => x.Id == patientId).FirstOrDefault();

            return data;
        }
    }
}
