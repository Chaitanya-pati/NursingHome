using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NursingHome.Db.Models;

namespace NursingHome.Db.Interface
{
    public interface IOldAge
    {
        bool AddData(OldAge oldAge);

        List<Models.OldAge> GetData(DateTime startDate, DateTime endDate);

        bool UpdateData(Models.OldAge oldAge);

        bool DeleteData(int id);

        int getLatestID();

    }
}
