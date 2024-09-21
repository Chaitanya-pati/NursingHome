using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NursingHome.Db.Models;

namespace NursingHome.Db.Interface
{
    public interface IUserService
    {
        bool AddData(Users users);
        bool UpdateData(Users user);
        List<Users> GetData();
        bool DeleteUser(int id);
        bool SaveFaceDescriptor(string username, string faceid);

        string GetFaceDescriptor(string Username);
    }
}
