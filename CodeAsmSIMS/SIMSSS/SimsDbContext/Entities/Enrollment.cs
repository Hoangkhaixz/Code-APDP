using System;
using System.ComponentModel.DataAnnotations;

namespace SIMSS.SimsDbContext.Entities
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentID { get; set; }
        public int StudentID { get; set; }
        public int CourseID { get; set; }
        public DateTime EnrollmentDate { get; set; }
        [StringLength(50)]
        public string? Class { get; set; }
        [StringLength(10)]
        public string? Grade { get; set; }
    }
}
