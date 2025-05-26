namespace CgmscHO_API.AttendenceDTO
{
    public class AttendenceRecordDTO
    {
           // Employee Details
            public int? EmployeeId { get; set; }
            public string? EmployeeName { get; set; }
            public string? EmployeeCode { get; set; }
            public string? Gender { get; set; }
            public string? ContactNo { get; set; }
            public string? DesignationsName { get; set; }
            public string? DepartmentFName { get; set; }

            // Attendance Details
            public DateTime? AttendanceDate { get; set; }
            public string? AttendanceDateStr { get; set; }
            public string? InTime { get; set; }
            public string? OutTime { get; set; }
            public string? PunchDirections { get; set; }
            public string? Status { get; set; }
            public string? StatusCode { get; set; }
            public string? ReportPunchRecords { get; set; }
            public string? TotalDurationInHHMM { get; set; }
       

    }
}
