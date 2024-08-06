using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NursingHome.Db.Models;
using NursingHome.Db.ViewModel;

namespace NursingHome.Db.Interface
{
    public interface INursingHome
    {
        bool AddData(Models.HomeNursing homeNursing);

        List<HomeNursingView> GetData(DateTime startDate, DateTime endDate);

        bool UpdateData(Models.HomeNursing obj);

        bool DeleteData(int id);
    }
}
