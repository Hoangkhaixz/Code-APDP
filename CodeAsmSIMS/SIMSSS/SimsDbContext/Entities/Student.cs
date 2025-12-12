using System;
using System.ComponentModel.DataAnnotations;

namespace SIMSS.SimsDbContext.Entities
{
    public class Student
    {
        [Key]
        public int StudentID { get; set; }
        public int UserID { get; set; }
        public int? FacultyID { get; set; }
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        [StringLength(10)]
        public string Gender { get; set; }
        [StringLength(100)]
        public string Program { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}
