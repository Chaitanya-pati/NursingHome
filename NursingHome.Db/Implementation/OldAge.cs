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
using Microsoft.Data.SqlClient;
using System.Data;


namespace NursingHome.Db.Implementation
{
    public class OldAge : IOldAge
    {
        private readonly DbContextOptions<TaskContext> _dbConn;

        public OldAge(string DbConn)
        {
            _dbConn = new DbContextOptionsBuilder<TaskContext>().UseSqlServer(DbConn).Options;
        }

        public bool AddData(Models.OldAge oldAge)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
                oldAge.SUser = "Admin";
                oldAge.CreatedDate = DateTime.Now;
                oldAge.UpdatedDate = DateTime.Now;

                Db.Add(oldAge);
                Db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public List<Models.OldAge> GetData(DateTime startDate, DateTime endDate)
        {
            using var Db = new TaskContext(_dbConn);

            var query = @"
        SELECT *
        FROM OldAge
        WHERE AdmissionDate BETWEEN @startDate AND @endDate";
            List<Models.OldAge> result = new List<Models.OldAge>();
            result = Db.OldAge.FromSqlRaw(query, new SqlParameter("@startDate", startDate), new SqlParameter("@endDate", endDate)).ToList();

            return result;
        }

        public bool UpdateData(Models.OldAge oldAge)
        {
            using var Db = new TaskContext(_dbConn);
            var GetData = Db.OldAge.Where(x => x.Id == oldAge.Id).FirstOrDefault();
            if (GetData != null)
            {
                GetData.PatientName = oldAge.PatientName;
                GetData.UpdatedDate = DateTime.Now;
                GetData.AdmissionDate = oldAge.AdmissionDate;
                GetData.Age = oldAge.Age;
                GetData.CustomerName = oldAge.CustomerName;
                GetData.Address = oldAge.Address;
                GetData.Condition = oldAge.Condition;
                GetData.IdProof = oldAge.IdProof;
                GetData.MonthlyPayment = oldAge.MonthlyPayment;
                GetData.RegistrationCharges = oldAge.RegistrationCharges;
                GetData.PeriodFrom = oldAge.PeriodFrom;
                GetData.PeriodTo = oldAge.PeriodTo;
                GetData.PaymentStatus = oldAge.PaymentStatus;
                GetData.SUser = oldAge.SUser;
                GetData.TypesofServices = oldAge.TypesofServices;
                GetData.MobileNo = oldAge.MobileNo;
                Db.SaveChanges();
                return true;
            }
            else { return false; }
        }

        public bool DeleteData(int id)
        {
            using var Db = new TaskContext(_dbConn);
            var data = Db.OldAge.Where(x => x.Id == id).FirstOrDefault();
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

        public bool AddOldAgePayment(OldAgePayment data)
        {
            using var Db = new TaskContext(_dbConn);
            try
            {
                Db.Add(data);
                Db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdatePaymentData(OldAgePayment oldAge)
        {
            using var Db = new TaskContext(_dbConn);
            var GetData = Db.OldAgePayment.Where(x => x.Id == oldAge.Id).FirstOrDefault();
            if (GetData != null)
            {
                GetData.fkOldAgeId = oldAge.fkOldAgeId;
                GetData.Date = oldAge.Date;
                GetData.Amount = oldAge.Amount;
                Db.SaveChanges();
                return true;
            }
            else { return false; }
        }

        public bool DeleteOldAgePayment(int id)
        {
            using var Db = new TaskContext(_dbConn);
            var data = Db.OldAgePayment.Where(x => x.Id == id).FirstOrDefault();
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


        public List<OldAgePaymentData> GetPaymentData(DateTime startDate, DateTime endDate)
        {
            using var Db = new TaskContext(_dbConn);

            var query = @"
        SELECT *
        FROM OldAge
        WHERE date BETWEEN @startDate AND @endDate";
            List<OldAgePayment> result = new List<OldAgePayment>();
            result = Db.OldAgePayment.FromSqlRaw(query, new SqlParameter("@startDate", startDate), new SqlParameter("@endDate", endDate)).ToList();
            List<OldAgePaymentData> data = new List<OldAgePaymentData>();
            foreach (var item in result)
            {
                var dt = new OldAgePaymentData()
                {
                    Id = item.Id,
                    PatientName = Db.OldAge.Where(x=>x.Id == item.Id).Select(x=>x.PatientName).FirstOrDefault(),
                    Date = item.Date,
                    Amount = item.Amount,
                };
                data.Add(dt);
            }
            return data;
        }
        public int getLatestID()
        {
             var Db = new TaskContext(_dbConn);
            return Db.OldAge.Max(o => o.Id);

        }
    }

  
}
