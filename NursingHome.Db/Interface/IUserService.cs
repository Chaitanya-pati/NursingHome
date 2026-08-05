using System;
using System.Collections.Generic;
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
        dynamic GetFaceDescriptor(string Username);
        Users CheckValidUser(string userName, string password);
        Users GetUserDataById(int id);

        // ── Extended methods ──────────────────────────────────────────────────
        Users GetUserByUsername(string username);

        /// <summary>
        /// Creates a new user with uniqueness validation.
        /// Returns (success, errorMessage).
        /// </summary>
        (bool success, string error) CreateUserWithValidation(Users user, string checkNotAssignedToOtherHelper, List<string> otherHelperSusers);

        /// <summary>
        /// Validates whether the given user is already assigned as suser to another helper.
        /// </summary>
        bool IsUserAlreadyAssigned(string userName, List<string> allHelperSusers);

        bool UpdatePassword(string userName, string newPassword);
        void RecordLogin(string userName);

        /// <summary>
        /// Returns the first available username derived from <paramref name="firstName"/>.
        /// Tries firstName → firstName2 → firstName3 … until one is not taken.
        /// </summary>
        string SuggestUsername(string firstName);
    }
}
