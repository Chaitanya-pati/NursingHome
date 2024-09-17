using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NursingHome.Db.Models;

namespace NursingHome.Db.Interface
{
    public interface ICashMemo
    {

        bool AddOldAgeCashMemo(OldAgeCashMemo data);
        bool UpdateOldAgeCashMemo(OldAgeCashMemo oldAge);

        bool DeleteOldAgeCashMemo(int id);
        List<OldAgeCashMemo> GetOldAgePaymentData(DateTime startDate, DateTime endDate);
         bool AddNursingCashMemo(HomeNursingCashMemo data);
        bool UpdateNursingCashMemo(HomeNursingCashMemo oldAge);

        bool DeleteNursingCashMemo(int id);
        List<HomeNursingCashMemo> GetNursingPaymentData(DateTime startDate, DateTime endDate);

    }
}
