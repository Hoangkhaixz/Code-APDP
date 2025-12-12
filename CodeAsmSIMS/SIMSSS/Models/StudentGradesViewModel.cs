using System;
using System.ComponentModel.DataAnnotations;

namespace SIMSS.Models
{
    public class StudentGradesViewModel
    {
        public int EnrollmentID { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string? Class { get; set; }
        public string? Grade { get; set; }
    }
}
