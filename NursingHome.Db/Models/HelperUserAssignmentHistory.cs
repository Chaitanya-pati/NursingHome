using System;

namespace NursingHome.Db.Models;

public class HelperUserAssignmentHistory
{
    public int Id { get; set; }
    public int HelperId { get; set; }
    public string AssignedUserName { get; set; }

    /// <summary>'Assigned' | 'Changed' | 'Removed' | 'PasswordReset'</summary>
    public string Action { get; set; }

    public string AssignedBy { get; set; }
    public DateTime? AssignedDate { get; set; }
    public string RemovedBy { get; set; }
    public DateTime? RemovedDate { get; set; }
    public string Notes { get; set; }
}
