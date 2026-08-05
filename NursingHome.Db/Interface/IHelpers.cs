using System;
using System.Collections.Generic;
using NursingHome.Db.Models;

namespace NursingHome.Db.Interface
{
    public interface IHelpers
    {
        bool AddData(Helpers helpers);
        bool UpdateData(Models.Helpers helperData);
        List<Models.Helpers> GetData(string username);
        bool DeleteData(int id);

        // ── User assignment ───────────────────────────────────────────────────
        bool AssignUser(int helperId, string userName);
        bool RemoveUserAssignment(int helperId);
        bool ClearAllUserAssignments();

        // ── Assignment history / audit ────────────────────────────────────────
        void RecordAssignmentHistory(HelperUserAssignmentHistory entry);
        List<HelperUserAssignmentHistory> GetAssignmentHistory(int helperId);
    }
}
