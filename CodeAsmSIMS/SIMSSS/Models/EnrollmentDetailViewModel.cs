using System;
using System.ComponentModel.DataAnnotations;

namespace SIMSS.Models
{
    public class EnrollmentDetailViewModel
    {
        public int EnrollmentID { get; set; }
        public int StudentID { get; set; }
        public int CourseID { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string? Class { get; set; }
        public string? Grade { get; set; }
        public string StudentName { get; set; }
    }
}
