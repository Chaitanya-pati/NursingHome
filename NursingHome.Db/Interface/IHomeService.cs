using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NursingHome.Db.Models;

namespace NursingHome.Db.Interface
{
    public interface IHomeService
    {
        void SaveLog(string ControllerName, string ActionName, string ErrorDescription);

        int TotalOldAgeAdmissionLast30days();
        int TotalHomeNursingAdmissionLast30days();
        int TotalHelpersAdded30days();
        Dictionary<DateTime, int> GetOldAgeCounts(DateTime startDate, DateTime endDate);
        Dictionary<DateTime, int> GetNursingHomeCounts(DateTime startDate, DateTime endDate);
        Dictionary<DateTime, int> GetHelpersCounts(DateTime startDate, DateTime endDate);
    }
}
