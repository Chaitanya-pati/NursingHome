using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NursingHome.Db.Models;

namespace NursingHome.Db.Interface
{
    public interface IHelpers
    {
        bool AddData(Helpers helpers);

        bool UpdateData(Models.Helpers helperData);
        List<Models.Helpers> GetData(string username);
        bool DeleteData(int id);
    }
}
