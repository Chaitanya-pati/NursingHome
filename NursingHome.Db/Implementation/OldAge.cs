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

        public int AddData(Models.OldAge oldAge)
        {
            try
            {
                using var Db = new TaskContext(_dbConn);
                oldAge.CreatedDate = DateTime.Now;
                oldAge.UpdatedDate = DateTime.Now;

                Db.Add(oldAge);
                Db.SaveChanges();
                return oldAge.Id;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public List<Models.OldAge> GetData(DateTime startDate, DateTime endDate, string username)
        {
            using var Db = new TaskContext(_dbConn);

            var query = @"
    SELECT *
    FROM OldAge
    WHERE AdmissionDate BETWEEN @startDate AND @endDate 
    AND (@username = '' OR SUSer = @username)";

            List<Models.OldAge> result = new List<Models.OldAge>();
            result = Db.OldAge.FromSqlRaw(query,
                new SqlParameter("@startDate", startDate),
                new SqlParameter("@endDate", endDate),
                new SqlParameter("@username", string.IsNullOrWhiteSpace(username) ? "" : username)
            ).ToList();

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
                GetData.clientSign = oldAge.clientSign;
                GetData.authorizedSign = oldAge.authorizedSign;
                GetData.AdmissionNo = oldAge.AdmissionNo;
                GetData.ResidentIdNo = oldAge.ResidentIdNo;
                GetData.RoomBedNo = oldAge.RoomBedNo;
                GetData.AdmissionType = oldAge.AdmissionType;
                GetData.ExpectedDurationOfStay = oldAge.ExpectedDurationOfStay;
                GetData.FatherHusbandWifeName = oldAge.FatherHusbandWifeName;
                GetData.DateOfBirth = oldAge.DateOfBirth;
                GetData.Gender = oldAge.Gender;
                GetData.AadhaarNo = oldAge.AadhaarNo;
                GetData.OtherIdProof = oldAge.OtherIdProof;
                GetData.CurrentAddress = oldAge.CurrentAddress;
                GetData.MaritalStatus = oldAge.MaritalStatus;
                GetData.NativePlace = oldAge.NativePlace;
                GetData.MotherTongue = oldAge.MotherTongue;
                GetData.Religion = oldAge.Religion;
                GetData.GuardianName = oldAge.GuardianName;
                GetData.GuardianRelationship = oldAge.GuardianRelationship;
                GetData.GuardianAddress = oldAge.GuardianAddress;
                GetData.GuardianMobileNo = oldAge.GuardianMobileNo;
                GetData.AlternateMobileNo = oldAge.AlternateMobileNo;
                GetData.GuardianEmail = oldAge.GuardianEmail;
                GetData.GuardianIdProof = oldAge.GuardianIdProof;
                GetData.LegalGuardianName = oldAge.LegalGuardianName;
                GetData.LegalGuardianRelationship = oldAge.LegalGuardianRelationship;
                GetData.EmergencyContactName = oldAge.EmergencyContactName;
                GetData.EmergencyRelationship = oldAge.EmergencyRelationship;
                GetData.EmergencyMobileNo = oldAge.EmergencyMobileNo;
                GetData.EmergencyAlternateNo = oldAge.EmergencyAlternateNo;
                GetData.NearestRelativeName = oldAge.NearestRelativeName;
                GetData.NearestRelativeRelationship = oldAge.NearestRelativeRelationship;
                GetData.PresentHealthCondition = oldAge.PresentHealthCondition;
                GetData.MedicalConditions = oldAge.MedicalConditions;
                GetData.OtherMedicalCondition = oldAge.OtherMedicalCondition;
                GetData.Allergies = oldAge.Allergies;
                GetData.CurrentMedicines = oldAge.CurrentMedicines;
                GetData.DoctorName = oldAge.DoctorName;
                GetData.HospitalClinic = oldAge.HospitalClinic;
                GetData.DoctorContactNo = oldAge.DoctorContactNo;
                GetData.BloodGroup = oldAge.BloodGroup;
                GetData.Mobility = oldAge.Mobility;
                GetData.AssistanceRequired = oldAge.AssistanceRequired;
                GetData.OtherAssistance = oldAge.OtherAssistance;
                GetData.MentalMemoryConditions = oldAge.MentalMemoryConditions;
                GetData.MedicalDocumentsAttached = oldAge.MedicalDocumentsAttached;
                GetData.MedicalDocuments = oldAge.MedicalDocuments;
                GetData.PreferredHospital = oldAge.PreferredHospital;
                GetData.HospitalContact = oldAge.HospitalContact;
                GetData.InsuranceHealthScheme = oldAge.InsuranceHealthScheme;
                GetData.PolicyIdNo = oldAge.PolicyIdNo;
                GetData.EmergencyConsent = oldAge.EmergencyConsent;
                GetData.ServicesSelected = oldAge.ServicesSelected;
                GetData.OtherService = oldAge.OtherService;
                GetData.SpecialCareInstructions = oldAge.SpecialCareInstructions;
                GetData.FoodCharges = oldAge.FoodCharges;
                GetData.NursingCareCharges = oldAge.NursingCareCharges;
                GetData.OtherCharges = oldAge.OtherCharges;
                GetData.TotalMonthlyCharges = oldAge.TotalMonthlyCharges;
                GetData.AdvancePaid = oldAge.AdvancePaid;
                GetData.PaymentMode = oldAge.PaymentMode;
                GetData.PaymentReference = oldAge.PaymentReference;
                GetData.PaymentNotes = oldAge.PaymentNotes;
                GetData.DeclarationAccepted = oldAge.DeclarationAccepted;
                GetData.SignerName = oldAge.SignerName;
                GetData.SignerRelationship = oldAge.SignerRelationship;
                GetData.SignedDate = oldAge.SignedDate;
                GetData.StaffRemarks = oldAge.StaffRemarks;
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


        public int getLatestID()
        {
            var Db = new TaskContext(_dbConn);
            int maxId = Db.OldAge.AsEnumerable().Select(o => o.Id).DefaultIfEmpty(0).Max();
            return maxId == 0 ? 1 : maxId + 1;
        }

        public bool UpdateClientSignature(int id, string signature)
        {
            using var Db = new TaskContext(_dbConn);
            var record = Db.OldAge.FirstOrDefault(x => x.Id == id);
            if (record != null)
            {
                record.clientSign = signature;
                Db.SaveChanges();
                return true;
            }
            return false;
        }


        public Models.OldAge GetPatientData(int patientId)
        {
            using var Db = new TaskContext(_dbConn);
            var data = Db.OldAge.Where(x => x.Id == patientId).FirstOrDefault();

            return data;
        }
    }
}
