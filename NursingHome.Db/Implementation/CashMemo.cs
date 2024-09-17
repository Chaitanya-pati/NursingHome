using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NursingHome.Db.Interface;
using NursingHome.Db.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace NursingHome.Db.Implementation
{
    public class CashMemo : ICashMemo
    {
        private readonly DbContextOptions<TaskContext> _dbConn;

        public CashMemo(string DbConn)
        {
            _dbConn = new DbContextOptionsBuilder<TaskContext>().UseSqlServer(DbConn).Options;
        }

        public bool AddOldAgeCashMemo(OldAgeCashMemo data)
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

        public bool UpdateOldAgeCashMemo(OldAgeCashMemo oldAge)
        {
            using var Db = new TaskContext(_dbConn);
            var GetData = Db.OldAgeCashMemo.Where(x => x.id == oldAge.id).FirstOrDefault();
            if (GetData != null)
            {
                GetData.patientName = oldAge.patientName;
                GetData.date = oldAge.date;
                GetData.amount = oldAge.amount;
                GetData.Inpaymentof = oldAge.Inpaymentof;
                GetData.amountInWords = oldAge.amountInWords;
                GetData.authorizedSign = oldAge.authorizedSign;
                GetData.recieversSign = oldAge.recieversSign;
                GetData.paymentMode = oldAge.paymentMode;
                Db.SaveChanges();
                return true;
            }
            else { return false; }
        }

        public bool DeleteOldAgeCashMemo(int id)
        {
            using var Db = new TaskContext(_dbConn);
            var data = Db.OldAgeCashMemo.Where(x => x.id == id).FirstOrDefault();
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


        public List<OldAgeCashMemo> GetOldAgePaymentData(DateTime startDate, DateTime endDate)
        {
            using var Db = new TaskContext(_dbConn);

            var query = @"
        SELECT *
        FROM OldAgeCashMemo
        WHERE date BETWEEN @startDate AND @endDate";

            return Db.OldAgeCashMemo.FromSqlRaw(query, new SqlParameter("@startDate", startDate), new SqlParameter("@endDate", endDate)).ToList();

        }
        public bool AddNursingCashMemo(HomeNursingCashMemo data)
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

        public bool UpdateNursingCashMemo(HomeNursingCashMemo oldAge)
        {
            using var Db = new TaskContext(_dbConn);
            var GetData = Db.HomeNursingCashMemo.Where(x => x.id == oldAge.id).FirstOrDefault();
            if (GetData != null)
            {
                GetData.patientName = oldAge.patientName;
                GetData.date = oldAge.date;
                GetData.amount = oldAge.amount;
                GetData.Inpaymentof = oldAge.Inpaymentof;
                GetData.amountInWords = oldAge.amountInWords;
                GetData.authorizedSign = oldAge.authorizedSign;
                GetData.recieversSign = oldAge.recieversSign;
                GetData.paymentMode = oldAge.paymentMode;
                Db.SaveChanges();
                return true;
            }
            else { return false; }
        }

        public bool DeleteNursingCashMemo(int id)
        {
            using var Db = new TaskContext(_dbConn);
            var data = Db.HomeNursingCashMemo.Where(x => x.id == id).FirstOrDefault();
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


        public List<HomeNursingCashMemo> GetNursingPaymentData(DateTime startDate, DateTime endDate)
        {
            using var Db = new TaskContext(_dbConn);

            var query = @"
        SELECT *
        FROM HomeNursingCashMemo
        WHERE date BETWEEN @startDate AND @endDate";

            return Db.HomeNursingCashMemo.FromSqlRaw(query, new SqlParameter("@startDate", startDate), new SqlParameter("@endDate", endDate)).ToList();

        }

    }
}
